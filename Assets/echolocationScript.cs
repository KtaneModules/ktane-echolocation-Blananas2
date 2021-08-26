using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class echolocationScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] moves; //ULDR
    public KMSelectable center;

    public Material white;
    public GameObject actualModule;
    public GameObject actualButtons;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private int size = 4;

    private string[,] maze;
    string smaze = "";
    string xmaze = "";

    private List<string> directionNames = new List<string> {"North", "West", "South", "East" };
    string symbols = "─│┌┐└┘├┤┬┴┼˂˃˄˅";
    private List<string> validMoves = new List<string> {"X.XX....X..XX.X", ".XX.X.X.....XXX", "X...XX...X.XXX.", ".X.X.X.X...X.XX" };
    int playerPos = -1;
    int keyPos = -1;
    int exitPos = -1;
    int direction = -1; // u l d r [+1 = counterclockwise or turn left, -1 clockwise or turn right]
    char tile = '?';
    int tilePlace = -1;
    bool keyGet = false;
    bool playingSound = false;

    private Coroutine buttonHold;
	private bool holding = false;

    private Coroutine startEcho;
	private bool echoing = false;
    int halfSeconds = -1;
    bool hitWall = false;
    char echoTile = '?';
    int echoPos = -1;
    int echoPlace = -1;

    void Awake () {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable move in moves) {
            KMSelectable pressedMove = move;
            move.OnInteract += delegate () { movePress(pressedMove); return false; };
        }

        center.OnInteract += delegate () { CenterPress(); return false; };
        center.OnInteractEnded += delegate { CenterRelease(); };

    }

    // Use this for initialization
    void Start () {
        if (size < 2 || size > 26) {
            size = 4;
        }

        playerPos = UnityEngine.Random.Range(0, size*size);
        keyPos = UnityEngine.Random.Range(0, size*size);
        exitPos = UnityEngine.Random.Range(0, size*size);
        direction = UnityEngine.Random.Range(0, 4);
        if (keyPos == exitPos) {
            exitPos = (size*size-1) - keyPos;
        }

        maze = MazeGenerator.GenerateMaze(size, size);
        smaze = MazeToString(maze);
        xmaze = smaze.Replace("\n", "");
        Debug.LogFormat("[Echolocation #{0}] Maze:\n{1}", moduleId, smaze);
        Debug.LogFormat("[Echolocation #{0}] # THE MAZE STOPS HERE #", moduleId);

        Debug.LogFormat("[Echolocation #{0}] Player Position: {1}", moduleId, LocationName(playerPos));
        Debug.LogFormat("[Echolocation #{0}] Key Position: {1}", moduleId, LocationName(keyPos));
        Debug.LogFormat("[Echolocation #{0}] Exit Position: {1}", moduleId, LocationName(exitPos));
        Debug.LogFormat("[Echolocation #{0}] Player Direction: {1}", moduleId, directionNames[direction]);
        Debug.LogFormat("[Echolocation #{0}] Moves:", moduleId);
	}

    void movePress (KMSelectable move) {
        if (playingSound == false) {
            move.AddInteractionPunch();
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (move == moves[0]) { //U
                switch (direction) {
                    case 0: //u
                        if (playerPos < size) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go north from {1}, STRIKE!", moduleId, LocationName(playerPos));
                        } else {
                            tile = xmaze[playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos -= size;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, LocationName(playerPos));
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go north from {1}, STRIKE!", moduleId, LocationName(playerPos));
                            }
                        }
                        break;
                    case 1: //l
                        if (playerPos % size == 0) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go west from {1}, STRIKE!", moduleId, LocationName(playerPos));
                        } else {
                            tile = xmaze[playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos -= 1;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, LocationName(playerPos));
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go west from {1}, STRIKE!", moduleId, LocationName(playerPos));
                            }
                        }
                        break;
                    case 2: //d
                        if ((size*(size-1))-1  < playerPos) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go south from {1}, STRIKE!", moduleId, LocationName(playerPos));
                        } else {
                            tile = xmaze[playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos += size;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, LocationName(playerPos));
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go south from {1}, STRIKE!", moduleId, LocationName(playerPos));
                            }
                        }
                        break;
                    case 3: //r
                        if (playerPos % size == size-1) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go east from {1}, STRIKE!", moduleId, LocationName(playerPos));
                        } else {
                            tile = xmaze[playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos += 1;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, LocationName(playerPos));
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go east from {1}, STRIKE!", moduleId, LocationName(playerPos));
                            }
                        }
                        break;
                    default:
                    Debug.LogFormat("[Echolocation #{0}] Bug found, let Blan know immediately. (movePress reached the bottom of up switch statement)", moduleId);
                        break;
                }
            } else if (move == moves[1]) { //L
                direction = (direction + 1) % 4;
                Debug.LogFormat("[Echolocation #{0}] L) Now facing {1}", moduleId, directionNames[direction]);
            } else if (move == moves[2]) { //D
                direction = (direction + 2) % 4;
                Debug.LogFormat("[Echolocation #{0}] D) Now facing {1}", moduleId, directionNames[direction]);
            } else if (move == moves[3]) { //R
                direction = (direction + 3) % 4;
                Debug.LogFormat("[Echolocation #{0}] R) Now facing {1}", moduleId, directionNames[direction]);
            } else {
                Debug.LogFormat("[Echolocation #{0}] Bug found, let Blan know immediately. (movePress reached the bottom of if statement)", moduleId);
            }
        }
    }

    void CenterPress () {
        center.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        hitWall = false;
        echoPos = playerPos;
        echoTile = xmaze[echoPos];
        halfSeconds = 0;
        startEcho = StartCoroutine(Echo());

        if (buttonHold != null)
		{
			holding = false;
			StopCoroutine(buttonHold);
			buttonHold = null;
		}

		buttonHold = StartCoroutine(HoldChecker());

    }

    void CenterRelease () {
        StopCoroutine(buttonHold);
    }

    IEnumerator HoldChecker()
	{
		yield return new WaitForSeconds(.4f);
        StopCoroutine(startEcho);
        playingSound = false;
		holding = true;
        if (playerPos == keyPos) {
            keyGet = true;
            keyPos = -1;
            Debug.LogFormat("[Echolocation #{0}] C HOLD) You are at the key and you've picked it up.", moduleId);
            center.AddInteractionPunch();
        } else if (playerPos == exitPos) {
            if (keyGet == true) {
                Debug.LogFormat("[Echolocation #{0}] C HOLD) You are at the exit and you have the key. MODULE SOLVED!", moduleId);
                actualModule.GetComponent<MeshRenderer>().material = white;
                moves[0].GetComponent<MeshRenderer>().material = white;
                moves[1].GetComponent<MeshRenderer>().material = white;
                moves[2].GetComponent<MeshRenderer>().material = white;
                moves[3].GetComponent<MeshRenderer>().material = white;
                center.GetComponent<MeshRenderer>().material = white;
                actualButtons.gameObject.SetActive(false);
                GetComponent<KMBombModule>().HandlePass();
                Audio.PlaySoundAtTransform("win", transform);
            } else {
                Debug.LogFormat("[Echolocation #{0}] C HOLD) You are at the exit but you don't have the key. STRIKE!", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
        } else {
            Debug.LogFormat("[Echolocation #{0}] C HOLD) You are not at the key nor the exit. STRIKE!", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }

    IEnumerator Echo() {
        if (!playingSound) {
            playingSound = true;
        } else {
            yield return null;
        }
        while (hitWall == false) {
            if (halfSeconds % 2 == 0 && halfSeconds != 0) {
                switch (direction) {
                    case 0: //u
                        echoPos = echoPos - size;
                        echoTile = xmaze[echoPos];
                        break;
                    case 1: //l
                        echoPos = echoPos - 1;
                        echoTile = xmaze[echoPos];
                        break;
                    case 2: //d
                        echoPos = echoPos + size;
                        echoTile = xmaze[echoPos];
                        break;
                    case 3: //r
                        echoPos = echoPos + 1;
                        echoTile = xmaze[echoPos];
                        break;
                    default:
                    Debug.LogFormat("[Echolocation #{0}] Bug found, let Blan know immediately. (Echo coroutine reached the bottom of direction switch statement)", moduleId);
                    break;
                }
            }

            if (halfSeconds % 2 == 0) { //OBJECTS
                if (echoPos == keyPos) {
                    Audio.PlaySoundAtTransform("key", transform);
                } else if (echoPos == exitPos) {
                    Audio.PlaySoundAtTransform("exit", transform);
                }
            } else { //WALLS
                echoPlace = symbols.IndexOf(echoTile);
                if (validMoves[direction][echoPlace] == 'X') {
                    Audio.PlaySoundAtTransform("wall", transform);
                    hitWall = true;
                    yield return new WaitForSeconds(3f);
                    playingSound = false;
                }
            }

            halfSeconds += 1;
            yield return new WaitForSeconds(.5f);
        }
    }

    //KINGBRANBRAN'S CODE BY THE WAY
    private string MazeToString(string[,] maze)
	{
		var mazeString = "";
		for (var h = 0; h < maze.GetLength(1); h++)
		{
			for (var w = 0; w < maze.GetLength(0); w++)
			{
				mazeString += maze[w, h] + " ";
			}

			mazeString += "\n";
		}

        mazeString = BetterString(mazeString); //ok this is my code though, you can tell bc it sucks

		return mazeString;
	}

    string BetterString (string s) {
        s = s.Replace("N", "˄");
        s = s.Replace("E", "˃");
        s = s.Replace("W", "˂");
        s = s.Replace("S", "˅");
        s = s.Replace("˂˃", "─");
        s = s.Replace("˃˂", "─");
        s = s.Replace("˄˅", "│");
        s = s.Replace("˅˄", "│");
        s = s.Replace("˃˅", "┌");
        s = s.Replace("˅˃", "┌");
        s = s.Replace("˂˅", "┐");
        s = s.Replace("˅˂", "┐");
        s = s.Replace("˃˄", "└");
        s = s.Replace("˄˃", "└");
        s = s.Replace("˂˄", "┘");
        s = s.Replace("˄˂", "┘");
        s = s.Replace("─˄", "┴");
        s = s.Replace("˄─", "┴");
        s = s.Replace("─˅", "┬");
        s = s.Replace("˅─", "┬");
        s = s.Replace("│˂", "┤");
        s = s.Replace("˂│", "┤");
        s = s.Replace("│˃", "├");
        s = s.Replace("˃│", "├");
        s = s.Replace("┌˄", "├");
        s = s.Replace("˄┌", "├");
        s = s.Replace("┌˂", "┬");
        s = s.Replace("˂┌", "┬");
        s = s.Replace("┐˄", "┤");
        s = s.Replace("˄┐", "┤");
        s = s.Replace("┐˃", "┬");
        s = s.Replace("˃┐", "┬");
        s = s.Replace("└˂", "┴");
        s = s.Replace("˂└", "┴");
        s = s.Replace("└˅", "├");
        s = s.Replace("˅└", "├");
        s = s.Replace("┘˅", "┤");
        s = s.Replace("˅┘", "┤");
        s = s.Replace("┘˃", "┴");
        s = s.Replace("˃┘", "┴");
        s = s.Replace("˂├", "┼");
        s = s.Replace("├˂", "┼");
        s = s.Replace("˄┬", "┼");
        s = s.Replace("┬˄", "┼");
        s = s.Replace("˃┤", "┼");
        s = s.Replace("┤˃", "┼");
        s = s.Replace("˅┴", "┼");
        s = s.Replace("┴˅", "┼");
        s = s.Replace(" ", "");
        return s;
    }

    string LocationName (int x) {
        List<string> colLet = new List<string> {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
        List<string> rowNum = new List<string> {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26"};

        int c = x/size;
        int r = x%size;

        return colLet[r] + "" + rowNum[c];
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} u/d/l/r/c [Presses the button(s) in the specified position(s)] | !{0} hold/h [Holds the center button]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*hold\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*h\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            center.OnInteract();
            yield return new WaitForSeconds(1f);
            center.OnInteractEnded();
            yield break;
        }

        string[] valids = { "u", "l", "d", "r", "c", "f", "useless", "b"};
        command = command.Replace(" ","");
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
                yield return "trycancel The command is cancelled during move #" + (i+1) +".";
                if (command.Length > 1) yield return "strikemessage input #" + (i+1);
                center.OnInteract();
                center.OnInteractEnded();
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                yield return null;
                yield return "trycancel The command is cancelled during move #" + (i+1) +".";
                if (command.Length > 1) yield return "strikemessage input #" + (i+1);
                moves[Array.IndexOf(valids, command.ElementAt(i) + "") % 5].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
            while (playingSound) {
                yield return new WaitForSeconds(.1f);
            }
        }
        yield break;
    }

}
