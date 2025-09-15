using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementItem : MonoBehaviour
{
    public Image achieIcon;

    public Image achieBoard;

    public Image achieStroke;

    public TextMeshProUGUI achieName;

    public TextMeshProUGUI achieDes;

    public TextMeshProUGUI bonusCoinTxt;

    public TextMeshProUGUI progressTxt;

    public Image progressBar;

    public void InitView(AchievementData mData)
    {
        if(mData.currentValue >= mData.maxValue)
        {
            achieIcon.sprite = mData.achieIconUnlock;
            achieBoard.sprite = mData.achieBoardUnLock;
            achieStroke.sprite = mData.strokeUnLock;
        }
        else
        {
            achieIcon.sprite = mData.achieIconLock;
            achieBoard.sprite = mData.achieBoardLock;
            achieStroke.sprite = mData.strokeLock;
        }

        achieName.text = mData.achieName;
        achieDes.text = mData.achieDes;
        bonusCoinTxt.text = "+" + mData.bonusCoin.ToString();

        if(mData.currentValue < mData.maxValue)
        {
            progressTxt.text = mData.currentValue + "/" + mData.maxValue;
            progressBar.fillAmount = (float)mData.currentValue / (float)mData.maxValue;
        }
        else
        {
            progressTxt.text = mData.maxValue + "/" + mData.maxValue;
            progressBar.fillAmount = 1.0f;
        }
        
    }
}
