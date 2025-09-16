using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

// MySDK Unity data classes and methods. Some features are only available in certain platforms.

// Set Platform so we can call platform specific implementation 
using Platform = MyGamez.MySDK.Android.Api;


namespace MyGamez.MySDK.Api
{
    public enum ResultCode
    {
        // Result
        SUCCESS=0,
        FAILED=-1,
        CANCELLED=-2,
        UNKNOWN=-3,

        // Issue in input
        EMPTY=-11,
        INVALID=-12,

        // Failed for a reason
        ALREADY_DONE=-21,
        LIMITED=-22,             // The operation is prevented by a limit (usage for promocode, frequency for crosspromo, age for antiaddiction, etc)
        TIME_OVER=-23,
        DISALLOWED=-24,
        SDK_BUSY=-25,           // Already waiting for an response to a request, please wait
        NOT_SUPPORTED=-26,      // Operation is not supported
        NOT_INITIALIZED=-27,

        // Operation needed before
        NEED_TO_SHOW_DIALOG=-31,
        PP_AND_TOS_NOT_ACCEPTED=-32,

        // Something went wrong
        GENERAL_ERROR=-40,      // Something that has no separate code
        CONFIG_ERROR=-42,       // Unknown app or other issue in config
        LOCAL_ERROR=-43,        // Error parsing the response data
        NETWORK_ERROR=-44,      // Connection error, check internet and try again
        SERVER_ERROR=-45,       // Backend encountered an error
    }

    public static class MySDKInit
    {
        public class MySDKInitResult
        {
            public ResultCode ResultCode { get; set; }
            public string ResultMsg { get; set; }

            public MySDKInitResult(ResultCode resultCode, string resultMsg)
            {
                ResultCode = resultCode;
                ResultMsg = resultMsg;
            }
        }

        public interface IMySDKInitCallback
        {
            void OnResult(MySDKInitResult result);
        }

        /// <summary>
        /// Initialize MySDK. Only call after PP/TOS has been accepted.
        /// </summary>
        public static void Initialize(IMySDKInitCallback listener)
        {
            Platform.MySDKInit.Initialize(listener);
        }
    }

    /// <summary>
    /// Payment related methods.
    /// </summary>
    public static class Billing
    {
#region Actual API Methods

        /// <summary>
        /// Sets the callback for payment results. Same callback is used for all payments.
        /// This should be set as soon as game is able to handle payment results.
        /// Usually callback set here will be triggered after payment dialog is closed.
        /// It is possible that callback will be triggered right after this method is called.
        /// </summary>
        /// <param name="payCallback">Callback that will process payment result.</param>
        public static void SetPayCallback(IPayCallback payCallback)
        {
            Platform.Billing.SetPayCallback(payCallback);
        }

        /// <summary>
        /// Do IAP
        /// </summary>
        /// <param name="biller">Biller to use for the payment</param>
        /// <param name="payInfo">Information about the payment</param>
        public static void DoBilling(Biller biller, PayInfo payInfo)
        {
            Platform.Billing.DoBilling(biller, payInfo);
        }

        /// <summary>
        /// Get list of billers that are available for IAP processing in this build
        /// </summary>
        /// <returns>List of billers</returns>
        public static List<Biller> GetAvailableBillers()
        {
            return Platform.Billing.GetAvailableBillers();
        }

        /// <summary>
        /// Get the preferred (ie. previously successfully used) biller
        /// </summary>
        /// <returns>biller of the last successful IAP</returns>
        public static Biller GetPreferredBiller()
        {
            return Platform.Billing.GetPreferredBiller();
        }

        public static Util.ChannelDisplay GetStyleForBiller(Biller biller)
        {
            // TODO: store icons in Unity side rather than trying to load them from platform side?
            return Platform.Billing.GetStyleForBiller(biller);
        }

#endregion

#region Data Holders
        public enum Biller
        {
            ISBN,
            ALIPAY,
            BILIBILI,
            HONOR,
            HUAWEI,
            FOUR399,
            MEIZU,
            MONOPOLY,
            OPPO,
            TENCENT,
            VIVO,
            WECHAT3,
            XIAOMI,
            APPLE,
            UNKNOWN
        }

        /// <summary>
        /// Payment result
        /// </summary>
        public class BillingResult
        {
            /// <summary>
            /// ResultCode of this payment. Only give items if SUCCESS.
            /// </summary>
            public ResultCode ResultCode { get; set; }

            /// <summary>
            /// ResultMsg of this payment. Description of result.
            /// </summary>
            public string ResultMsg { get; set; }

            /// <summary>
            /// Biller of this payment. 3rd party who processes the payment
            /// </summary>
            public string Biller { get; set; }

            /// <summary>
            /// Information of this payment. Same what was provided to MySDK when DoBilling was called.
            /// </summary>
            public PayInfo PayInfo { get; set; }

