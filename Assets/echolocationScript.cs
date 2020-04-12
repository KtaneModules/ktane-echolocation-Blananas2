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

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    public List<string> mazes = new List<string> { "┌─┐┌─˂│┌┘└─┐│└┐┌─┤│˃┴┘˃┤├─┐┌˂│└˂└┘˃┘", "˃┬˂┌┬˂┌┘┌┘└┐│┌┘┌─┤├┘┌┘˅││˅│┌┘│˄└┘└─┘", "┌─┐˅┌┐˄˅│└┘│┌┤│┌┐││││││││└┘│││└──┘└┘", "┌┐˃──┐││┌──┤│└┘┌˂││˃─┴─┤├───┐│└─˂˃┘˄", "˃───┬┐┌──┬┘˄├┐˃┘┌┐│└─┐˄││┌─┴˂│˄└───┘", "˅┌┐˃┬┐│││┌┘│├┘˄│┌┘└┐┌┤│˅┌┘˄│└┤└──┘˃┘", "┌──┐┌┐│┌˂└┘│└┘┌˂┌┘┌┐├─┘˅│˄└─┐│└───┴┘", "˅┌─┐┌┐├┴˂└┘││┌──┐││└┐˃┴┘│˅└──˂└┴───˂", "˅┌──┬┐││┌˂││├┴┘┌┘││˅┌┘˃┤│││┌┐˄└┘└┘└˂", "┌──┬─┐│┌┐│┌┘└┘˄││˅┌┐┌┘└┤│└┘˃┐│└───┘˄", "˃─┬┬─┐┌┐││┌┘││˄│└˂│└┐└─┐│˃┴──┤└──˂˃┘", "┌────┐└┐┌─˂│┌┘└┐┌┤└─┐││˄┌˂└┤└┐└──┴˂˄", "┌┐┌┬─˂˄└┤└─┐┌┐˄┌─┤│└┬┘˅│├┐˄┌┘│˄└─┘˃┘", "┌─┐˃┬┐│˅└┐│˄│└┬┘└┐└┐└─┐│˅│˅┌┘│└┘└┴─┘", "┌────┐│˃─┬┐││┌─┤˄│└┘┌┘˅│┌˂└┐└┘└──┴─˂", "┌───┬┐˄┌─┐│˄┌┘˅│├┐│┌┤│˄││˄│└┐│└˂└─┴┘", "┌─┬┐┌┐└˂│└┘│┌─┘┌─┘│┌┐│┌˂│││˄│˅└┘└─┴┘", "┌┐˃┐┌┐˄└┐└┘│┌┐└──┤│└˂┌┐│├┐┌┘││˄└┴˂└┘" };
    public List<string> niceMazes = new List<string> { "██████████████     █     ██ ███ █ ██████ █   █     ██ █ ███████ ██ █   █     ██ ███ █ ███ ██ █     █   ██ █████████ ██     █   █ ██ ███ █ ███ ██   █   █   ██████████████", "██████████████     █     ████ ███ █ ████   █   █   ██ ███ █████ ██ █   █     ██ █ ███ ███ ██   █   █ █ ██ ███ ███ █ ██ █ █ █   █ ██ █ █ █ ███ ██ █   █     ██████████████", "██████████████     █ █   ██ ███ █ █ █ ██ █ █ █   █ ████ █ █████ ██   █ █   █ ██ █ █ █ █ █ ██ █ █ █ █ █ ██ █ █ █ █ █ ██ █   █ █ █ ██ █████ █ █ ██       █   ██████████████", "██████████████   █       ██ █ ███████ ██ █ █       ██ █ █ █████ ██ █   █   █ ██ █████ ███ ██ █         ██ █████████ ██         █ ██ ███████ █ ██     █   █ ██████████████", "██████████████           ██████████ █ ██         █ ██ █████ ██████   █   █   ██ █ █████ █ ██ █     █ █ ██ █████ ███ ██ █       █ ██ █ ███████ ██ █         ██████████████", "██████████████ █   █     ██ █ █ ███ █ ██ █ █ █   █ ██ █ █ █ ███ ██   █ █ █   ██ █████ █ ████   █   █ █ ████ █ █ █ █ ██   █ █ █   ██ █████ ███ ██       █   ██████████████", "██████████████       █   ██ █████ █ █ ██ █   █   █ ██ █ ███████ ██   █   █   ██████ ███ ████   █     █ ██ █ █ █████ ██ █ █     █ ██ ███████ █ ██           ██████████████", "██████████████ █     █   ██ █ ███ █ █ ██     █   █ ██ █████████ ██ █       █ ██ █ █████ █ ██ █   █     ██ ███ ████████ █ █       ██ █ ██████████           ██████████████", "██████████████ █         ██ █ █████ █ ██ █ █   █ █ ██ █ █ ███ █ ██     █   █ ██ █████ ███ ██ █ █   █   ██ █ █ █████ ██ █ █ █   █ ██ █ █ █ █ ████   █   █   ██████████████", "██████████████           ██ █████ ███ ██ █   █ █   ██ █ █ █ █ ████   █ █ █ █ ████████ █ █ ██   █   █   ██ █ █ █████ ██ █   █   █ ██ ███████ █ ██         █ ██████████████", "██████████████           ██████ █ ███ ██   █ █ █   ██ █ █ █ █ ████ █ █ █ █   ██ █ ███ ██████ █   █     ██ ███ █████ ██ █         ██ █████████ ██       █   ██████████████", "██████████████           ██ █████████ ██   █     █ ████ █ █████ ██   █   █   ██ █████ █ █ ██     █ █ █ ██████ █ █ ████   █   █   ██ █████ ███ ██         █ ██████████████", "██████████████   █       ██ █ █ █ ██████ █   █     ██████ █████ ██   █ █     ██ █ ███ ███ ██ █     █ █ ██ ███ ███ █ ██   █ █   █ ██ █ ███ ███ ██ █     █   ██████████████", "██████████████     █     ██ ███ ███ █ ██ █ █   █ █ ██ █ ███ █ ████ █     █   ██ ███ █████ ██   █     █ ████ █████ █ ██ █ █ █   █ ██ █ █ █ ███ ██   █       ██████████████", "██████████████           ██ █████████ ██ █       █ ██ █████ █ █ ██ █     █ █ ██ █ ███ ███ ██   █   █ █ ██████ ███ █ ██   █   █   ██ █████ ██████           ██████████████", "██████████████           ██ ███████ █ ██ █     █ █ ████ ███ █ ████   █ █ █   ██ ███ █ █ █ ██ █   █ █ █ ██ █ █ █ ███ ██ █ █ █   █ ██ ███ ███ █ ██   █       ██████████████", "██████████████       █   ██ ███ █ █ █ ██   █ █   █ ██████ █████ ██     █     ██ █████ ██████ █   █ █   ██ █ █ █ █ ████ █ █ █ █ █ ██ █ █ ███ █ ██   █       ██████████████", "██████████████   █   █   ██ █ ███ █ █ ██ █   █   █ ██████ █████ ██   █       ██ █ ███████ ██ █   █   █ ██ █████ █ █ ██   █   █ █ ██ █ █ ███ █ ██ █     █   ██████████████" };
    public List<string> locationNames = new List<string> {"A1", "B1", "C1", "D1", "E1", "F1", "A2", "B2", "C2", "D2", "E2", "F2", "A3", "B3", "C3", "D3", "E3", "F3", "A4", "B4", "C4", "D4", "E4", "F4", "A5", "B5", "C5", "D5", "E5", "F5", "A6", "B6", "C6", "D6", "E6", "F6" };
    public List<string> directionNames = new List<string> {"North", "West", "South", "East" };
    string symbols = "─│┌┐└┘├┤┬┴┼˂˃˄˅";
    public List<string> validMoves = new List<string> {"X.XX....X..XX.X", ".XX.X.X....X.XX", "X...XX...X.XXX.", ".X.X.X.X....XXX" };
    int chosenMaze = -1;
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
        chosenMaze = UnityEngine.Random.Range(0, 18);
        playerPos = UnityEngine.Random.Range(0, 36);
        keyPos = UnityEngine.Random.Range(0, 36);
        exitPos = UnityEngine.Random.Range(0, 36);
        direction = UnityEngine.Random.Range(0, 4);
        if (keyPos == exitPos) {
            exitPos = 35 - keyPos;
        }

        Debug.LogFormat("[Echolocation #{0}] Maze:", moduleId);
        for (int i = 0; i < 13; i++) {
            Debug.LogFormat("[Echolocation #{0}] {1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}", moduleId, niceMazes[chosenMaze][(13 * i) + 0], niceMazes[chosenMaze][(13 * i) + 1], niceMazes[chosenMaze][(13 * i) + 2], niceMazes[chosenMaze][(13 * i) + 3], niceMazes[chosenMaze][(13 * i) + 4], niceMazes[chosenMaze][(13 * i) + 5], niceMazes[chosenMaze][(13 * i) + 6], niceMazes[chosenMaze][(13 * i) + 7], niceMazes[chosenMaze][(13 * i) + 8], niceMazes[chosenMaze][(13 * i) + 9], niceMazes[chosenMaze][(13 * i) + 10], niceMazes[chosenMaze][(13 * i) + 11], niceMazes[chosenMaze][(13 * i) + 12]);
        }
        Debug.LogFormat("[Echolocation #{0}] Player Position: {1}", moduleId, locationNames[playerPos]);
        Debug.LogFormat("[Echolocation #{0}] Key Position: {1}", moduleId, locationNames[keyPos]);
        Debug.LogFormat("[Echolocation #{0}] Exit Position: {1}", moduleId, locationNames[exitPos]);
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
                        if (playerPos < 6) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go north from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                        } else {
                            tile = mazes[chosenMaze][playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos -= 6;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, locationNames[playerPos]);
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go north from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                            }
                        }
                        break;
                    case 1: //l
                        if (playerPos % 6 == 0) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go west from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                        } else {
                            tile = mazes[chosenMaze][playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos -= 1;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, locationNames[playerPos]);
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go west from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                            }
                        }
                        break;
                    case 2: //d
                        if (29 < playerPos) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go south from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                        } else {
                            tile = mazes[chosenMaze][playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos += 6;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, locationNames[playerPos]);
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go south from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                            }
                        }
                        break;
                    case 3: //r
                        if (playerPos % 6 == 5) {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Echolocation #{0}] U) Can't go east from {1}, STRIKE!", moduleId, locationNames[playerPos]);
                        } else {
                            tile = mazes[chosenMaze][playerPos];
                            tilePlace = symbols.IndexOf(tile);
                            if (validMoves[direction][tilePlace] == '.') {
                                playerPos += 1;
                                Debug.LogFormat("[Echolocation #{0}] U) Current Location: {1}", moduleId, locationNames[playerPos]);
                            } else {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Echolocation #{0}] U) Can't go east from {1}, STRIKE!", moduleId, locationNames[playerPos]);
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
        hitWall = false;
        echoPos = playerPos;
        echoTile = mazes[chosenMaze][echoPos];
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
        playingSound = true;
        while (hitWall == false) {
            if (halfSeconds % 2 == 0 && halfSeconds != 0) {
                switch (direction) {
                    case 0: //u
                        echoPos = echoPos - 6;
                        echoTile = mazes[chosenMaze][echoPos];
                        break;
                    case 1: //l
                        echoPos = echoPos - 1;
                        echoTile = mazes[chosenMaze][echoPos];
                        break;
                    case 2: //d
                        echoPos = echoPos + 6;
                        echoTile = mazes[chosenMaze][echoPos];
                        break;
                    case 3: //r
                        echoPos = echoPos + 1;
                        echoTile = mazes[chosenMaze][echoPos];
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
                    playingSound = false;
                }
            }

            halfSeconds += 1;
            yield return new WaitForSeconds(.5f);
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} u/d/l/r [Presses the button(s) in the specified position(s)] | !{0} center/c [Presses the center button] | !{0} hold/h [Holds the center button]";
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
        if (Regex.IsMatch(command, @"^\s*center\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*c\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            center.OnInteract();
            center.OnInteractEnded();
            yield break;
        }
        yield return null;
        string[] valids = { "u", "l", "d", "r" };
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
            moves[Array.IndexOf(valids, command.ElementAt(i) + "")].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }
}
