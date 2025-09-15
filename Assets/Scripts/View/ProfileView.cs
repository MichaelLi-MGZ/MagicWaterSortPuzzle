using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileView : BaseView
{
    public Image soundIcon, hapticIcon;

    public Sprite soundOn, soundOff, hapticOn, hapticOff;

    public TextMeshProUGUI coinTxt;

    public List<AchievementData> achieDataList;

    public List<AchievementItem> achievementItemList = new List<AchievementItem>();

    public AchievementItem achieItemObject;

    public Transform achieItemRoot;

    public AchieView achieView;
  
    public override void InitView()
    {
        //achievementItemList.Clear();

        if (AudioManager.instance.musicState == 1)
        {
            soundIcon.sprite = soundOn;
        }
        else
        {
            soundIcon.sprite = soundOff;
        }

        if (AudioManager.instance.hapticState == 1)
        {
            hapticIcon.sprite = hapticOn;
        }
        else
        {
            hapticIcon.sprite = hapticOff;
        }

        coinTxt.text = GameManager.instance.currentCoin.ToString();
        GetAchieData();
        InitAchievement();
    }

    public void GetAchieData()
    {
        achieDataList[0].currentValue = PlayerPrefs.GetInt("UseBottles");
        achieDataList[1].currentValue = GameManager.instance.currentLv;
        achieDataList[2].currentValue = GameManager.instance.currentLv;
        achieDataList[3].currentValue = GameManager.instance.currentLv;
        achieDataList[4].currentValue = PlayerPrefs.GetInt("RestartNumber");
        achieDataList[5].currentValue = GameManager.instance.currentLv;
        achieDataList[6].currentValue = GameManager.instance.currentLv;
        achieDataList[7].currentValue = GameManager.instance.currentLv;
        achieDataList[8].currentValue = GameManager.instance.currentLv;
        achieDataList[9].currentValue = GameManager.instance.currentLv;
        achieDataList[10].currentValue = GameManager.instance.currentLv;
    }

    public void InitAchievement()
    {
        //Debug.Log("Achie Lenght " + achievementItemList.Count);
        if(achievementItemList.Count <= 0)
        {
            achievementItemList.Add(achieItemObject);
            achieItemObject.InitView(achieDataList[0]);
            achieItemObject.transform.parent = achieItemRoot;
            achieItemObject.transform.localPosition = Vector3.zero;
            achieItemObject.transform.localScale = Vector3.one;

            for (int i = 1; i < achieDataList.Count; i++)
            {
                AchievementItem achieItem = Instantiate(achieItemObject, Vector3.zero, Quaternion.identity);
                achieItem.transform.parent = achieItemRoot;
                achieItem.transform.localPosition = Vector3.zero;
                achieItem.transform.localScale = Vector3.one;
                achieItem.InitView(achieDataList[i]);
                achievementItemList.Add(achieItem);
            }
            
        }
        else
        {
            
            for (int i = 0; i < achievementItemList.Count; i++)
            {
                achievementItemList[i].InitView(achieDataList[i]);
                
            }
            
        }
    }

    public override void ShowView()
    {
        base.ShowView();
        GetAchieData();
        InitAchievement();
        AdsControl.Instance.HideBannerAd();
    }

    public void CheckUnlockAchie(int achieIndex)
    {
        if (achieDataList[achieIndex].currentValue  == achieDataList[achieIndex].maxValue)
        {
            achieView.InitView(achieDataList[achieIndex]);
            achieView.ShowView();
        }
    }

    public void UnlockAchie(int achieIndex)
    {
       
            achieView.InitView(achieDataList[achieIndex]);
            achieView.ShowView();
        
    }

    public override void Start()
    {
       
    }

    public override void Update()
    {
        
    }

    public void ShowShop()
    {
        HideView();
        AudioManager.instance.clickBtn.Play();
        GameManager.instance.uiManager.shopView.ShowView();
    }

    public void ToggleSound()
    {
        int musicState = AudioManager.instance.musicState;

        if(musicState == 0)
        {
            PlayerPrefs.SetInt("Music", 0);
            AudioManager.instance.musicState = 1;
            soundIcon.sprite = soundOn;

            AudioManager.instance.ToogleMusic(true);
            AudioManager.instance.ToogleSound(true);
        }
        else
        {
            PlayerPrefs.SetInt("Music", 1);
            AudioManager.instance.musicState = 0;
            soundIcon.sprite = soundOff;

            AudioManager.instance.ToogleMusic(false);
            AudioManager.instance.ToogleSound(false);
        }
    }


    public void ToggleHaptic()
    {
        int hapticState = AudioManager.instance.hapticState;

        if (hapticState == 0)
        {
            PlayerPrefs.SetInt("Haptic", 0);
            AudioManager.instance.hapticState = 1;
            hapticIcon.sprite = hapticOn;
        }
        else
        {
            PlayerPrefs.SetInt("Haptic", 1);
            AudioManager.instance.hapticState = 0;
            hapticIcon.sprite = hapticOff;
        }
    }

    public override void HideView()
    {
        base.HideView();
        AdsControl.Instance.ShowBannerAd();
        AudioManager.instance.clickBtn.Play();
    }


    public void RestoreAds()
    {
        AudioManager.instance.clickBtn.Play();
        IAPManager.instance.RestorePurchases();
    }

}

[System.Serializable]
public class AchievementData
{
    public int achieID;

    public Sprite achieIconLock;

    public Sprite achieIconUnlock;

    public Sprite achieBoardLock;

    public Sprite achieBoardUnLock;

    public Sprite strokeLock;

    public Sprite strokeUnLock;

    public string achieName;

    public string achieDes;

    public int bonusCoin;

    public int maxValue;

    public int currentValue;

}