            /// <summary>
            /// Verification for this payment. Only set if ResultCode is SUCCESS. Verification is created at MyGamez server using game specific private key.
            /// </summary>
            public Security.Verification Verification { get; set; }

            /// <summary>
            /// Notify MySDK that payment is fully processed and player has received what they purchased.
            /// </summary>
            public void ConfirmGoodsGiven()
            {
                Platform.Billing.ConfirmGoodsGiven();
            }

            public BillingResult(ResultCode resultCode, string resultMsg, string biller, PayInfo payInfo, MySDK.Api.Security.Verification verification)
            {
                ResultCode = resultCode;
                ResultMsg = resultMsg;
                Biller = biller;
                PayInfo = payInfo;
                Verification = verification;
            }
        }

        /// <summary>
        /// PayInfo is used when triggering payment dialog. It has basic iap info and additional details for this payment.
        /// </summary>
        public class PayInfo
        {
            /// <summary>
            /// Basic information for this purchase.
            /// </summary>
            public IAPInfo IapInfo { get; set; }

            /// <summary>
            /// ID that game has created for this payment. Max length 128 characters.
            /// </summary>
            public string CustomID { get; set; }

            /// <summary>
            /// Extra info that game has created for this payment. Can be JSON formatted text for example. 
            /// </summary>
            public string ExtraInfo { get; set; }

            /// <summary>
            /// MyGamez ID for this payment. Will be overwritten during the payment process if set earlier.
            /// </summary>
            public string OrderID { get; set; }

            /// <summary>
            /// MyGamez server time when this payment was created. Will be overwritten during the payment process if set earlier.
            /// </summary>
            public long CreateTime { get; set; }

            /// <summary>
            /// Constructor for PayInfo
            /// </summary>
            /// <param name="iapInfo">IAPInfo for this payment</param>
            /// <param name="customID">Custom ID for this payment. Generated by the game. Max. 128 characters.</param>
            /// <param name="extraInfo">Any extra info for this payment. </param>
            public PayInfo(IAPInfo iapInfo, string customID, string extraInfo)
            {
                IapInfo = iapInfo;
                CustomID = customID;
                ExtraInfo = extraInfo;
            }
        }

        /// <summary>
        /// Basic information of this payment.
        /// </summary>
        public class IAPInfo
        {
            /// <summary>
            /// Price in Fens. 100 Fens = 1 RMB
            /// </summary>
            public int AmountFen { get; set; }

            /// <summary>
            /// Name of the IAP in Chinese
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Description of the IAP in Chinese
            /// </summary>
            public string Desc { get; set; }

            /// <summary>
            /// Basic information of the IAP player is about to make. All fields are required
            /// </summary>
            /// <param name="amountFen">Price in Fens. 100 Fens = 1 RMB</param>
            /// <param name="name">Name of the IAP in Chinese</param>
            /// <param name="desc">Description of the IAP in Chinese</param>
            public IAPInfo(int amountFen, string name, string desc)
            {
                AmountFen = amountFen;
                Name = name;
                Desc = desc;
            }
        }

#endregion

#region Interfaces

        /// <summary>
        /// Interface for receiving payment result
        /// </summary>
        public interface IPayCallback
        {
            /// <summary>
            /// This method is triggered when payment result is received.
            /// </summary>
            /// <param name="result">Result of the payment</param>
            void OnBillingResult(BillingResult result);
        }
#endregion
    }

    public static class Security
    {
#region Data Holders

        /// <summary>
        /// Information that can be used to verify that message was from MyGamez server.
        /// </summary>
        [System.Serializable]
        public class Verification
        {
            /// <summary>
            /// First 6 characters of SHA3-256(DER-PublicKey)
            /// </summary>
            public string KID;

            /// <summary>
            /// Base64 endcoded result json
            /// </summary>
            public string Payload;

            /// <summary>
            /// Base64 encoded ECDSA-SHA3-256 signature, directly calculated from payload value.
            /// </summary>
            public string Signature;

            public Verification(string kId, string payload, string signature)
            {
                KID = kId;
                Payload = payload;
                Signature = signature;
            }
        }
#endregion
    }

    /// <summary>
    /// Advertisement related classes and methods.
    /// </summary>
    public static class Advertising
    {
#region Actual API Methods

        /// <summary>
        /// Sets the listener to listen when Rewarded Video is ready to be shown.
        /// </summary>
        /// <param name="listener">IRewardedVideoAdReadyListener that gets the notification when video ad is ready.</param>
        public static void SetRewardedVideoAdStatusListener(IRewardedVideoAdReadyListener listener)
        {
            Platform.Advertising.SetRewardedVideoAdStatusListener(listener);
        }

