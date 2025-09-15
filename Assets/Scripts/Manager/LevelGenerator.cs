using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Advertisements;
using static AdsControl;

public class LevelGenerator : MonoBehaviour
{
    public LevelSetting currentLevel;

    public Transform firstRowRootTrs;

    public Transform centerRowRootTrs;

    public Transform secondRowRootTrs;

    public float[] rowSegmentPos;

    public TubeController tubeInPrefab;

    public TubeController hintTube;

    public ColorConfig[] colorConfig;

    public ColorConfig currentColorConfig;

    public int numberBottleWin;

    public List<TubeController> currentTubeListInFirstRow;

    public List<TubeController> currentTubeListInSecondRow;

    public CoinFXTarget coinTarget;
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitLvGen()
    {
        currentColorConfig = colorConfig[PlayerPrefs.GetInt("CurrentPalette")];
        LoadLevel(GameManager.instance.currentLv);
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevel = Resources.Load<LevelSetting>("LevelConfigs/Level" + levelIndex.ToString());
        GenerateBottleInLevel();
        GetNumberBottleWin();
    }

    public void GenerateBottleInLevel()
    {
        currentTubeListInFirstRow = new List<TubeController>();
        currentTubeListInSecondRow = new List<TubeController>();

        if (currentLevel.bottlesInFirstRow.Count == 5 || currentLevel.bottlesInSecondRow.Count == 5)
            Camera.main.orthographicSize = 11;
        if (currentLevel.bottlesInFirstRow.Count == 6 || currentLevel.bottlesInSecondRow.Count == 6)
            Camera.main.orthographicSize = 12;
        if (currentLevel.bottlesInFirstRow.Count == 7 || currentLevel.bottlesInSecondRow.Count == 7)
            Camera.main.orthographicSize = 13;
        coinTarget.UpdatePos();

        float firstPosRow1 = 0.0f;


        firstPosRow1 = -(currentLevel.bottlesInFirstRow.Count - 1) * rowSegmentPos[currentLevel.bottlesInFirstRow.Count - 1] * 0.5f;


        for (int i = 0; i < currentLevel.bottlesInFirstRow.Count; i++)
        {
            Vector3 genPos = Vector3.zero;

            if (currentLevel.bottlesInSecondRow.Count > 0)
                genPos = new Vector3(firstPosRow1 + rowSegmentPos[currentLevel.bottlesInFirstRow.Count - 1] * i, firstRowRootTrs.position.y, 0.0f);
            else
                genPos = new Vector3(firstPosRow1 + rowSegmentPos[currentLevel.bottlesInFirstRow.Count - 1] * i, centerRowRootTrs.position.y, 0.0f);

            TubeController tubeObject = Instantiate(tubeInPrefab, genPos, Quaternion.identity);


            for (int j = 0; j < currentLevel.bottlesInFirstRow[i].volumeIndex.Length; j++)
            {
                tubeObject.bottleColors[j] = currentColorConfig.colorList[currentLevel.bottlesInFirstRow[i].volumeIndex[j]];
                // UnityEngine.Debug.Log("bottle  " + i + " Color" + currentLevel.bottlesInFirstRow[i].volumeIndex[j]);
            }

            tubeObject.InitTube(currentLevel.bottlesInFirstRow[i].volumeIndex.Length);

            GameManager.instance.tubeListInGame.Add(tubeObject);
            currentTubeListInFirstRow.Add(tubeObject);
        }

        if (currentLevel.bottlesInSecondRow.Count > 0)
        {
            float firstPosRow2 = -(currentLevel.bottlesInSecondRow.Count - 1) * rowSegmentPos[currentLevel.bottlesInSecondRow.Count - 1] * 0.5f;

            for (int i = 0; i < currentLevel.bottlesInSecondRow.Count; i++)
            {
                Vector3 genPos = new Vector3(firstPosRow2 + rowSegmentPos[currentLevel.bottlesInSecondRow.Count - 1] * i, secondRowRootTrs.position.y, 0.0f);

                TubeController tubeObject = Instantiate(tubeInPrefab, genPos, Quaternion.identity);

                for (int j = 0; j < currentLevel.bottlesInSecondRow[i].volumeIndex.Length; j++)
                {
                    tubeObject.bottleColors[j] = currentColorConfig.colorList[currentLevel.bottlesInSecondRow[i].volumeIndex[j]];

                }

                tubeObject.InitTube(currentLevel.bottlesInSecondRow[i].volumeIndex.Length);
                GameManager.instance.tubeListInGame.Add(tubeObject);
                currentTubeListInSecondRow.Add(tubeObject);
            }
        }

        if (GameManager.instance.currentLv >= 4)
            AddHintBottle();

    }

