using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class echolocationScript : MonoBehaviour
{

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

    private int _size = 4;
    private MazeGenerator _mazeGenerator;
    private string _mazeString;

    private Coroutine _holdCheck;
    private int _curOrientation;

    private void Start()
    {
        _mazeGenerator = new MazeGenerator(_size);
        _moduleId = _moduleIdCounter++;
        Center.OnInteract += CenterPress;
        Center.OnInteractEnded += CenterRelease;
        for (int btn = 0; btn < MoveSels.Length; btn++)
            MoveSels[btn].OnInteract += MovePress(btn);
        _mazeString = _mazeGenerator.GenerateMaze();
        LogMaze();
    }

    private bool CenterPress()
    {
        Center.AddInteractionPunch(0.5f);
        return false;
    }

    private void CenterRelease()
    {

    }

    private KMSelectable.OnInteractHandler MovePress(int btn)
    {
        return delegate ()
        {

            return false;
        };
    }

    private void LogMaze()
    {
        var maze = new string[(_size * 2 + 1)];
        for (int i = 0; i < maze.Length; i++)
            maze[i] = _mazeString.Substring(i * (_size * 2 + 1), (_size * 2 + 1));
        Debug.Log(maze.Join("\n"));
    }
}