        /// <summary>
        /// Sets the listener to listen when Interstitial is ready to be shown.
        /// </summary>
        /// <param name="listener">IInterstitialAdReadyListener that gets the notification when Interstitial ad is ready.</param>
        public static void SetInterstitialAdStatusListener(IInterstitialAdReadyListener listener)
        {
            Platform.Advertising.SetInterstitialAdStatusListener(listener);
        }

        /// <summary>
        /// Check if Interstitials are allowed on this Chinese Android Store
        /// </summary>
        /// <returns></returns>
        public static bool AreInterstitialAdsEnabled()
        {
            return Platform.Advertising.AreInterstitialAdsEnabled();
        }

        /// <summary>
        /// Check if Rewarded Videos are allowed on this Chinese Android Store
        /// </summary>
        /// <returns></returns>
        public static bool AreRewardedVideoAdsEnabled()
        {
            return Platform.Advertising.AreRewardedVideoAdsEnabled();
        }

        /// <summary>
        /// Set the state of personalized ads
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetPersonalizedAdsEnabled(bool enabled)
        {
            Platform.Advertising.SetPersonalizedAdsEnabled(enabled);
        }

        /// <summary>
        /// Check if personalized ads are enabled
        /// </summary>
        /// <returns>true if personalized ads are enabled, false otherwise</returns>
        public static bool GetPersonalizedAdsEnabled()
        {
            return Platform.Advertising.GetPersonalizedAdsEnabled();
        }
#endregion

#region Interfaces

        /// <summary>
        /// Base Interface for all MySDK Ads.
        /// </summary>
        public interface IAd
        {
            string MediationAdapterClassName { get; }
            void Show();
        }

        /// <summary>
        /// Interface for MySDK Rewarded Video Ads. 
        /// </summary>
        public interface IRewardedVideoAd : IAd
        {
            void SetRewardedVideoAdListener(IRewardedVideoAdListener callback);
            void SetRewards(Reward[] rewards);
        }

        /// <summary>
        /// Interface for Rewarded Video Ad Ready Listener.
        /// Method OnAdReady is triggered when Rewarded Video Ad can be shown.
        /// </summary>
        public interface IRewardedVideoAdReadyListener
        {
            void OnAdReady(IRewardedVideoAd ad);
        }

        /// <summary>
        /// Interface for listener that listens Rewarded Video Ad events.
        /// </summary>
        public interface IRewardedVideoAdListener
        {
            /// <summary>
            /// This method is triggered when Video has been started.
            /// No special actions needed here.
            /// </summary>
            void OnStarted();

            /// <summary>
            /// This method is triggered when Video has been completed.
            /// Player should receive reward.
            /// </summary>
            /// <param name="rewards">List of Rewards that are set to RewardedVideoAd before RewardedVideoAd.Show() is called.</param>
            void OnComplete(Reward[] rewards);

            /// <summary>
            /// This method is triggered when player cancels the video.
            /// Player should not receive reward.
            /// </summary>
            void OnCancel();

            /// <summary>
            /// This method is triggered when any error happens in video playback.
            /// Player should not receive reward.
            /// Video Ad should considered as exited.
            /// </summary>
            /// <param name="code">Error Code from 3rd party Ad SDK.</param>
            /// <param name="msg">Error Message from 3rd party Ad SDK.</param>
            void OnError(int code, string msg);
        }

        /// <summary>
        /// Interface for MySDK Interstitial Ads. 
        /// </summary>
        public interface IInterstitialAd : IAd
        {
            void SetInterstitialAdListener(IInterstitialAdListener callback);
        }

        /// <summary>
        /// Interface for Rewarded Video Ad Ready Listener.
        /// Method OnAdReady is triggered when Interstitial Ad can be shown.
        /// </summary>
        public interface IInterstitialAdReadyListener
        {
            void OnAdReady(IInterstitialAd ad);
        }
        public interface IInterstitialAdListener
        {
            /// <summary>
            /// Callback method to get notification if any error happens when Interstitial is initialising, preparing or showing.
            /// </summary>
            /// <param name="code">Error Code as an <code>int</code> value from 3rd party Ad SDK.</param>
            /// <param name="msg">Error Message as a <code>String</code> value from 3rd party Ad SDK.</param>
            void OnError(int code, string msg);

            /// <summary>
            /// Callback method to get notification when Interstitial ad is drawn on screen. Game should be paused here.
            /// </summary>
            void OnShown();

            /// <summary>
            /// Callback method to get notification when player clicks Interstitial ad.
            /// </summary>
            void OnClicked();

