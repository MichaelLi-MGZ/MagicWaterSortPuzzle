using UnityEngine;

public class AdvertisingManager : MonoBehaviour
{
    public static AdvertisingManager Instance { get; private set; }

    private MyGamez.MySDK.Api.Advertising.IRewardedVideoAd rewardedVideoAd;
    private MyGamez.MySDK.Api.Advertising.IInterstitialAd interstitialAd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Set up advertising system
        SetupAdvertising();
    }

    private void SetupAdvertising()
    {
        Debug.Log("Setting up advertising system");
        
        // Set up Rewarded Video Ads
        if (MyGamez.MySDK.Api.Advertising.AreRewardedVideoAdsEnabled())
        {
            Debug.Log("Rewarded Video Ads enabled, setting up listener");
            MyGamez.MySDK.Api.Advertising.SetRewardedVideoAdStatusListener(new RewardedVideoAdReadyListener());
        }
        else
        {
            Debug.Log("Rewarded Video Ads not enabled");
        }
        
        // Set up Interstitial Ads
        if (MyGamez.MySDK.Api.Advertising.AreInterstitialAdsEnabled())
        {
            Debug.Log("Interstitial Ads enabled, setting up listener");
            MyGamez.MySDK.Api.Advertising.SetInterstitialAdStatusListener(new InterstitialAdReadyListener());
        }
        else
        {
            Debug.Log("Interstitial Ads not enabled");
        }
    }

    // Show Rewarded Video Ad
    public void ShowRewardedVideoAd()
    {
        if (rewardedVideoAd != null)
        {
            Debug.Log("Showing Rewarded Video Ad");
            
            // Set rewards before showing ad
            var rewards = new MyGamez.MySDK.Api.Advertising.Reward[]
            {
                new MyGamez.MySDK.Api.Advertising.Reward("coins", 100),
                new MyGamez.MySDK.Api.Advertising.Reward("hints", 1)
            };
            rewardedVideoAd.SetRewards(rewards);
            
            rewardedVideoAd.Show();
        }
        else
        {
            Debug.Log("Rewarded Video Ad not ready");
        }
    }

    // Show Interstitial Ad
    public void ShowInterstitialAd()
    {
        if (interstitialAd != null)
        {
            Debug.Log("Showing Interstitial Ad");
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial Ad not ready");
        }
    }

    // Check if ads are ready
    public bool IsRewardedVideoAdReady()
    {
        return rewardedVideoAd != null;
    }

    public bool IsInterstitialAdReady()
    {
        return interstitialAd != null;
    }

    // Give rewards to player
    private void GiveRewards(MyGamez.MySDK.Api.Advertising.Reward[] rewards)
    {
        Debug.Log("Giving rewards to player");
        
        foreach (var reward in rewards)
        {
            Debug.Log("Reward: " + reward.Type + " x" + reward.Amount);
            
            switch (reward.Type.ToLower())
            {
                case "coins":
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.currentCoin += reward.Amount;
                        GameManager.instance.SaveCoin();
                        Debug.Log("Added " + reward.Amount + " coins. Total: " + GameManager.instance.currentCoin);
                    }
                    break;
                case "hints":
                    // Add hints to player
                    Debug.Log("Added " + reward.Amount + " hints");
                    break;
                default:
                    Debug.Log("Unknown reward type: " + reward.Type);
                    break;
            }
        }
    }

    private class RewardedVideoAdReadyListener : MyGamez.MySDK.Api.Advertising.IRewardedVideoAdReadyListener
    {
        public void OnAdReady(MyGamez.MySDK.Api.Advertising.IRewardedVideoAd ad)
        {
            Debug.Log("Rewarded Video Ad ready");
            
            if (AdvertisingManager.Instance != null)
            {
                AdvertisingManager.Instance.rewardedVideoAd = ad;
                
                // Set up ad listener
                ad.SetRewardedVideoAdListener(new RewardedVideoAdListener());
            }
        }
    }

    private class RewardedVideoAdListener : MyGamez.MySDK.Api.Advertising.IRewardedVideoAdListener
    {
        public void OnStarted()
        {
            Debug.Log("Rewarded Video Ad started");
        }

        public void OnComplete(MyGamez.MySDK.Api.Advertising.Reward[] rewards)
        {
            Debug.Log("Rewarded Video Ad completed, giving rewards");
            
            if (AdvertisingManager.Instance != null)
            {
                AdvertisingManager.Instance.GiveRewards(rewards);
            }
        }

        public void OnCancel()
        {
            Debug.Log("Rewarded Video Ad canceled by user");
        }

        public void OnError(int code, string msg)
        {
            Debug.LogError("Rewarded Video Ad error: " + code + " - " + msg);
        }
    }

    private class InterstitialAdReadyListener : MyGamez.MySDK.Api.Advertising.IInterstitialAdReadyListener
    {
        public void OnAdReady(MyGamez.MySDK.Api.Advertising.IInterstitialAd ad)
        {
            Debug.Log("Interstitial Ad ready");
            
            if (AdvertisingManager.Instance != null)
            {
                AdvertisingManager.Instance.interstitialAd = ad;
                
                // Set up ad listener
                ad.SetInterstitialAdListener(new InterstitialAdListener());
            }
        }
    }

    private class InterstitialAdListener : MyGamez.MySDK.Api.Advertising.IInterstitialAdListener
    {
        public void OnShown()
        {
            Debug.Log("Interstitial Ad shown");
            // Game should be paused here
        }

        public void OnClosed()
        {
            Debug.Log("Interstitial Ad closed");
            // Continue game play if game is paused
        }

        public void OnClicked()
        {
            Debug.Log("Interstitial Ad clicked");
        }

        public void OnError(int code, string msg)
        {
            Debug.LogError("Interstitial Ad error: " + code + " - " + msg);
        }
    }
}