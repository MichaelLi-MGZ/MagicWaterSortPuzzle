using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG;
using DG.Tweening;

public class AchieView : BaseView
{
    public TextMeshProUGUI achieName;

    public TextMeshProUGUI achieDes;

    public TextMeshProUGUI achieBonusCoin;

    public TextMeshProUGUI progressTxt;

    public Image progressImage;

    public int coinBonus;

    public bool canShowGameWin;

    public RectTransform achieBoard;

    public AchievementData currentAchieData;

    public override void InitView()
    {
        
    }

    public void InitView(AchievementData mData)
    {
        currentAchieData = mData;
        achieName.text = mData.achieName;
        achieDes.text = mData.achieDes;
        achieBonusCoin.text = "+" + mData.bonusCoin.ToString();
        coinBonus = mData.bonusCoin;

        if(mData.currentValue >= mData.maxValue)
        {
            progressTxt.text = mData.maxValue + "/" + mData.maxValue;
            progressImage.fillAmount = 1.0f;
        }
        else
        {
            progressTxt.text = mData.currentValue + "/" + mData.maxValue;
            progressImage.fillAmount = (float)mData.currentValue  /  (float)mData.maxValue;
        }
    }

    public override void Start()
    {
       
    }

    public override void Update()
    {
        
    }

    public override void ShowView()
    {
        base.ShowView();
        //achieBoard.localScale = Vector3.zero;
        achieBoard.DOMoveY( -6.0f , 0.25f).SetDelay(0.25f).SetRelative().SetEase(Ease.Linear)
              .OnComplete(() =>
              {
                  AudioManager.instance.waterFull.Play();
                   achieBoard.DOMoveY(6.0f, 0.25f).SetDelay(1.5f).SetRelative().SetEase(Ease.Linear)

                   .OnComplete(() =>
                   {

                       HideView();
                       GameManager.instance.AddCoin(coinBonus);

                       if (currentAchieData.achieID != 0 && currentAchieData.achieID != 4)
                           StartCoroutine(ShowGameWin());


                   });
                  
              });
    }

    public override void HideView()
    {
        base.HideView();
    }

    IEnumerator ShowGameWin()
    {
        yield return new WaitForSeconds(1.0f);
        AudioManager.instance.gameWin.Play();
        GameManager.instance.uiManager.finishView.ShowView();
    }
}
