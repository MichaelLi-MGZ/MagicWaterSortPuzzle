using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UndoElement 
{
    public int moveNumber;

    public Tube undoFirstTube, undoSecondTube;

    public TubeController undoFirstTubeController, undoSecondTubeController;
}
