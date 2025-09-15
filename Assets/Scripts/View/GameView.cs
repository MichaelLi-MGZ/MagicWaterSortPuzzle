using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
using static AdsControl;

public class GameView : BaseView
{
    public TextMeshProUGUI levelTxt;

    public TextMeshProUGUI remainUndoTxt;

    public TextMeshProUGUI coinTxt;

    private int undoRemain;

    public bool unlockHintView;

    public Transform coinIconInBoard;

    public GameObject hintRWIcon, undoRWIcon, undoDes,booster, boosterTut,boosterMask;

    public Button replayBtn, hintBtn;
   

    public void Replay()
    {
        AudioManager.instance.clickBtn.Play();
        GameManager.instance.ReplayGame();
        //SceneManager.LoadScene(0);
    }

    public void MoreBottle()
    {
        AudioManager.instance.clickBtn.Play();
        //GameManager.instance.levelGen.AddMoreBottle();
    }

    public void ShowBooster()
    {
        booster.SetActive(true);
    }

    public void ShowTutorial()
    {
        ShowBooster();
        boosterMask.SetActive(true);
        replayBtn.interactable = false;
        hintBtn.interactable = false;
        boosterTut.SetActive(true);
    }

    public void HideTutorial()
    {
        boosterMask.SetActive(false);
        replayBtn.interactable = true;
        hintBtn.interactable = true;
        boosterTut.SetActive(false);
    }

    public void HideBooster()
    {
        booster.SetActive(false);
    }

    public void UndoCB()
    {

        undoRemain = 6;
        PlayerPrefs.SetInt("Undo", undoRemain);

        remainUndoTxt.gameObject.SetActive(true);
        remainUndoTxt.text = undoRemain.ToString();
        undoDes.SetActive(false);
        undoRWIcon.SetActive(false);

    }

    public void Undo()
    {
        AudioManager.instance.clickBtn.Play();

       

        if (undoRemain > 0)
        {
            if (GameManager.instance.CanUndo())
            {
                undoRemain--;
                PlayerPrefs.SetInt("Undo", undoRemain);
                GameManager.instance.ProcessUndo();
                RefreshUndo();
            }
            
        }

        else
        {
            //AdsControl.Instance.ShowRewardedAd(AdsControl.REWARD_TYPE.UNDO);
        }

        if (GameManager.instance.currentLv == 3 && !GameManager.instance.finishFinalTut)
        {
            
                HideTutorial();
                GameManager.instance.finishFinalTut = true;
        }

    }

    public override void Start()
    {
       
    }

    public override void Update()
    {
        
    }

    private void RefreshUndo()
    {
        if (undoRemain > 0)
        {
            remainUndoTxt.gameObject.SetActive(true);
            remainUndoTxt.text = undoRemain.ToString();
            undoDes.SetActive(false);
            undoRWIcon.SetActive(false);
        }
        else
        {
            remainUndoTxt.gameObject.SetActive(false);
            undoDes.SetActive(true);
            undoRWIcon.SetActive(true);
        }
    }

    public override void InitView()
    {
        levelTxt.text = "Level " + GameManager.instance.currentLv.ToString();
        undoRemain = PlayerPrefs.GetInt("Undo");
        coinTxt.text = GameManager.instance.currentCoin.ToString();
        RefreshUndo();
    }

    public void ShowShop()
    {
        AudioManager.instance.clickBtn.Play();
        GameManager.instance.uiManager.shopView.ShowView();
    }

    public void ShowProfile()
    {
        AudioManager.instance.clickBtn.Play();
        GameManager.instance.uiManager.profileView.ShowView();

    }

    public void OpenLevelSelect()
    {
        AudioManager.instance.clickBtn.Play();
        SceneRouter.LoadLevelSelectScene();
    }

    public void ShowLevelBonusView()
    {
        AudioManager.instance.clickBtn.Play();
        AudioManager.instance.cat.Play();
        GameManager.instance.uiManager.bonusLevelView.ShowView();
    }

    public void ShowHintViewCB()
    {
        
        GameManager.instance.uiManager.hintView.ShowView();
        
        if(!unlockHintView)
        {
            hintRWIcon.SetActive(false);
            unlockHintView = true;
        }
    }

    public void ShowHintView()
    {
        AudioManager.instance.clickBtn.Play();

        if (!unlockHintView)
           WatchAds();
        else
            ShowHintViewCB();
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
       ShowHintViewCB();
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
               ShowHintViewCB();
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }
}
