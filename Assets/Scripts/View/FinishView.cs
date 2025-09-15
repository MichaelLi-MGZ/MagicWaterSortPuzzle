using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;

public class FinishView : BaseView
{ 

    public ParticleSystem confetVfx,starVfx;

    public Transform bigCoin, bonusCoinBoard, awesomeTxt,continueBtn;

   

    public void ShowView()
    {

        base.ShowView();
        continueBtn.gameObject.SetActive(false);
        bonusCoinBoard.gameObject.SetActive(false);
        awesomeTxt.gameObject.SetActive(false);

        if (AudioManager.instance.hapticState == 1)
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);

        confetVfx.gameObject.SetActive(true);
        starVfx.gameObject.SetActive(true);

        confetVfx.Play();
        

        bigCoin.localScale = new Vector3(0.5f, 0.5f, 1.0f);
        bonusCoinBoard.localScale = new Vector3(0.5f, 0.5f, 1.0f);
        awesomeTxt.localScale = new Vector3(0.5f, 0.5f, 1.0f);

        bigCoin.DOScale(1f, 0.25f).SetDelay(0f).SetEase(Ease.Linear).OnComplete(() =>
        {

            bonusCoinBoard.gameObject.SetActive(true);
            awesomeTxt.gameObject.SetActive(true);

            bonusCoinBoard.DOScale(1f, 0.25f).SetDelay(0f).SetEase(Ease.Linear);
            awesomeTxt.DOScale(1f, 0.25f).SetDelay(0f).SetEase(Ease.Linear);
            starVfx.Play();

        });

        StartCoroutine(ShowContinueBtn());
    }


    public void HideView()
    {
        base.HideView();
        continueBtn.gameObject.SetActive(false);
        bonusCoinBoard.gameObject.SetActive(false);
        awesomeTxt.gameObject.SetActive(false);
    }

    IEnumerator ShowContinueBtn()
    {
        yield return new WaitForSeconds(1.25f);

        continueBtn.gameObject.SetActive(true);
        continueBtn.localScale = new Vector3(0.5f, 0.5f, 1.0f);
        continueBtn.DOScale(1f, 0.5f).SetDelay(0f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(0.5f);
        AdsControl.Instance.ShowInterstital();
    }

    public void AddBonusCoinCB()
    {
        //AudioManager.instance.clickBtn.Play();
        confetVfx.Stop();
        starVfx.Stop();
        confetVfx.gameObject.SetActive(false);
        starVfx.gameObject.SetActive(false);
        HideView();
        GameManager.instance.AddCoin(300);
        GameManager.instance.NextLevel();
    }

    public void AddBonusCoin()
    {
        //AdsControl.Instance.ShowRewardedAd(AdsControl.REWARD_TYPE.COIN_300);
    }

    public void NextLevel()
    {
       
        AudioManager.instance.clickBtn.Play();
        confetVfx.Stop();
        starVfx.Stop();
        confetVfx.gameObject.SetActive(false);
        starVfx.gameObject.SetActive(false);
        HideView();
        GameManager.instance.AddCoin(20);
        GameManager.instance.NextLevel();
        //SceneManager.LoadScene(0);
    }

    public override void Start()
    {
        
    }

    public override void Update()
    {
       
    }

    public override void InitView()
    {
        
    }
}
