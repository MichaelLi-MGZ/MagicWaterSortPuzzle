using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Level")]
public class LevelSetting : ScriptableObject
{

    [Header("Row 1")]
    [Space(5)]
    public List<Level> bottlesInFirstRow;



    [Header("Row 2")]
    [Space(5)]
    public List<Level> bottlesInSecondRow;


}



[System.Serializable]
public class Level
{

    public int[] volumeIndex;

    // if you instentiate 12 bottles you should let 2 bottles empety 

    public Level()
    {
       
       

    }

}