            /// <summary>
            /// Callback method to get notification when Interstitial ad is closed.
            /// Use this method to continue game play if game is paused for Interstitial ad.
            /// </summary>
            void OnClosed();

        }
#endregion

#region Data Holders
        /// <summary>
        /// Reward that player should get
        /// </summary>
        public class Reward
        {
            /// <summary>
            /// Name of the resource. For example "gold".
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// How many pieces should player get. 
            /// </summary>
            public int Amount { get; set; }

            public Reward(string type, int amount)
            {
                Type = type;
                Amount = amount;
            }
        }
#endregion
    }

    /// <summary>
    /// Collection of MySDK features mainly related to making game itself China compatible.
    /// </summary>
    public static class Features
    {
        public static class ChannelAnalytics
        {
            public static bool IsChannelAnalyticsSupported()
            {
                return Platform.Features.ChannelAnalytics.IsChannelAnalyticsSupported();
            }

            public static bool GetCurrentAnalyticsSwitchStatus()
            {
                return Platform.Features.ChannelAnalytics.GetCurrentAnalyticsSwitchStatus();
            }
            public static void SetDataCollectionEnabled(bool Enabled)
            {
                Platform.Features.ChannelAnalytics.SetDataCollectionEnabled(Enabled);
            }
        }

        /// <summary>
        /// Methods for checking if Rate Game feature is enabled and to request MySDK to send player to rate the game.
        /// </summary>
        public static class RateGame
        {
            /// <summary>
            /// Check if Rate Game feature is enabled.
            /// </summary>
            /// <returns>True if Rate Game feature is enabled. Otherwise false.</returns>
            public static bool IsRateGameSupported()
            {
                return Platform.Features.RateGame.IsRateGameSupported();
            }

            /// <summary>
            /// Open game's landing page in correct store application.
            /// </summary>
            public static void DoRateGame()
            {
                Platform.Features.RateGame.DoRateGame();
            }
        }

        /// <summary>
        /// MySDK methods to implement Chinese Privacy notification requirements 
        /// </summary>
        public static class PrivacyPolicy
        {
            public static bool IsPpAccepted()
            {
                return Platform.Features.PrivacyPolicy.IsPpAccepted();
            }
            
            public static void SetPpAccepted()
            {
                Platform.Features.PrivacyPolicy.SetPpAccepted();
            }

            public class DialogData
            {
                public string title;
                public string content;
                public string button;

                public DialogData(string title, string content, string button)
                {
                    this.title = title;
                    this.content = content;
                    this.button = button;
                }
            }


            public static DialogData GetPpData()
            {
                return Platform.Features.PrivacyPolicy.GetPpData();
            }

            public static DialogData GetTosData()
            {
                return Platform.Features.PrivacyPolicy.GetTosData();
            }
        }

        /// <summary>
        /// Methods to notify MySDK of special game events.
        /// </summary>
        public static class GameEvents
        {
            /// <summary>
            /// This method needs to be called when PLAYER pauses the game. For example Player clicks pause button.
            /// Only use this method when player pauses the game.
            /// </summary>
            public static void OnGamePaused()
            {
                Platform.Features.GameEvents.OnGamePaused();
            }
        }

        /// <summary>
        /// Methods to show correct MyGamez Chinese Customer Support information for this build.
        /// </summary>
        public static class CustomerSupport
        {
            /// <summary>
            /// Checks if it is allowed to show Customer Support Information
            /// </summary>
            /// <returns>True if Customer Support Information should be available to player in game. False if Customer Support Information should not be available to player in game.</returns>
            public static bool IsCustomerSupportAllowed()
            {
                return Platform.Features.CustomerSupport.IsCustomerSupportAllowed();
            }

            /// <summary>
            /// Get the Customer Support Information title. 
            /// </summary>
            /// <returns>Title for customer support dialog. Show this to the player.</returns>
            public static string GetCustomerSupportTitle()
            {
                return Platform.Features.CustomerSupport.GetCustomerSupportTitle();
            }
            /// <summary>
            /// Get the Customer Support Information text. 
            /// </summary>
            /// <returns>Customer Support Information text. Show this to the player.</returns>
            public static string GetCustomerSupportText()
            {
                return Platform.Features.CustomerSupport.GetCustomerSupportText();
            }
        }

        /// <summary>
        /// Methods for social interactions.
        /// </summary>
        public static class Social
        {
            /// <summary>
            /// Share picture and text in social media. Opens list of installed applications that are able to share image and text. Usually player can modify text before sharing.
            /// Use this method for example when player has made new high score. Take screenshot of game showing new high score and pass it to this method with suitable Chinese text. MyGamez support will help with Chinese text.
            /// </summary>
            /// <param name="desc">Text to be shared</param>
            /// <param name="imageAbsolutePath">Absolute path to the image that will be shared</param>
            public static void SharePictureAndText(string desc, string imageAbsolutePath)
            {
                Platform.Features.Social.SharePictureAndText(desc, imageAbsolutePath);
            }
        }