    /*
    public void AddMoreBottle()
    {
        if (CheckFullBottleTotal())
        {
            GameManager.instance.uiManager.warningView.ShowView("Bottles are full \n Let's try later");
           
        }
        else
        AdsControl.Instance.ShowRewardedAd(AdsControl.REWARD_TYPE.MORE_BOTTLE);
    }
    */

    public void AddHintBottle()
    {
        if (CheckFullBottleTotal())
        {
            return;
        }


        // AudioManager.instance.addTube.Play();

        if (currentLevel.bottlesInSecondRow.Count > 0)
        {

            if (currentTubeListInFirstRow.Count <= currentTubeListInSecondRow.Count)
            {
                TubeController tubeObject = Instantiate(tubeInPrefab, Vector3.zero, Quaternion.identity);
                //tubeObject.InitTube(0);
                tubeObject.LockTube();
                hintTube = tubeObject;
                //GameManager.instance.tubeListInGame.Add(tubeObject);
                currentTubeListInFirstRow.Add(tubeObject);

                float firstPosRow1 = -(currentTubeListInFirstRow.Count - 1) * rowSegmentPos[currentTubeListInFirstRow.Count - 1] * 0.5f;

                for (int i = 0; i < currentTubeListInFirstRow.Count; i++)
                {
                    Vector3 genPos = new Vector3(firstPosRow1 + rowSegmentPos[currentTubeListInFirstRow.Count - 1] * i, firstRowRootTrs.position.y, 0.0f);

                    currentTubeListInFirstRow[i].transform.position = genPos;

                    currentTubeListInFirstRow[i].RePos();
                }

            }
            else
            {
                TubeController tubeObject = Instantiate(tubeInPrefab, Vector3.zero, Quaternion.identity);
                //tubeObject.InitTube(0);
                tubeObject.LockTube();
                hintTube = tubeObject;
                //GameManager.instance.tubeListInGame.Add(tubeObject);
                currentTubeListInSecondRow.Add(tubeObject);

                float firstPosRow2 = -(currentTubeListInSecondRow.Count - 1) * rowSegmentPos[currentTubeListInSecondRow.Count - 1] * 0.5f;

                for (int i = 0; i < currentTubeListInSecondRow.Count; i++)
                {
                    Vector3 genPos = new Vector3(firstPosRow2 + rowSegmentPos[currentTubeListInSecondRow.Count - 1] * i, secondRowRootTrs.position.y, 0.0f);

                    currentTubeListInSecondRow[i].transform.position = genPos;
                    currentTubeListInSecondRow[i].RePos();

                }

            }
        }
        else
        {

            TubeController tubeObject = Instantiate(tubeInPrefab, Vector3.zero, Quaternion.identity);
            //tubeObject.InitTube(0);
            // GameManager.instance.tubeListInGame.Add(tubeObject);
            tubeObject.LockTube();
            hintTube = tubeObject;
            currentTubeListInFirstRow.Add(tubeObject);

            float firstPosRow1 = -(currentTubeListInFirstRow.Count - 1) * rowSegmentPos[currentTubeListInFirstRow.Count - 1] * 0.5f;

            for (int i = 0; i < currentTubeListInFirstRow.Count; i++)
            {
                Vector3 genPos = new Vector3(firstPosRow1 + rowSegmentPos[currentTubeListInFirstRow.Count - 1] * i, centerRowRootTrs.position.y, 0.0f);

                currentTubeListInFirstRow[i].transform.position = genPos;

                currentTubeListInFirstRow[i].RePos();
            }
        }

        if (currentTubeListInFirstRow.Count == 5 || currentTubeListInSecondRow.Count == 5)
            Camera.main.orthographicSize = 11;
        if (currentTubeListInFirstRow.Count == 6 || currentTubeListInSecondRow.Count == 6)
            Camera.main.orthographicSize = 12;
        if (currentTubeListInFirstRow.Count == 7 || currentTubeListInSecondRow.Count == 7)
            Camera.main.orthographicSize = 13;
        coinTarget.UpdatePos();
    }

