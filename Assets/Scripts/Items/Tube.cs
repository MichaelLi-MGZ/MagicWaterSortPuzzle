using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tube 
{
    public string tubeName;

    public Color[] tubeColors;

    [Range(0, 4)]
    public int numberOfColorInBottle = 4;
}