        /// <summary>
        /// Methods to check if text has any forbidden words.
        /// NOTE! The only forbidden word in MySDK Integration version is 'teddybear'.
        /// Final Android Store builds will validate text on MyGamez server. Word list on MyGamez server is updated regularly based on different Chinese requirements. 
        /// </summary>
        public static class TextValidation
        {
            /// <summary>
            /// Validates given text on MyGamez server.
            /// </summary>
            /// <param name="text">Text to be validated.</param>
            /// <param name="callback">Callback that will be triggered when text validation is completed.</param>
            public static void ValidateText(string text, ITextValidationCallback callback)
            {
                Platform.Features.TextValidation.ValidateText(text, callback);
            }

            /// <summary>
            /// Interface for Text Validation Callback.
            /// </summary>
            public interface ITextValidationCallback
            {
                /// <summary>
                /// This method is triggered when text validation is completed.
                /// </summary>
                /// <param name="result">Validation result. Contains text that was validated and validation result code.</param>
                void OnTextValidation(TextValidationResult result);
            }

            /// <summary>
            /// Class to present Text Validation Result.
            /// </summary>
            public class TextValidationResult
            {
                /// <summary>
                /// Text that was validated.
                /// </summary>
                public string Text { get; set; }

                /// <summary>
                /// Validation Result Code. Only allow text to be published if ResultCode is VALID.
                /// </summary>
                public ResultCode ResultCode { get; set; }

                public TextValidationResult(ResultCode code, string text)
                {
                    this.ResultCode = code;
                    this.Text = text;
                }
            }
        }

        /// <summary>
        /// Methods to use MySDK Promo Code system.
        /// Promo Codes are used to give small rewards to player.
        /// MyGamez creates promo codes for a game and provides these codes either directly to a player or to a Chinese Android Store.
        /// Promo codes that are given directly to a player are normally used to compensate for example failed purchase.
        /// Promo codes that are given to a Chinese Android Store are used to get featuring from the Store. For example 10k first installs will get a promo code.
        ///
        /// It is possible to either use MySDK default dialog or create your own dialog for player to type in promo code.
        /// </summary>
        public static class PromoCode
        {
#region Actual API Methods
            /// <summary>
            /// Use this method to get Promo Code Handler from MySDK.
            /// </summary>
            /// <returns>IPromoCodeHandler that can be used to validate promo code and/or mark promo code used.</returns>
            public static IPromoCodeHandler GetPromoCodeHandler()
            {
                return Platform.Features.PromoCode.GetPromoCodeHandler();
            }

            /// <summary>
            /// Interface for PromoCodeHandler methods.
            /// </summary>
            public interface IPromoCodeHandler
            {
                /// <summary>
                /// This will check from MyGamez server if promo code is valid.
                /// This method is only needed if you use your own dialog for player to type in promo code.
                /// </summary>
                /// <param name="promoCode">Promo code that player typed in</param>
                /// <param name="callback">IPromoCodeCallback that will process promo code check result.</param>
                void CheckPromoCode(string promoCode, IPromoCodeCallback callback);

                /// <summary>
                /// Tells MySDK that this promo code is used and player has received the reward.
                /// </summary>
                /// <param name="promoCode">Promo code that was used.</param>
                void SetPromoCodeUsed(string promoCode);
            }

#endregion

            /// <summary>
            /// Interface for promo code callback.
            /// </summary>
            public interface IPromoCodeCallback
            {
                /// <summary>
                /// This method is triggered when promo code has been validate on MyGamez server.
                /// </summary>
                /// <param name="result">Validation result</param>
                void OnResultReceived(PromoCodeResult result);
            }

            /// <summary>
            /// Result of promo code validation
            /// </summary>
            public class PromoCodeResult
            {
                /// <summary>
                /// Array of rewards that should be given to the player
                /// </summary>
                public Advertising.Reward[] Rewards { get; set; }

                /// <summary>
                /// Result code of promo code validation. Only give reward if ResultCode is VALID.
                /// </summary>
                public ResultCode ResultCode { get; set; }

                /// <summary>
                /// Promo code that was validated.
                /// </summary>
                public string PromoCode { get; set; }

                /// <summary>
                /// Verification for this promo code. Verification is created at MyGamez server using game specific private key.
                /// </summary>
                public Security.Verification Verification { get; set; }
            }
        }

        /// <summary>
        /// Methods related to updates.
        /// </summary>
        public static class Update
        {
            public class UpdateData
            {
                public string fileUrl;
                public long sizeBytes;
                public string imageUrl;
                public bool forced;
                public UpdateData(string file, long bytes, string image, bool forced)
                {
                    this.fileUrl = file;
                    this.sizeBytes = bytes;
                    this.imageUrl = image;
                    this.forced = forced;
                }
            }