    public void UnlockHintBottle()
    {
        WatchAds();
    }

    public void UnlockHintBottleCB()
    {
        hintTube.UnlockTube();
        AudioManager.instance.addTube.Play();
        GameManager.instance.tubeListInGame.Add(hintTube);
        AddHintBottle();

        if (GameManager.instance.uiManager.profileView.achieDataList[0].currentValue < GameManager.instance.uiManager.profileView.achieDataList[0].maxValue)
        {
            GameManager.instance.uiManager.profileView.achieDataList[0].currentValue++;
            PlayerPrefs.SetInt("UseBottles", GameManager.instance.uiManager.profileView.achieDataList[0].currentValue);
            GameManager.instance.uiManager.profileView.CheckUnlockAchie(0);
        }

    }

    public bool CheckFullBottleTotal()
    {
        bool checkFull = false;

        if (currentLevel.bottlesInSecondRow.Count > 0)
        {
            if (currentTubeListInFirstRow.Count == 7 && currentTubeListInSecondRow.Count == 7)
                checkFull = true;
        }
        else if (currentLevel.bottlesInSecondRow.Count == 0)
        {
            if (currentTubeListInFirstRow.Count == 7)
                checkFull = true;
        }

        return checkFull;
    }

    private void GetNumberBottleWin()
    {
        int count = 0;

        for (int i = 0; i < GameManager.instance.tubeListInGame.Count; i++)
        {
            count += GameManager.instance.tubeListInGame[i].numberOfColorsInBottle;
        }

        numberBottleWin = count / 4;
    }

    public void RefreshColor(ColorConfig newColorConfig)
    {


        for (int i = 0; i < GameManager.instance.tubeListInGame.Count; i++)
        {
            for (int j = 0; j < GameManager.instance.tubeListInGame[i].bottleColors.Length; j++)
            {
                int colorID = GetIDFromColor(GameManager.instance.tubeListInGame[i].bottleColors[j]);
                GameManager.instance.tubeListInGame[i].bottleColors[j] = newColorConfig.colorList[colorID];

            }

            GameManager.instance.tubeListInGame[i].UpdateColorOnShader();
            GameManager.instance.tubeListInGame[i].UpdateTopColorValues();
        }

        currentColorConfig = newColorConfig;
    }

    public void UpdateBottleSkin(int skinID)
    {
        for (int i = 0; i < GameManager.instance.tubeListInGame.Count; i++)
        {
            for (int j = 0; j < GameManager.instance.tubeListInGame[i].bottleColors.Length; j++)
            {
                GameManager.instance.tubeListInGame[i].ChangeSkin(skinID);

            }
        }

        GameManager.instance.levelGen.hintTube.ChangeSkin(skinID);
    }

    int GetIDFromColor(Color mColor)
    {
        int ID = 0;

        for (int i = 0; i < currentColorConfig.colorList.Count; i++)
        {
            if (mColor == currentColorConfig.colorList[i])
            {
                ID = i;
                break;
            }

        }

        return ID;
    }

    public void WatchAds()
    {
        AudioManager.instance.clickBtn.Play();
        if (AdsControl.Instance.currentAdsType == ADS_TYPE.ADMOB)
        {
            if (AdsControl.Instance.rewardedAd != null)
            {
                if (AdsControl.Instance.rewardedAd.CanShowAd())
                {
                    AdsControl.Instance.ShowRewardAd(EarnReward);
                }
            }
        }
        else if (AdsControl.Instance.currentAdsType == ADS_TYPE.UNITY)
        {
            ShowRWUnityAds();
        }
        else if (AdsControl.Instance.currentAdsType == ADS_TYPE.MEDIATION)
        {
            if (AdsControl.Instance.rewardedAd.CanShowAd())

                AdsControl.Instance.ShowRewardAd(EarnReward);

            else
                ShowRWUnityAds();
        }
    }

    public void EarnReward(Reward reward)
    {
        UnlockHintBottleCB();
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                UnlockHintBottleCB();
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }
}
