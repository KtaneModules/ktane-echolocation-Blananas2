using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using System.Reflection;
using Newtonsoft.Json;

public class echolocationScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] MoveSels;
    public KMSelectable Center;
    public Material WhiteMat;
    public GameObject ModuleObj;
    public GameObject ButtonParentObj;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private const int _defaultSize = 4;
    private int _size;
    private static readonly string[] _directionNames = new string[4] { "North", "East", "South", "West" };
    private MazeGenerator _mazeGenerator;
    private string _mazeString;
    private int _direction;
    private Coroutine _holdCheck;
    private int _currentPosition;
    private int _keyPosition;
    private int _exitPosition;
    private bool _keyCollected;

    public class ModSettingsJSON
    {
        public int size;
    }
    public KMModSettings ModSettings;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;

        Center.OnInteract += CenterPress;
        Center.OnInteractEnded += CenterRelease;
        for (int btn = 0; btn < MoveSels.Length; btn++)
            MoveSels[btn].OnInteract += MovePress(btn);

        _size = GetSize();

        _mazeGenerator = new MazeGenerator(_size);
        _mazeString = _mazeGenerator.GenerateMaze();

        _direction = Rnd.Range(0, 4);
        var positions = Enumerable.Range(0, _size * _size).ToArray().Shuffle().Take(3).Select(i => GetTransformedPosition(i)).ToArray();
        _currentPosition = positions[0];
        _keyPosition = positions[1];
        _exitPosition = positions[2];
        var tempMazeArr = _mazeString.ToCharArray();
        tempMazeArr[_keyPosition] = 'k';
        tempMazeArr[_exitPosition] = 'e';
        _mazeString = tempMazeArr.Join("");
        LogMaze();
        Debug.LogFormat("[Echolocation #{0}] Your starting position is at {1} and you are facing {2}.", _moduleId, GetCoord(_currentPosition), _directionNames[_direction]);
    }

    private bool CenterPress()
    {
        if (_moduleSolved)
            return false;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Center.AddInteractionPunch(0.5f);
        StartCoroutine(PlayNoises());
        if (_holdCheck != null)
            StopCoroutine(_holdCheck);
        _holdCheck = StartCoroutine(HoldCheck());
        return false;
    }

    private void CenterRelease()
    {
        if (_moduleSolved)
            return;
        if (_holdCheck != null)
            StopCoroutine(_holdCheck);
    }

    private KMSelectable.OnInteractHandler MovePress(int btn)
    {
        return delegate ()
        {
            if (_moduleSolved)
                return false;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            MoveSels[btn].AddInteractionPunch(0.5f);
            if (btn != 0)
            {
                _direction = (_direction + btn) % 4;
                Debug.LogFormat("[Echolocation #{0}] {1}) Now facing {2}.", _moduleId, "LDR"[btn - 1], _directionNames[_direction]);
                return false;
            }
            var validMove = CheckMovement(_currentPosition, _direction);
            if (!validMove)
            {
                Debug.LogFormat("[Echolocation #{0}] U) Attempted to move {1}, but there's a wall there! Strike.", _moduleId, _directionNames[_direction]);
                Module.HandleStrike();
                return false;
            }
            var oldPos = _currentPosition;
            _currentPosition += 2 * GetMoveOffset(_direction);
            Debug.LogFormat("[Echolocation #{0}] U) Moved {1} from {2} to {3}.", _moduleId, _directionNames[_direction], GetCoord(oldPos), GetCoord(_currentPosition));
            return false;
        };
    }

    private string GetCoord(int pos)
    {
        return "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[(pos - (_size * 2 + 1)) / 2 % (_size * 2 + 1)].ToString() + (((pos / (_size * 2 + 1)) - 1) / 2 + 1).ToString();
    }

    private void LogMaze()
    {
        var maze = new string[(_size * 2 + 1)];
        for (int i = 0; i < maze.Length; i++)
            maze[i] = _mazeString.Substring(i * (_size * 2 + 1), _size * 2 + 1);
        Debug.LogFormat("[Echolocation #{0}] Maze: (Key = k, Exit = e)", _moduleId);
        for (int i = 0; i < maze.Length; i++)
            Debug.LogFormat("[Echolocation #{0}] {1}", _moduleId, maze[i]);
    }

    private IEnumerator HoldCheck()
    {
        yield return new WaitForSeconds(0.4f);
        CenterHold();
    }

    private void CenterHold()
    {
        if (_mazeString[_currentPosition] == 'k')
        {
            var tempMazeArr = _mazeString.ToCharArray();
            tempMazeArr[_keyPosition] = ' ';
            _mazeString = tempMazeArr.Join("");
            _keyCollected = true;
            Debug.LogFormat("[Echolocation #{0}] You are at the key and you've picked it up.", _moduleId);
            Center.AddInteractionPunch(0.5f);
            return;
        }
        if (_mazeString[_currentPosition] == 'e')
        {
            if (_keyCollected)
            {
                ButtonParentObj.SetActive(false);
                Module.HandlePass();
                _moduleSolved = true;
                ModuleObj.GetComponent<MeshRenderer>().material = WhiteMat;
                _moduleSolved = true;
                Audio.PlaySoundAtTransform("win", transform);
                Debug.LogFormat("[Echolocation #{0}] You are at the exit and you have the key. MODULE SOLVED!", _moduleId);
                return;
            }
            Module.HandleStrike();
            Debug.LogFormat("[Echolocation #{0}] You are at the exit but you don't have the key. STRIKE!", _moduleId);
            return;
        }
        Debug.LogFormat("[Echolocation #{0}] You are not at the key nor the exit. STRIKE!", _moduleId);
        Module.HandleStrike();
    }

    private int GetMoveOffset(int dir)
    {
        return
            dir == 0 ? -1 * (_size * 2 + 1) :
            dir == 1 ? 1 :
            dir == 2 ? (_size * 2 + 1) :
            -1;
    }

    private int GetTransformedPosition(int pos)
    {
        return (pos / _size * (_size * 2 + 1) * 2) + (pos % _size * 2) + _size * 2 + 2;
    }

    private bool CheckMovement(int pos, int dir)
    {
        return _mazeString[pos + GetMoveOffset(dir)] != '█';
    }

    private IEnumerator PlayNoises()
    {
        Debug.LogFormat("[Echolocation #{0}] Played a noise from {1} while facing {2}.", _moduleId, GetCoord(_currentPosition), _directionNames[_direction]);
        var index = _currentPosition;
        var offset = GetMoveOffset(_direction);
        while (true)
        {
            if (_mazeString[index] == 'k')
                Audio.PlaySoundAtTransform("key", transform);
            if (_mazeString[index] == 'e')
                Audio.PlaySoundAtTransform("exit", transform);
            if (_mazeString[index] == '█')
            {
                Audio.PlaySoundAtTransform("wall", transform);
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
            index += offset;
        }
    }

    private int GetSize()
    {
        string missionId = GetMissionID();
        if (missionId != "undefined" && missionId != "custom")
        {
            Debug.LogFormat("[Echolocation #{0}] Mission '{1}' detected. Generating maze with default size of {2}.", _moduleId, missionId, _defaultSize);
            return 4;
        }
        try
        {
            ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(ModSettings.Settings);
            if (settings != null)
            {
                if (settings.size < 2)
                {
                    Debug.LogFormat("[Echolocation #{0}] Maze size cannot be less than 2. Generating maze with default size of {1}.", _moduleId, _defaultSize);
                    return _defaultSize;
                }
                if (settings.size > 26)
                {
                    Debug.LogFormat("[Echolocation #{0}] Maze size cannot be greater than 26. Generating maze with default size of {1}.", _moduleId, _defaultSize);
                    return _defaultSize;
                }
                Debug.LogFormat("[Echolocation #{0}] Generating maze with size of {1}.", _moduleId, settings.size);
                return settings.size;
            }
            Debug.LogFormat("[Echolocation #{0}] Generating maze with default size of {1}.", _moduleId, _defaultSize);
            return _defaultSize;
        }
        catch (JsonReaderException e)
        {
            Debug.LogFormat("[Echolocation #{0}] JSON reading failed with error: {1}. Generating maze with default size of {2}.", _moduleId, e.Message, _defaultSize);
            return _defaultSize;
        }
    }

    private string GetMissionID()
    {
        try
        {
            Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
            Type type = gameplayState.GetType();
            FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
            return fieldMission.GetValue(gameplayState).ToString();
        }
        catch (NullReferenceException)
        {
            return "undefined";
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} u/d/l/r/c [Presses the button(s) in the specified position(s)] | !{0} hold/h [Holds the center button]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*hold\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*h\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Center.OnInteract();
            yield return new WaitForSeconds(1f);
            Center.OnInteractEnded();
            yield break;
        }

        string[] valids = { "u", "l", "d", "r", "c", "f", "useless", "b" };
        command = command.Replace(" ", "");
        command = command.ToLower();
        for (int i = 0; i < command.Length; i++)
        {
            if (!valids.Contains(command.ElementAt(i) + ""))
            {
                yield return "sendtochaterror The specified position '" + command.ElementAt(i) + "' for a button is not valid!";
                yield break;
            }
        }
        for (int i = 0; i < command.Length; i++)
        {
            if (command.ElementAt(i) == 'c')
            {
                yield return null;
                yield return "trycancel The command is cancelled during move #" + (i + 1) + ".";
                if (command.Length > 1) yield return "strikemessage input #" + (i + 1);
                Center.OnInteract();
                yield return null;
                Center.OnInteractEnded();
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                yield return null;
                yield return "trycancel The command is cancelled during move #" + (i + 1) + ".";
                if (command.Length > 1) yield return "strikemessage input #" + (i + 1);
                MoveSels[Array.IndexOf(valids, command.ElementAt(i) + "") % 5].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
        yield break;
    }

    struct QueueItem
    {
        public int Cell;
        public int Parent;
        public int Direction;
        public QueueItem(int cell, int parent, int dir)
        {
            Cell = cell;
            Parent = parent;
            Direction = dir;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for (int stage = 0; stage < 2; stage++)
        {
            if (_keyCollected && stage == 0)
                continue;
            var visited = new Dictionary<int, QueueItem>();
            var q = new Queue<QueueItem>();
            var sol = new[] { _keyPosition, _exitPosition }[stage];
            q.Enqueue(new QueueItem(_currentPosition, -1, 0));
            while (q.Count > 0)
            {
                var qi = q.Dequeue();
                if (visited.ContainsKey(qi.Cell))
                    continue;
                visited[qi.Cell] = qi;
                if (qi.Cell == sol)
                    break;
                if (CheckMovement(qi.Cell, 0))
                    q.Enqueue(new QueueItem(qi.Cell - 2 * (2 * _size + 1), qi.Cell, 0));
                if (CheckMovement(qi.Cell, 1))
                    q.Enqueue(new QueueItem(qi.Cell + 2, qi.Cell, 1));
                if (CheckMovement(qi.Cell, 2))
                    q.Enqueue(new QueueItem(qi.Cell + 2 * (2 * _size + 1), qi.Cell, 2));
                if (CheckMovement(qi.Cell, 3))
                    q.Enqueue(new QueueItem(qi.Cell - 2, qi.Cell, 3));
            }
            var r = sol;
            var path = new List<int>();
            while (true)
            {
                var nr = visited[r];
                if (nr.Parent == -1)
                    break;
                path.Add(nr.Direction);
                r = nr.Parent;
            }
            path.Reverse();
            for (int i = 0; i < path.Count; i++)
            {
                if (_direction == (path[i] + 1) % 4)
                    MoveSels[3].OnInteract();
                if (_direction == (path[i] + 2) % 4)
                    MoveSels[2].OnInteract();
                if (_direction == (path[i] + 3) % 4)
                    MoveSels[1].OnInteract();
                yield return new WaitForSeconds(0.2f);
                MoveSels[0].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
            Center.OnInteract();
            yield return new WaitForSeconds(1f);
            Center.OnInteractEnded();
            yield return new WaitForSeconds(0.2f);
        }
    }
}
