using System;
using UnityEngine;
using System.Collections.Generic;

using Common = MyGamez.MySDK.Api;
using ResultCode = MyGamez.MySDK.Api.ResultCode;

namespace MyGamez.MySDK.Android.Api
{
    using static Common.MySDKInit;
    using static Common.AntiAddiction;
    using static Common.Login;
    using static Common.Billing;
    using static Common.Oaid;
    using static Common.App;
    using static Common.Features.Update;
    using static Common.Features.TextValidation;
    using static Common.Features.PrivacyPolicy;
    using static Common.Features.PromoCode;
    using static Common.Advertising;
    using static Common.Util;
    using static Common.Security;

    public static class MySDKInit
    {
        public static void Initialize(IMySDKInitCallback listener)
        {
            using var mySdkInitJava = new AndroidJavaClass("com.mygamez.mysdk.api.MySDK");
            mySdkInitJava.CallStatic("initialize", new object[] { new JavaMySDKInitCallback(listener) });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        public class JavaMySDKInitCallback : AndroidJavaProxy
        {
            private readonly IMySDKInitCallback callback;
            public JavaMySDKInitCallback(IMySDKInitCallback callback) : base("com.mygamez.mysdk.api.MySDK$MySDKInitCallback")
            {
                this.callback = callback;
            }

            public void onResult(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("MySDKInitCallback onResult()");
                    var resultCode = Util.EnumForJavaObject<ResultCode>(result.Call<AndroidJavaObject>("getResultCode"), ResultCode.GENERAL_ERROR);
                    var resultMsg = result.Call<string>("getResultMsg");
                    callback.OnResult(new MySDKInitResult(resultCode, resultMsg));
                });
            }
        }
    }

    public static class Billing
    {
        public static void SetPayCallback(IPayCallback payCallback)
        {
            try
            {
                using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Billing");
                myBillingJava.CallStatic("setPayCallback", new JavaBillingListener(payCallback));
                Debug.Log("Payment callback set successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to set payment callback: " + e.Message);
                // Continue without payment callback - game can still work
            }
        }

        public static void DoBilling(Biller biller, PayInfo payInfo)
        {
            try
            {
                using var javaBiller = new AndroidJavaClass("com.mygamez.mysdk.api.billing.Biller");
                using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Billing");
                myBillingJava.CallStatic("doBilling", javaBiller.CallStatic<AndroidJavaObject>("valueOf", biller.ToString()), JavaObjFromPayInfo(payInfo));
                Debug.Log("Billing initiated successfully for biller: " + biller);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to initiate billing: " + e.Message);
                // Could trigger a callback with failure status if needed
            }
        }

        public static List<Biller> GetAvailableBillers()
        {
            List<Biller> billers = new();
            try
            {
                using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Billing");
                using AndroidJavaObject javaBillers = javaClass.CallStatic<AndroidJavaObject>("getAvailableBillers");
                if (javaBillers != null)
                {
                    for (int i = 0; i < javaBillers.Call<int>("size"); i++)
                    {
                        Biller matched = Util.EnumForJavaObject<Biller>(javaBillers.Call<AndroidJavaObject>("get", i), Biller.UNKNOWN);
                        if (matched != Biller.UNKNOWN)
                        {
                            billers.Add(matched);
                        }
                        else
                        {
                            Debug.Log("Received unknown biller. Possibly incompatible versions of unity wrapper and MySDK version used.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No available billers returned from MySDK");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to get available billers: " + e.Message);
                // Return empty list - game can continue without billing
            }
            return billers;
        }

        public static Biller GetPreferredBiller()
        {
            try
            {
                using AndroidJavaClass javaClass = new("com.mygamez.mysdk.api.Billing");
                return Util.EnumForJavaObject<Biller>(javaClass.CallStatic<AndroidJavaObject>("getPreferredBiller"), Biller.UNKNOWN);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to get preferred biller: " + e.Message);
                return Biller.UNKNOWN;
            }
        }

        public static ChannelDisplay GetStyleForBiller(Biller biller)
        {
            using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Billing");
            using var vendorClass = new AndroidJavaClass("com.mygamez.mysdk.api.billing.Biller");
            return Util.ChannelDisplayFromJavaObject(javaClass.CallStatic<AndroidJavaObject>(
                "getStyleForBiller",
                vendorClass.CallStatic<AndroidJavaObject>("valueOf", biller.ToString())
            ));
        }

        public static void ConfirmGoodsGiven() {
            if ( _result == null)
            {
                Debug.Log("Can't confirm goods given, result is null");
            }
            _result.Call("confirmGoodsGiven");
            _result = null;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaBillingListener : AndroidJavaProxy
        {
            private readonly IPayCallback callback;
            public JavaBillingListener(IPayCallback callback) : base("com.mygamez.mysdk.api.billing.PayCallback")
            {
                this.callback = callback;
            }

            public void onBillingResult(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk onBillingResult()");
                    var billingResult = BillingResultFromJavaObj(result);
                    callback.OnBillingResult(billingResult);
                });
            }
        }

        public static AndroidJavaObject JavaObjFromPayInfo(PayInfo payInfo)
        {
            var iapInfoJava = new AndroidJavaObject("com.mygamez.mysdk.api.billing.IAPInfo", payInfo.IapInfo.AmountFen, payInfo.IapInfo.Name, payInfo.IapInfo.Desc);
            var builder = new AndroidJavaObject("com.mygamez.mysdk.api.billing.PayInfo$Builder", iapInfoJava);
            builder.Call<AndroidJavaObject>("withCustomID", payInfo.CustomID);
            builder.Call<AndroidJavaObject>("withExtraInfo", payInfo.ExtraInfo);
            var payInfoJava = builder.Call<AndroidJavaObject>("build");
            return payInfoJava;
        }

        private static AndroidJavaObject _result;
        private static BillingResult BillingResultFromJavaObj(AndroidJavaObject result)
        {
            _result = result;
            ResultCode resultCode = Util.EnumForJavaObject<ResultCode>(result.Call<AndroidJavaObject>("getResultCode"), ResultCode.UNKNOWN);
            var resultMsg = result.Call<string>("getResultMsg");
            var biller = result.Call<string>("getBiller");
            var payInfo = PayInfoFromJavaObj(result.Call<AndroidJavaObject>("getPayInfo"));

            // Only Successful payments have Verification
            Verification verification = new Verification("", "", "");
            if (ResultCode.SUCCESS.Equals(resultCode))
            {
                verification = Security.VerificationFromJavaObject(result.Call<AndroidJavaObject>("getVerification"));
            }
            return new BillingResult(resultCode, resultMsg, biller, payInfo, verification);
        }

        private static PayInfo PayInfoFromJavaObj(AndroidJavaObject payInfoJavaObj)
        {
            IAPInfo iapInfo = IAPInfoFromJavaObj(payInfoJavaObj.Call<AndroidJavaObject>("getIAPInfo"));
            var orderID = payInfoJavaObj.Call<string>("getOrderID");
            var customID = payInfoJavaObj.Call<string>("getCustomID");
            var extraInfo = payInfoJavaObj.Call<string>("getExtraInfo");
            var createTime = payInfoJavaObj.Call<long>("getCreateTime");
            var payInfo = new PayInfo(iapInfo, customID, extraInfo);
            payInfo.OrderID = orderID;
            payInfo.CreateTime = createTime;

            return payInfo;
        }

        private static IAPInfo IAPInfoFromJavaObj(AndroidJavaObject iapInfoJavaObj)
        {
            var amountFen = iapInfoJavaObj.Call<int>("getAmountFen");
            var name = iapInfoJavaObj.Call<string>("getName");
            var desc = iapInfoJavaObj.Call<string>("getDesc");

            return new IAPInfo(amountFen, name, desc);
        }
    }

    public static class Security
    {
        public static Verification VerificationFromJavaObject(AndroidJavaObject iapInfoJavaObj)
        {
            if (iapInfoJavaObj == null)
                return null;

            var kID = iapInfoJavaObj.Call<string>("getKId");
            var payload = iapInfoJavaObj.Call<string>("getPayload");
            var signature = iapInfoJavaObj.Call<string>("getSignature");
            return new Verification(kID, payload, signature);
        }
    }

    public static class Advertising
    {
        public static void SetRewardedVideoAdStatusListener(IRewardedVideoAdReadyListener listener)
        {
            using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Advertising");
            myBillingJava.CallStatic("setRewardedVideoAdStatusListener", new JavaRewardedVideoAdReadyListener(listener));
        }

        public static void SetInterstitialAdStatusListener(IInterstitialAdReadyListener listener)
        {
            using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Advertising");
            myBillingJava.CallStatic("setInterstitialAdStatusListener", new JavaInterstitialAdReadyListener(listener));
        }

        public static bool AreInterstitialAdsEnabled()
        {
            if (!InterstitialAdsEnabledChecked)
            {
                CheckAdsEnabled();
            }
            return InterstitialAdsEnabled;
        }

        public static bool AreRewardedVideoAdsEnabled()
        {
            if (!RewardedVideoAdsEnabledChecked)
            {
                CheckAdsEnabled();
            }
            return RewardedVideoAdsEnabled;
        }

        public static void SetPersonalizedAdsEnabled(bool enabled)
        {
            using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Advertising");
            myBillingJava.CallStatic("setPersonalizedAdsEnabled", enabled);
        }

        public static bool GetPersonalizedAdsEnabled()
        {
            using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Advertising");
            return myBillingJava.CallStatic<bool>("getPersonalizedAdsEnabled");
        }

        private static bool RewardedVideoAdsEnabledChecked = false;
        private static bool RewardedVideoAdsEnabled = false;
        private static bool InterstitialAdsEnabledChecked = false;
        private static bool InterstitialAdsEnabled = false;


        public class RewardedVideo : IRewardedVideoAd
        {
            private readonly AndroidJavaObject javaAd;
            public string MediationAdapterClassName { get; }

            public RewardedVideo(AndroidJavaObject javaObj)
            {
                this.javaAd = javaObj;
                this.MediationAdapterClassName = javaAd.Call<string>("getMediationAdapterClassName");
            }

            public void SetRewardedVideoAdListener(IRewardedVideoAdListener callback)
            {
                javaAd.Call("setRewardedVideoAdListener", new JavaRewardedVideoAdListener(callback));
            }

            public void SetRewards(Reward[] rewards)
            {
                using AndroidJavaObject list = new("java.util.ArrayList");
                foreach (Reward reward in rewards)
                {
                    using AndroidJavaObject javaReward = new("com.mygamez.mysdk.api.advertising.Reward", reward.Type, reward.Amount);
                    list.Call<bool>("add", javaReward);
                }
                javaAd.Call("setRewards", list);
            }

            public void Show()
            {
                javaAd.Call("show");
            }
        }


        public class InterstitialAd : IInterstitialAd
        {
            private readonly AndroidJavaObject javaAd;
            public string MediationAdapterClassName { get; }

            public InterstitialAd(AndroidJavaObject javaObj)
            {
                javaAd = javaObj;
                this.MediationAdapterClassName = javaAd.Call<string>("getMediationAdapterClassName");
            }

            public void SetInterstitialAdListener(IInterstitialAdListener callback)
            {
                {
                    javaAd.Call("setInterstitialAdListener", new JavaInterstitialAdListener(callback));
                }
            }

            public void Show()
            {
                javaAd.Call("show");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaRewardedVideoAdReadyListener : AndroidJavaProxy
        {
            private readonly IRewardedVideoAdReadyListener callback;
            public JavaRewardedVideoAdReadyListener(IRewardedVideoAdReadyListener callback) : base("com.mygamez.mysdk.api.advertising.AdReadyListener")
            {
                this.callback = callback;
            }

            public void onAdReady(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaRewardedVideoAdReadyListener onAdReady()");
                    callback.OnAdReady(RewardedVideoAdFromJavaObj(result));
                });
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaInterstitialAdReadyListener : AndroidJavaProxy
        {
            private readonly IInterstitialAdReadyListener callback;
            public JavaInterstitialAdReadyListener(IInterstitialAdReadyListener callback) : base("com.mygamez.mysdk.api.advertising.AdReadyListener")
            {
                this.callback = callback;
            }

            public void onAdReady(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaInterstitialAdReadyListener onAdReady()");
                    callback.OnAdReady(InterstitialAdFromJavaObj(result));
                });
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaRewardedVideoAdListener : AndroidJavaProxy
        {
            private readonly IRewardedVideoAdListener callback;
            public JavaRewardedVideoAdListener(IRewardedVideoAdListener callback) : base("com.mygamez.mysdk.api.advertising.RewardedVideoAdListener")
            {
                this.callback = callback;
            }

            public void onStarted()
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaRewardedVideoAdListener onStarted()");
                    callback.OnStarted();
                });
            }

            public void onComplete(AndroidJavaObject rewardsJavaObj)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaRewardedVideoAdListener onComplete()");
                    int size = rewardsJavaObj.Call<int>("size");
                    var rewards = new Reward[size];

                    for (int i = 0; i < size; i++)
                    {
                        rewards[i] = RewardFromJavaObject(rewardsJavaObj.Call<AndroidJavaObject>("get", i));
                    }
                    callback.OnComplete(rewards);
                });
            }

            public void onCancel()
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaRewardedVideoAdListener onCancel()");
                    callback.OnCancel();
                });
            }

            public void onError(int code, string msg)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaRewardedVideoAdListener onError()");
                    this.callback.OnError(code, msg);
                });
            }

            private Reward RewardFromJavaObject(AndroidJavaObject rewardJavaObj)
            {
                var type = rewardJavaObj.Call<string>("getType");
                var amount = rewardJavaObj.Call<int>("getAmount");
                var reward = new Reward(type, amount);
                return reward;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaInterstitialAdListener : AndroidJavaProxy
        {
            private readonly IInterstitialAdListener callback;
            public JavaInterstitialAdListener(IInterstitialAdListener callback) : base("com.mygamez.mysdk.api.advertising.InterstitialAdListener")
            {
                this.callback = callback;
            }


            public void onError(int code, string msg)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaInterstitialAdListener onError()");
                    this.callback.OnError(code, msg);
                });
            }
            public void onShown()
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaInterstitialAdListener onShown()");
                    this.callback.OnShown();
                });
            }
            public void onClicked()
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaInterstitialAdListener onClicked()");
                    this.callback.OnClicked();
                });
            }
            public void onClosed()
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaInterstitialAdListener onClosed()");
                    this.callback.OnClosed();
                });
            }
        }

        private static void CheckAdsEnabled()
        {
            using var myAdvertisingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Advertising");
            bool rewardsEnabled = myAdvertisingJava.CallStatic<bool>("areRewardedVideoAdsEnabled");
            RewardedVideoAdsEnabled = rewardsEnabled;
            RewardedVideoAdsEnabledChecked = true;

            bool interstitialsEnabled = myAdvertisingJava.CallStatic<bool>("areInterstitialAdsEnabled");
            InterstitialAdsEnabled = interstitialsEnabled;
            InterstitialAdsEnabledChecked = true;
        }

        private static IRewardedVideoAd RewardedVideoAdFromJavaObj(AndroidJavaObject javaObj)
        {
            return new RewardedVideo(javaObj);
        }
        private static IInterstitialAd InterstitialAdFromJavaObj(AndroidJavaObject javaObj)
        {
            return new InterstitialAd(javaObj);
        }
    }


    public static class Features
    {
        public static class ChannelAnalytics
        {
            public static bool IsChannelAnalyticsSupported()
            {
                using var myAdvertisingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$ChannelAnalytics");
                return myAdvertisingJava.CallStatic<bool>("isChannelAnalyticsSupported");
            }

            public static bool GetCurrentAnalyticsSwitchStatus()
            {
                using var myAdvertisingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$ChannelAnalytics");
                return myAdvertisingJava.CallStatic<bool>("getCurrentAnalyticsSwitchStatus");
            }
            public static void SetDataCollectionEnabled(bool Enabled)
            {
                using var myFeaturesJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$ChannelAnalytics");
                myFeaturesJava.CallStatic("setDataCollectionEnabled", Enabled);
            }
        }

        public static class RateGame
        {
            public static bool IsRateGameSupported()
            {
                var myAdvertisingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$RateGame");
                return myAdvertisingJava.CallStatic<bool>("isRateGameSupported");
            }

            public static void DoRateGame()
            {
                using var myFeaturesJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$RateGame");
                myFeaturesJava.CallStatic("doRateGame");
            }
        }

        public static class PrivacyPolicy
        {
            public static bool IsPpAccepted()
            {
                using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Features$PrivacyPolicy");
                return javaClass.CallStatic<bool>("isPpAccepted");
            }

            public static void SetPpAccepted()
            {
                using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Features$PrivacyPolicy");
                javaClass.CallStatic("setPpAccepted");
            }

            public static DialogData DialogDataFromJavaObject(AndroidJavaObject javaObject)
            {
                if (javaObject == null)
                {
                    // Return default dialog data if Java object is null
                    return new DialogData("Privacy Policy", "Please accept our privacy policy to continue.", "Accept");
                }
                
                string title = javaObject.Get<string>("title");
                string content = javaObject.Get<string>("content");
                string button = javaObject.Get<string>("button");
                return new DialogData(title, content, button);
            }

            public static DialogData GetPpData()
            {
                try
                {
                    using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Features$PrivacyPolicy");
                    var javaObject = javaClass.CallStatic<AndroidJavaObject>("getPpData");
                    return DialogDataFromJavaObject(javaObject);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to get privacy policy data: " + e.Message);
                    return new DialogData("Privacy Policy", "Please accept our privacy policy to continue.", "Accept");
                }
            }

            public static DialogData GetTosData()
            {
                try
                {
                    using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Features$PrivacyPolicy");
                    var javaObject = javaClass.CallStatic<AndroidJavaObject>("getTosData");
                    return DialogDataFromJavaObject(javaObject);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to get terms of service data: " + e.Message);
                    return new DialogData("Terms of Service", "Please accept our terms of service to continue.", "Accept");
                }
            }
        }

        public static class GameEvents
        {
            public static void OnGamePaused()
            {
                using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Features$GameEvents");
                javaClass.CallStatic("onGamePaused");
            }
        }

        public static class CustomerSupport
        {
            private static bool isAllowed = false;
            private static bool isJavaChecked = false;
            private static string customerSupportText = "";
            private static string customerSupportTitle = "";

            public static bool IsCustomerSupportAllowed()
            {
                CheckFromJava();
                return isAllowed;
            }

            public static string GetCustomerSupportTitle()
            {
                CheckFromJava();
                return customerSupportTitle;
            }

            public static string GetCustomerSupportText()
            {
                CheckFromJava();
                return customerSupportText;
            }

            private static void CheckFromJava()
            {
                if (!isJavaChecked)
                {
                    using AndroidJavaClass javaClass = new("com.mygamez.mysdk.api.Features$CustomerSupport");
                    isAllowed = javaClass.CallStatic<bool>("isCustomerSupportAllowed");
                    customerSupportTitle = javaClass.CallStatic<string>("getCustomerSupportTitle");
                    customerSupportText = javaClass.CallStatic<string>("getCustomerSupportText");
                    isJavaChecked = true;
                }
            }
        }

        public static class Social
        {
            public static void SharePictureAndText(string desc, string imageAbsolutePath)
            {
                using AndroidJavaClass javaClass = new("com.mygamez.mysdk.api.Features$Social");
                javaClass.CallStatic("shareTextAndPicture", desc, imageAbsolutePath);
            }
        }

        public static class TextValidation
        {
            public static void ValidateText(string text, ITextValidationCallback callback)
            {
                using AndroidJavaClass java = new("com.mygamez.mysdk.api.Features$TextValidation");
                java.CallStatic("validateText", text, new JavaTextValidationCallback(callback));
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
            private class JavaTextValidationCallback : AndroidJavaProxy
            {
                private readonly ITextValidationCallback callback;
                public JavaTextValidationCallback(ITextValidationCallback callback) : base("com.mygamez.mysdk.api.features.textvalidation.TextValidationCallback")
                {
                    this.callback = callback;
                }

                void onTextValidation(AndroidJavaObject result)
                {
                    MyMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        Debug.Log("mysdk JavaTextValidationCallback onTextValidation()");
                        TextValidationResult resultCSharp = TextValidationResultFromJavaObj(result);
                        callback.OnTextValidation(resultCSharp);
                    });
                }
            }

            private static TextValidationResult TextValidationResultFromJavaObj(AndroidJavaObject javaObj)
            {
                string text = javaObj.Call<string>("getText");
                ResultCode resultCode = Util.EnumForJavaObject<ResultCode>(javaObj.Call<AndroidJavaObject>("getResultCode"), ResultCode.FAILED);
                return new TextValidationResult(resultCode, text);
            }
        }

        public static class PromoCode
        {
            public static IPromoCodeHandler GetPromoCodeHandler()
            {
                using AndroidJavaClass javaClass = new("com.mygamez.mysdk.api.Features$PromoCode");
                return new PromoCodeHandler(javaClass.CallStatic<AndroidJavaObject>("getPromoCodeHandler"));
            }

            private class PromoCodeHandler : IPromoCodeHandler
            {
                private readonly AndroidJavaObject javaObject;

                public PromoCodeHandler(AndroidJavaObject obj)
                {
                    this.javaObject = obj;
                }

                public void CheckPromoCode(string promoCode, IPromoCodeCallback callback)
                {
                    this.javaObject.Call("checkPromoCode", promoCode, new JavaPromoCodeCallback(callback));
                }

                public void SetPromoCodeUsed(string promoCode)
                {
                    this.javaObject.Call("setPromoCodeUsed", promoCode);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
            public class JavaPromoCodeCallback : AndroidJavaProxy
            {
                private readonly IPromoCodeCallback callback;
                public JavaPromoCodeCallback(IPromoCodeCallback callback) : base("com.mygamez.mysdk.api.features.promocode.PromoCodeCallback")
                {
                    this.callback = callback;
                }

                public void onResultReceived(AndroidJavaObject result)
                {
                    MyMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        Debug.Log("mysdk JavaPromoCodeCallback onResultReceived()");
                        PromoCodeResult promoCodeResult = PromoCodeResultFromJavaObj(result);
                        callback.OnResultReceived(promoCodeResult);
                    });
                }
            }

            private static PromoCodeResult PromoCodeResultFromJavaObj(AndroidJavaObject result)
            {
                PromoCodeResult resultCsharp = new();

                // Save Promo Code to C# PromoCodeResult
                resultCsharp.PromoCode = result.Call<string>("getPromoCode");

                // Save Rewards to C# PromoCodeResult
                using AndroidJavaObject rewardsJavaObj = result.Call<AndroidJavaObject>("getRewards");
                int size = rewardsJavaObj.Call<int>("size");
                Reward[] rewards = new Reward[size];
                for (int i = 0; i < size; i++)
                {
                    rewards[i] = RewardFromJavaObject(rewardsJavaObj.Call<AndroidJavaObject>("get", i));
                }
                resultCsharp.Rewards = rewards;

                // Save Result Code to C# PromoCodeResult
                resultCsharp.ResultCode = Util.EnumForJavaObject<ResultCode>(result.Call<AndroidJavaObject>("getResultCode"), ResultCode.UNKNOWN);

                // Save Verificiation to C# PromoCodeResult
                // Only VALID promo codes have Verification
                if (ResultCode.SUCCESS.Equals(resultCsharp.ResultCode))
                {
                    resultCsharp.Verification = Security.VerificationFromJavaObject(result.Call<AndroidJavaObject>("getVerification")); ;
                }

                return resultCsharp;
            }

            private static Reward RewardFromJavaObject(AndroidJavaObject rewardJavaObj)
            {
                var type = rewardJavaObj.Call<string>("getType");
                var amount = rewardJavaObj.Call<int>("getAmount");
                var reward = new Reward(type, amount);
                return reward;
            }
        }


        public static class Update
        {

            public static UpdateData UpdateDataFromJavaObject(AndroidJavaObject javaData)
            {
                string fileUrl = javaData.Call<string>("getFileUrl");
                long sizeBytes = javaData.Call<long>("getSizeBytes");
                string imageUrl = javaData.Call<string>("getImageUrl");
                bool forced = javaData.Call<bool>("isForced");
                return new UpdateData(fileUrl, sizeBytes, imageUrl, forced);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
            private class JavaUpdateCheckCallback : AndroidJavaProxy
            {
                private readonly IUpdateCheckCallback callback;
                public JavaUpdateCheckCallback(IUpdateCheckCallback callback) : base("com.mygamez.mysdk.api.features.update.UpdateCheckCallback")
                {
                    this.callback = callback;
                }

                public void onSuccess(AndroidJavaObject javaData)
                {
                    MyMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        Debug.Log("mysdk JavaUpdateCheckCallback onSuccess()");
                        callback.OnSuccess(UpdateDataFromJavaObject(javaData));
                    });
                }

                public void onFailure(int errorCode, string errorMessage)
                {
                    MyMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        Debug.Log("MySDK JavaUpdateCheckCallback onFailure()");
                        callback.OnFailure((ResultCode)errorCode, errorMessage);
                    });
                }
            }

            public static void CheckUpdate(IUpdateCheckCallback callback)
            {
                using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$Update");
                myBillingJava.CallStatic("checkUpdate", new JavaUpdateCheckCallback(callback));
            }

            public static void UpdateVersion()
            {
                using var myBillingJava = new AndroidJavaClass("com.mygamez.mysdk.api.Features$Update");
                myBillingJava.CallStatic("updateVersion");
            }
        }
    }

    public static class App
    {
        public static void Exit(IExitCallback callback)
        {
            using AndroidJavaClass appJava = new("com.mygamez.mysdk.api.App");
            appJava.CallStatic("exit", new JavaExitCallback(callback));
        }

        public static void QuitApp()
        {
            using AndroidJavaClass appJava = new("com.mygamez.mysdk.api.App");
            appJava.CallStatic("exitApp");
        }
        
        public static void RestartApp()
        {
            using AndroidJavaClass appJava = new("com.mygamez.mysdk.api.App");
            appJava.CallStatic("restartApp");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        public class JavaExitCallback : AndroidJavaProxy
        {
            private readonly IExitCallback callback;
            public JavaExitCallback(IExitCallback callback) : base("com.mygamez.mysdk.api.app.ExitCallback")
            {
                this.callback = callback;
            }

            public void onExitResult(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaExitCallback onExitResult()");
                    ResultCode exitResult = Util.EnumForJavaObject<ResultCode>(result, ResultCode.SUCCESS);
                    callback.OnExitResult(exitResult);
                });
            }
        }
    }

    public static class Login
    {
        public static void RegisterLoginStateListener(ILoginStateListener listener)
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            myJava.CallStatic("registerLoginStateListener", new JavaLoginStateListener(listener));
        }

        public static LoginInfo GetLoginInfo()
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            using AndroidJavaObject JavaLoginInfo = myJava.CallStatic<AndroidJavaObject>("getLoginInfo");
            return LoginInfoFromJavaObject(JavaLoginInfo);
        }

        public static void DoLogin(Vendor vendor)
        {
            using var vendorJava = new AndroidJavaClass("com.mygamez.mysdk.api.login.Vendor");
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            myJava.CallStatic("doLogin", vendorJava.CallStatic<AndroidJavaObject>("valueOf", vendor.ToString()));
        }

        public static void DoLogout()
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            myJava.CallStatic("doLogout");
        }

        public static bool IsInGameLoginSupported()
        {
            bool res = false;
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            res = myJava.CallStatic<bool>("isInGameLoginSupported");
            return res;
        }

        public static List<Vendor> GetAvailableVendors()
        {
            List<Vendor> vendors = new List<Vendor>();
            using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            using AndroidJavaObject javaVendors = javaClass.CallStatic<AndroidJavaObject>("getAvailableVendors");
            for (int i = 0; i < javaVendors.Call<int>("size"); i++)
            {
                Vendor matched = Util.EnumForJavaObject<Vendor>(javaVendors.Call<AndroidJavaObject>("get", i), Vendor.UNKNOWN);
                if (matched != Vendor.UNKNOWN)
                {
                    vendors.Add(matched);
                }
                else
                {
                    Debug.Log("Received unknown vendor. Possibly incompatible versions of unity wrapper and MySDK version used.");
                }
            }
            return vendors;
        }

        public static Vendor GetPreferredVendor()
        {
            using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            return Util.EnumForJavaObject<Vendor>(javaClass.CallStatic<AndroidJavaObject>("getPreferredVendor"), Vendor.UNKNOWN);
        }

        public static ChannelDisplay GetStyleForVendor(Vendor vendor)
        {
            using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.Login");
            using var vendorClass = new AndroidJavaClass("com.mygamez.mysdk.api.login.Vendor");
            return Util.ChannelDisplayFromJavaObject(javaClass.CallStatic<AndroidJavaObject>(
                "getStyleForVendor",
                vendorClass.CallStatic<AndroidJavaObject>("valueOf", vendor.ToString())
            ));
        }

        public static LoginInfo LoginInfoFromJavaObject(AndroidJavaObject javaObj)
        {
            string playerID = javaObj.Call<string>("getPlayerID");
            Verification verification = Security.VerificationFromJavaObject(javaObj.Call<AndroidJavaObject>("getVerification"));
            LoginState loginState = Util.EnumForJavaObject<LoginState>(javaObj.Call<AndroidJavaObject>("getLoginState"), LoginState.LOGGED_OUT);
            string vendorName = javaObj.Call<string>("getVendorName");
            LoginInfo.LOGIN_VENDOR_LOCAL = javaObj.GetStatic<string>("LOGIN_VENDOR_LOCAL");
            return new LoginInfo(playerID, verification, loginState, vendorName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaLoginStateListener : AndroidJavaProxy
        {
            private readonly ILoginStateListener callback;
            public JavaLoginStateListener(ILoginStateListener callback) : base("com.mygamez.mysdk.api.login.LoginStateListener")
            {
                this.callback = callback;
            }

            public void onLoginStateChanged(AndroidJavaObject state)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaLoginStateListener OnLoginStateChanged()");
                    callback.OnLoginStateChanged(Util.EnumForJavaObject<LoginState>(state, LoginState.LOGGED_OUT));
                });
            }
        }
    }

    public static class AntiAddiction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        public class JavaAntiAddictionCallback : AndroidJavaProxy
        {
            private readonly IAntiAddictionCallback callback;
            public JavaAntiAddictionCallback(IAntiAddictionCallback callback) : base("com.mygamez.mysdk.api.antiaddiction.AntiAddictionCallback")
            {
                this.callback = callback;
            }
            public void onResult(AndroidJavaObject resultCode, string msg)
            {
                callback.OnAntiAddictionResult(Util.EnumForJavaObject<ResultCode>(resultCode, ResultCode.GENERAL_ERROR), msg);
            }
        }

        public static void Initialize(string playerId, IAntiAddictionCallback callback)
        {
            using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            javaClass.CallStatic("initialize", playerId, new JavaAntiAddictionCallback(callback));
        }

        // TODO: Part of iOS sdk api, implement on Adroid side as well / unify some other way?
        public static string GetCurrentMyGamezId()
        {
            throw new NotImplementedException("Not available in Android mysdk");
        }

        public static ResultCode DeleteUserAccount()
        {
            throw new NotImplementedException("Not available in Android mysdk");
        }

        public static int GetIAPCreditLeft()
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            return myJava.CallStatic<int>("getIapCreditLeft");
        }

        public static long GetPlaytimeLeft()
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            return myJava.CallStatic<long>("getPlaytimeLeft");
        }

        public static bool IsAdult()
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            return myJava.CallStatic<bool>("isAdult");
        }

        public static void AttemptRidCheck(String name, String rid, IRidCheckCallback callback)
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            myJava.CallStatic("attemptRidCheck", new object[] { name, rid, new JavaRidCheckCallback(callback) });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaRidCheckCallback : AndroidJavaProxy
        {
            private readonly IRidCheckCallback callback;

            public JavaRidCheckCallback(IRidCheckCallback callback) : base("com.mygamez.mysdk.api.antiaddiction.rid.RidCheckCallback")
            {
                this.callback = callback;
            }

            public void onResult(string name, string rid, AndroidJavaObject javaObj, string message)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaRidCheckCallback onResult");
                    var resultCode = Util.EnumForJavaObject<ResultCode>(javaObj, ResultCode.GENERAL_ERROR);
                    callback.OnResult(name, rid, resultCode, message);
                });
            }
        }

        public static bool CheckIdFormat(string rid)
        {
            using var javaClass = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            return javaClass.CallStatic<bool>("checkIdFormat", rid);
        }

        public static void VerifyIdentity(string name, string rid, IAntiAddictionCallback callback)
        {
            throw new NotImplementedException("Not available in Androdi mysdk");
        }

        public static void RequestGameStart(IAntiAddictionCallback listener)
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            myJava.CallStatic("requestGameStart", new object[] { new JavaAntiAddictionCallback(listener) });
        }

        public static PromptDialogData GetRidCheckFailsPromptDialogData()
        {
            return GetPromptDialogData("getRidCheckFailsPromptDialogData");
        }

        public static PromptDialogData GetPlayerIdentificationCompletedPromptDialogData()
        {
            return GetPromptDialogData("getPlayerIdentificationCompletedPromptDialogData");
        }

        public static PromptDialogData GetDailyGameTimeDepletionPromptDialogData()
        {
            return GetPromptDialogData("getDailyGameTimeDepletionPromptDialogData");
        }

        public static PromptDialogData GetTimeOfDayConstraintPromptDialogData()
        {
            return GetPromptDialogData("getTimeOfDayConstraintPromptDialogData");
        }

        public static PromptDialogData GetStoreEnterPromptDialogData()
        {
            return GetPromptDialogData("getStoreEnterPromptDialogData");
        }

        public static PromptDialogData GetSinglePurchaseLimitExceededPromptDialogData()
        {
            return GetPromptDialogData("getSinglePurchaseLimitExceededPromptDialogData");
        }

        public static PromptDialogData GetMonthlyPurchaseLimitExceededPromptDialogData()
        {
            return GetPromptDialogData("getMonthlyPurchaseLimitExceededPromptDialogData");
        }

        private static PromptDialogData GetPromptDialogData(string methodName)
        {
            using var myJava = new AndroidJavaClass("com.mygamez.mysdk.api.AntiAddiction");
            using var storePromptJavaObj = myJava.CallStatic<AndroidJavaObject>(methodName);
            if (storePromptJavaObj == null)
            {
                return null;
            }
            else
            {
                string title = storePromptJavaObj.Call<string>("getTitle");
                string button = storePromptJavaObj.Call<string>("getButton");
                string body = storePromptJavaObj.Call<string>("getBody");
                return new PromptDialogData(title, body, button);
            }
        }

        internal static void RequestInAppPurchase(float price, IAntiAddictionCallback callback)
        {
            throw new NotImplementedException("RequestInAppPurchase not available in android mysdk, use billing API instead");
        }

        internal static void AnnounceCompletedInappPurchase(float price, IAntiAddictionCallback callback)
        {
            throw new NotImplementedException("AnnounceCompletedInappPurchase not available in android mysdk, use billing API instead");
        }
    }

    public static class Oaid
    {
        public static void RequestOaid(IOaidRequestCallback callback)
        {
            using var appJava = new AndroidJavaClass("com.mygamez.mysdk.api.Oaid");
            appJava.CallStatic("requestOaid", new JavaOaidRequestCallback(callback));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Java naming convention conflicts with C#")]
        private class JavaOaidRequestCallback : AndroidJavaProxy
        {
            private readonly IOaidRequestCallback callback;
            public JavaOaidRequestCallback(IOaidRequestCallback callback) : base("com.mygamez.mysdk.api.analytics.oaid.OaidRequestCallback")
            {
                this.callback = callback;
            }

            public void onOaidRequestComplete(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaOaidRequestCallback onOaidRequestComplete()");
                    string oaidStr = result.Call<string>("getOAID");
                    OaidInfo oaidInfo = new OaidInfo(oaidStr);
                    callback.OnOaidRequestComplete(oaidInfo);
                });
            }

            public void onOaidRequestError(AndroidJavaObject result)
            {
                MyMainThreadDispatcher.instance.Enqueue(() =>
                {
                    Debug.Log("mysdk JavaOaidRequestCallback onOaidRequestError()");
                    ErrorResponse ErrorResponse = Util.ErrorResponseFromJavaObject(result);
                    callback.OnOaidRequestError(ErrorResponse);
                });
            }
        }
    }


    public static class Util
    {
        public static ErrorResponse ErrorResponseFromJavaObject(AndroidJavaObject javaObj)
        {
            int ErrCode = javaObj.Call<int>("getErrCode");
            string ErrMsg = javaObj.Call<string>("getErrMsg");
            return new ErrorResponse(ErrCode, ErrMsg);
        }

        public static ChannelDisplay ChannelDisplayFromJavaObject(AndroidJavaObject javaObj)
        {
            string name = javaObj.Call<string>("getName");
            int iconResourceId = javaObj.Call<int>("getIcon");

            // TODO: Verify if this actually loads the image correctly or not
            // Gets the drawable bitmap from java side, and parses those pixels into texture in unity
            using var currentActivity = Util.GetCurrentActivity();
            using var resources = currentActivity.Call<AndroidJavaObject>("getResources");
            using var bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
            using var javabitmap = bitmapFactory.CallStatic<AndroidJavaObject>("decodeResource", resources, iconResourceId);

            if (bitmapFactory == null)
            {
                Debug.Log("Failed to get icon");
                return null;
            }
                
            int width = javabitmap.Call<int>("getWidth");
            int height = javabitmap.Call<int>("getHeight");
            int[] pixelArray = new int[width * height];
            javabitmap.Call("getPixels", pixelArray, 0, width, 0, 0, width, height);

            Texture2D texture = new(width, height, TextureFormat.ARGB32, false);
            Color32[] colors = new Color32[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int pixel = pixelArray[i];
                // Android ColorInt is ARGB
                colors[i] = new Color32(
                    (byte)((pixel >> 16) & 0xFF), // R
                    (byte)((pixel >> 8) & 0xFF),  // G
                    (byte)((pixel >> 0) & 0xFF),  // B
                    (byte)((pixel >> 24) & 0xFF)  // A
                );
            }

            texture.Apply();
            return new ChannelDisplay(name, texture);
        }

        public static T EnumForJavaObject<T>(AndroidJavaObject obj, T defaultValue) where T : struct, Enum
        {
            T solution = defaultValue;

            if (obj == null)
            {
                Debug.Log("Received null enum value from android, using default ( "+ defaultValue.ToString() +" ) instead");
                return defaultValue;
            }

            Debug.Log("Trying to parse value for android object named " + obj.Call<string>("name"));

            foreach (string name in Enum.GetNames(typeof(T)))
            {
                if (obj.Call<string>("name").Equals(name))
                {
                    Enum.TryParse<T>(name, out solution);
                }
            }
            return solution;
        }

        public static AndroidJavaObject GetCurrentActivity()
        {
            using var javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            return currentActivity;
        }
    }
}