            public interface IUpdateCheckCallback
            {
                void OnSuccess(UpdateData updateData);
                void OnFailure(ResultCode errorCode, string errorMessage);
            }

            

            /// <summary>
            /// Check for updates. Returns true via callback if an update is available, false otherwise.
            /// If user chooses to start the update, call UpdateVersion() to start the update process.
            /// </summary>
            public static void CheckUpdate(IUpdateCheckCallback callback)
            {
                Platform.Features.Update.CheckUpdate(callback);
            }

            /// <summary>
            /// Start the update process.
            /// </summary>
            public static void UpdateVersion()
            {
                Platform.Features.Update.UpdateVersion();
            }
        }
    }

    /// <summary>
    /// Application related features
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Player wants to exit the game and used either device back button or exit button in game (if game has exit button).
        /// If your game has exit dialog, use this method instead of showing game exit dialog.
        /// </summary>
        /// <param name="callback">IExitCallback that will process exit result</param>
        public static void Exit(IExitCallback callback)
        {
            Platform.App.Exit(callback);
        }

        /// <summary>
        /// This method quits the application. Use this method when callback receives ExitResult EXITING.
        /// NOTE! Use this method instead of Application.Quit(). It is very likely that MyGamez will refactor this method at some point.
        /// </summary>
        public static void QuitApp()
        {
            Platform.App.QuitApp();
        }
        
        /// <summary>
        /// This method restarts the application. Use this method when game needs to restart.
        /// </summary>
        public static void RestartApp()
        {
            Platform.App.RestartApp();
        }

