using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelGenerator1 : MonoBehaviour
{

    public List<Level> levelInFirstRow;

    public List<Level> levelInSecondRow;

    public TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        /*
        for(int i = 1; i <= 10; i++)
        {
            ReadLevel(i);
        }
        */

        ReadLevel(3);
    }


    public void CreateLevel()
    {
        string inputContent = inputField.text.ToString();

        int maxLvIndex = int.Parse(inputContent);

        if(maxLvIndex > 0)
        {
            for (int i = 1; i <= maxLvIndex; i++)
            {
                ReadLevel(i);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ReadLevel(int mLevel)
    {
        levelInFirstRow.Clear();
        levelInSecondRow.Clear();

        string path = "Levels/" + mLevel.ToString();

        TextAsset levelTxt = Resources.Load(path) as TextAsset;

        string levelContent = levelTxt.text;

        string[] rowData = levelContent.Split('|');

        //Debug.Log(levelContent);

        if (rowData.Length <= 1)
            rowData[0] = rowData[0].Remove(rowData[0].Length - 1);

        string[] firstRowData = rowData[0].Split(';');

        

        //if(rowData.Length <= 1)
        // firstRowData = firstRowData.SkipLast(1).ToArray();


        // for (int i = 0; i < firstRowData.Length; i++)
        // {
        //firstRowData[i].Trim();
        //Debug.Log(" Split 1 :" + firstRowData[i] + " Size " + firstRowData[i].Length);
        // }





        levelInFirstRow = new List<Level>();

        for (int i = 0; i < firstRowData.Length; i++)
        {
            Level inLevel = new Level();

            int volumeSize = firstRowData[i].Length;

            inLevel.volumeIndex = new int[volumeSize];

            for (int j = 0; j < volumeSize; j++)
            {
                if (firstRowData[i][j].ToString() != "" && firstRowData[i][j].ToString() != "-")
                    int.TryParse(firstRowData[i][j].ToString(), out inLevel.volumeIndex[j]);
           
            }

            levelInFirstRow.Add(inLevel);
        }

        if(rowData.Length > 1)
        {
            //for (int i = 0; i < secondRowData.Length; i++)
            //  Debug.Log(" Split 2 :" + secondRowData[i]);

            rowData[1] = rowData[1].Remove(rowData[1].Length - 1);
            string[] secondRowData = rowData[1].Split(';');

            //secondRowData = secondRowData.SkipLast(1).ToArray();

            levelInSecondRow = new List<Level>();

            for (int i = 0; i < secondRowData.Length; i++)
            {
                Level inLevel = new Level();

                int volumeSize = secondRowData[i].Length;

                inLevel.volumeIndex = new int[volumeSize];

                for (int j = 0; j < volumeSize; j++)
                {
                    if (secondRowData[i][j].ToString() != "" && secondRowData[i][j].ToString() != "-")
                        int.TryParse(secondRowData[i][j].ToString(), out inLevel.volumeIndex[j]);

                }

                levelInSecondRow.Add(inLevel);
            }
        }

#if UNITY_EDITOR
        LevelSetting levelTest = ScriptableObject.CreateInstance<LevelSetting>();
        levelTest.bottlesInFirstRow = levelInFirstRow;
        levelTest.bottlesInSecondRow = levelInSecondRow;
        UnityEditor.AssetDatabase.CreateAsset(levelTest, "Assets/Data/Level" + mLevel.ToString() + ".asset");
# endif

    }
}