        /// <summary>
        /// Interface for Exit Callback
        /// </summary>
        public interface IExitCallback
        {
            /// <summary>
            /// This method is triggered when MySDK knows the exit result.
            /// </summary>
            /// <param name="result"></param>
            void OnExitResult(ResultCode result);
        }
    }

    /// <summary>
    /// Login state and player info related features.
    /// </summary>
    public static class Login
    {
        /// <summary>
        /// Register ILoginStateListener to MySDK.
        /// ILoginStateListener callback method OnLoginStateChanged when player's login state changes.
        /// It is possible that listener's callback method is triggered right after registering a listener.
        /// </summary>
        /// <param name="listener">ILoginStateListener that will be notified when player's login state changes. </param>
        public static void RegisterLoginStateListener(ILoginStateListener listener)
        {
            Platform.Login.RegisterLoginStateListener(listener);
        }

        /// <summary>
        /// Get LoginInfo of current player.
        /// </summary>
        /// <returns>LoginInfo of current player</returns>
        public static LoginInfo GetLoginInfo()
        {
            return Platform.Login.GetLoginInfo();
        }

        /// <summary>
        /// Requests MySDK to show login dialog to the player.
        /// MySDK will force player to either login or login as a guest.
        /// Callback method of the registered ILoginStateListener will be triggered when player has logged in or logged in as a guest. 
        /// </summary>
        public static void DoLogin(Vendor vendor)
        {
            Platform.Login.DoLogin(vendor);
        }

        /// <summary>
        /// Requests MySDK to show logout dialog to the player.
        /// Callback method of the registered ILoginStateListener will be triggered when player has logged in or logged in as a guest. 
        /// </summary>
        public static void DoLogout()
        {
            Platform.Login.DoLogout();
        }

        /// <summary>
        /// Returns TRUE if MySDK supports in-game login
        /// </summary>
        public static bool IsInGameLoginSupported()
        {
            return Platform.Login.IsInGameLoginSupported();
        }

        public static List<Vendor> GetAvailableVendors()
        {
            return Platform.Login.GetAvailableVendors();
        }

        public static Vendor GetPreferredVendor()
        {
            return Platform.Login.GetPreferredVendor();
        }

        public static Util.ChannelDisplay GetStyleForVendor(Vendor vendor)
        {
            // TODO: store icons in Unity side rather than trying to load them from platform side?
            return Platform.Login.GetStyleForVendor(vendor);
        }

#region Interfaces

        /// <summary>
        /// LoginState Listener with a callback method.
        /// </summary>
        public interface ILoginStateListener
        {
            /// <summary>
            /// This method is triggered when player's LoginState changes.
            /// </summary>
            /// <param name="loginState">New LoginState</param>
            void OnLoginStateChanged(LoginState loginState);
        }

        /// <summary>
        /// LoginCallback callback that is called when login returns a result. Used by iOS SDK
        /// </summary>
        public interface ILoginCallback
        {
            void OnLoginResult(ResultCode code, LoginInfo info);
        }
#endregion
#region Data Holders

        /// <summary>
        /// Information about current player and LoginState.
        /// Includes PlayerID and Verification data if player is logged in.
        /// </summary>
        [System.Serializable]
        public class LoginInfo
        {
            public static string LOGIN_VENDOR_LOCAL;
            /// <summary>
            /// Unique MyGamez PlayerID of logged in the player.
            /// Null if player is not logged in.
            /// </summary>
            public string PlayerID;

            /// <summary>
            /// Verification data of the logged in player.
            /// This data includes PlayerID.
            /// Verification data is created at MyGamez server and signed with private key.
            /// It is possible to validate verification data with correct public key.
            /// </summary>
            public Security.Verification Verification;

            /// <summary>
            /// Current LoginState of the player.
            /// </summary>
            public LoginState LoginState;

            /// <summary>
            /// Login vendor name
            /// </summary>
            public string VendorName;

            public LoginInfo(string playerId, Security.Verification verification, LoginState loginState, string vendorName)
            {
                this.PlayerID = playerId;
                this.Verification = verification;
                this.LoginState = loginState;
                this.VendorName = vendorName;
            }

            public static LoginInfo CreateFromJson(string jsonString)
            {
                return JsonUtility.FromJson<LoginInfo>(jsonString);
            }
        }

        public enum Vendor
        {
            UNKNOWN = -1,
            DEFAULT,
            LOCAL,
            ISBN,
            AURORA,
            BILIBILI,
            HONOR,
            HUAWEI,
            FOUR399,
            MEIZU,
            LOGINIUM,
            OPPO,
            TENCENT_WX,
            TENCENT_QQ,
            VIVO,
            WECHAT3,
            XIAOMI
        }

        [System.Serializable]
        public enum LoginState
        {
            LOGGED_IN,
            LOGIN_FAILED,
            LOGIN_CANCELED,
            LOGGED_OUT,
            LOGOUT_RESTART,
            LOGOUT_FAILED,
            LOGOUT_CANCELED
        }

#endregion
    }

    /// <summary>
    /// MyGamez Anti Addiction System related features
    /// </summary>
    public static class AntiAddiction
    {
        public interface IRidCheckCallback
        {
            void OnResult(string name, string rid, ResultCode resultCode, string message);
        }

        public interface IAntiAddictionEventListener
        {
            void OnAntiAddictionEvent(ResultCode resultCode);
        }

        public interface IAntiAddictionCallback
        {
            void OnAntiAddictionResult(ResultCode resultCode, string msg);
        }

        /// <summary>
        /// Initialize the antiaddiction session for the given playerId
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="callback">Callback for the results from antiaddiction system</param>
        public static void Initialize(string playerId, IAntiAddictionCallback callback)
        {
            Platform.AntiAddiction.Initialize(playerId, callback);
        }

        /// <summary>
        /// Get the current mygamez player id used in antiaddiction. iOS ONLY!
        /// </summary>
        /// <returns> Player id that the antiaddiction session is initialized for, on empty if none</returns>
        public static string GetCurrentMyGamezId()
        {
            return Platform.AntiAddiction.GetCurrentMyGamezId();
        }

        public static ResultCode DeleteUserAccount()
        {
            return Platform.AntiAddiction.DeleteUserAccount();
        }

        /// <summary>
        /// Check the amount how much player is allowed to spend on IAPs. 
        /// </summary>
        /// <returns>Remaining balance in fens. Integer.MAX_VALUE for adult.</returns>
        public static int GetIAPCreditLeft()
        {
            return Platform.AntiAddiction.GetIAPCreditLeft();
        }

        /// <summary>
        /// Check how long player is allowed to play. 
        /// </summary>
        /// <returns>Remaining play time in milliseconds. Zero, if there is no playtime left. Long.MAX_VALUE for adult.</returns>
        public static long GetPlaytimeLeft()
        {
            return Platform.AntiAddiction.GetPlaytimeLeft();
        }

        /// <summary>
        /// Check if player is an adult.
        /// </summary>
        /// <returns>True if player is an adult.
        public static bool IsAdult()
        {
            return Platform.AntiAddiction.IsAdult();
        }
        
        /// <summary>
        /// Checks a rid and name.
        /// </summary>
        public static void AttemptRidCheck(string name, string rid, IRidCheckCallback callback)
        {
            Platform.AntiAddiction.AttemptRidCheck(name, rid, callback);
        }

        /// <summary>
        /// Checks the format of the user inputted string. Note: only checks format, not validity. Use attemptRidCheck to verify user
        /// </summary>
        /// <param name="rid">Rid entered by user</param>
        /// <returns></returns>
        public static bool CheckIdFormat(string rid)
        {
            return Platform.AntiAddiction.CheckIdFormat(rid);
        }

        public static void VerifyIdentity(string name, string rid, IAntiAddictionCallback callback)
        {
            Platform.AntiAddiction.VerifyIdentity(name, rid, callback);
        }

        public static void RequestGameStart(IAntiAddictionCallback callback)
        {
            Platform.AntiAddiction.RequestGameStart(callback);
        }        


        /// <summary>
        /// Request Prompt texts for when RID check fails.
        /// </summary>
        /// <returns>RID Check Fails prompt data.</returns>
        public static PromptDialogData GetRidCheckFailsPromptDialogData()
        {
            return Platform.AntiAddiction.GetRidCheckFailsPromptDialogData();
        }

        /// <summary>
        /// Request Prompt texts for when player identification is completed.
        /// </summary>
        /// <returns>Player Identification Completed prompt data. Null is returned for adult user.</returns>
        public static PromptDialogData GetPlayerIdentificationCompletedPromptDialogData()
        {
            return Platform.AntiAddiction.GetPlayerIdentificationCompletedPromptDialogData();
        }

        /// <summary>
        /// Request Prompt texts for daily game time depletion.
        /// </summary>
        /// <returns>Daily Game Time Depletion prompt data.</returns>
        public static PromptDialogData GetDailyGameTimeDepletionPromptDialogData()
        {
            return Platform.AntiAddiction.GetDailyGameTimeDepletionPromptDialogData();
        }

        /// <summary>
        /// Request Prompt texts for time of day constraint prompt.
        /// </summary>
        /// <returns>Time of Day Constraint prompt data.</returns>
        public static PromptDialogData GetTimeOfDayConstraintPromptDialogData()
        {
            return Platform.AntiAddiction.GetTimeOfDayConstraintPromptDialogData();
        }

        /// <summary>
        /// Request Prompt texts for when player enters store.
        /// </summary>
        /// <returns>Store Enter prompt data. Null is returned for adult user.</returns>
        public static PromptDialogData GetStoreEnterPromptDialogData()
        {
            return Platform.AntiAddiction.GetStoreEnterPromptDialogData();
        }

        /// <summary>
        /// Request Prompt texts for when single purchase limit is exceeded.
        /// </summary>
        /// <returns>Single Purchase Limit Exceeded prompt data. Null is returned for adult user.</returns>
        public static PromptDialogData GetSinglePurchaseLimitExceededPromptDialogData()
        {
            return Platform.AntiAddiction.GetSinglePurchaseLimitExceededPromptDialogData();
        }

        /// <summary>
        /// Request Prompt texts for when monthly purchase limit is exceeded.
        /// </summary>
        /// <returns>Monthly Purchase Limit Exceeded prompt data. Null is returned for adult user.</returns>
        public static PromptDialogData GetMonthlyPurchaseLimitExceededPromptDialogData()
        {
            return Platform.AntiAddiction.GetMonthlyPurchaseLimitExceededPromptDialogData();
        }

        public class PromptDialogData
        {
            /// <summary>
            /// Title text for the Prompt
            /// </summary>
            public string Title { get; }

            /// <summary>
            /// Button text for the Prompt
            /// </summary>
            public string Button { get; }

            /// <summary>
            /// Body text for the Prompt
            /// </summary>
            public string Body { get; }

            public PromptDialogData(string title, string body, string button)
            {
                this.Title = title;
                this.Button = button;
                this.Body = body;
            }
        }

        // iOS only methods, for android call the billing api

        public static void RequestInAppPurchase(float price, IAntiAddictionCallback callback)
        {
            Platform.AntiAddiction.RequestInAppPurchase(price, callback);
        }

        public static void AnnounceCompletedInappPurchase(float price, IAntiAddictionCallback callback)
        {
            Platform.AntiAddiction.AnnounceCompletedInappPurchase(price, callback);
        }



        }
        /// <summary>
        /// MyGamez Oaid implementation
        /// </summary>
        public static class Oaid
    {
        public static void RequestOaid(IOaidRequestCallback callback)
        {
            Platform.Oaid.RequestOaid(callback);
        }

#region Interfaces

        /// <summary>
        /// 
        /// </summary>
        public interface IOaidRequestCallback
        {
            void OnOaidRequestComplete(OaidInfo oaidInfo);
            void OnOaidRequestError(Util.ErrorResponse errorResponse);
        }
#endregion
#region Data Holders
        public class OaidInfo
        {
            public string Oaid { get; }
            public OaidInfo(string oaid)
            {
                this.Oaid = oaid;
            }
        }
#endregion
    }

    public static class Util
    {
        public class ErrorResponse
        {
            public int ErrCode { get; }
            public string ErrMsg { get; }

            public ErrorResponse(int code, string msg)
            {
                this.ErrCode = code;
                this.ErrMsg = msg;
            }
        }

        public class ChannelDisplay
        {
            public string name;
            public Texture2D icon;

            public ChannelDisplay(string name, Texture2D icon)
            {
                this.name = name;
                this.icon = icon;
            }
        }
    }
}
