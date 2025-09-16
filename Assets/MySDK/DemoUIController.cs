using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyGamez.MySDK.Api;
using MyGamez.Demo.MySDKHelpers;
using System;

namespace MyGamez.Demo
{
    public class DemoUIController : MyGamezObserver
    {
        
        public TMPro.TMP_Text goldAmount;
        public DialogWindowController dialogWindow;
        public RIDCheckDialogController RIDCheckDialog;
        public ScrollRect menuItems;
        public GameObject toastMessage;

        private bool playing = false;
        private MySDK.Api.Login.ILoginStateListener loginStateListener;

        // Start is called before the first frame update
        private void Start()
        {
            SetMenuItemsVisibility(false);
            toastMessage.SetActive(false);  // hide until called to show
            ToastMessage.SetToastObject(toastMessage, this);
            if (! MySDK.Api.Features.PrivacyPolicy.IsPpAccepted())
            {
                // Player has not accepted PP & ToS earlier
                ShowPrivacyPolicyAndTosDialog();
            }
            else
            {
                Debug.Log("Going to init MySDK");
                MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
            }
        }

        private void ShowPrivacyPolicyAndTosDialog()
        {
            // Show dialog to the player.
            dialogWindow.setTitleText("TOS & PP");
            dialogWindow.setMessageText("To continue, you must accept the Privacy Policy and Terms of Service.");
            dialogWindow.setLeftText("Don't agree");
            dialogWindow.setLeftCallback(
                delegate
                {
                    dialogWindow.hide();
                    MySDK.Api.App.QuitApp();
                });
            dialogWindow.setRightButtonActive(true);
            dialogWindow.setRightText("Agree");
            dialogWindow.setRightCallback(
                delegate
                {
                    // Demo code
                    dialogWindow.hide();

                    // playing = Player clicked in-game button to check privacy policy
                    // !playing = First start and need to initialise MySDK
                    if (!playing)
                    {
                        MySDK.Api.Features.PrivacyPolicy.SetPpAccepted();

                        // Initialise MySDK
                        Debug.Log("Going to init MySDK");
                        MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
                    }

                });
            Debug.Log("Show PP Dialog");
            dialogWindow.show();
        }

        private int mySdkInitCounter = 0;
        /// <summary>
        /// This is one of the MySDKHelpers.MyGamezObserver's methods. Called when MySDK has been initialised.
        /// </summary>
        /// <param name="result">Initialisation result</param>
        public override void OnMySDKInit(MySDKInit.MySDKInitResult result)
        {
            mySdkInitCounter++;
            ToastMessage.Show("MySDK Init result is " + result.ResultCode + " " + result.ResultMsg, 1);
            Debug.Log("MySDK Init result is " + result.ResultCode + " " + result.ResultMsg);

            switch (result.ResultCode)
            {
                case ResultCode.SUCCESS:
                case ResultCode.ALREADY_DONE:
                    // Initialisation is completed. Move on to player login.
                    Login();
                    break;
                case ResultCode.PP_AND_TOS_NOT_ACCEPTED:
                    // Show Privacy Policy Dialog
                    ShowPrivacyPolicyAndTosDialog();
                    break;
                case ResultCode.SERVER_ERROR:
                case ResultCode.NETWORK_ERROR:
                case ResultCode.GENERAL_ERROR:
                    // Retry once
                    if (mySdkInitCounter < 2)
                        MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
                    else
                        ShowErrorDialog();
                    break;
                
                
            }
        }

        private void ShowErrorDialog()
        {
            // Show dialog to the player.
            dialogWindow.setTitleText("Error occurred repeatedly");
            dialogWindow.setMessageText("System encoutered errors repeatedly. Please check internet connection and try again later.");
            dialogWindow.setLeftText("OK");
            dialogWindow.setLeftCallback(
                delegate
                {
                    dialogWindow.hide();
                    MySDK.Api.App.QuitApp();
                    
                });
            dialogWindow.setRightButtonActive(false);
            dialogWindow.show();
        }


        private void Login()
        {   
            if (loginStateListener == null)
            {
                // Set LoginStateListener
                // Listener's OnLoginStateChanged method will be triggered when player's LoginState changes.
                loginStateListener = new MySDKHelpers.LoginListenerExample(this);
                MySDK.Api.Login.RegisterLoginStateListener(loginStateListener);
            }
            List<MySDK.Api.Login.Vendor> vendors = MySDK.Api.Login.GetAvailableVendors();
            Debug.Log("Vendors available: " + vendors.ToString());

            // ISBN version always has only one vendor
            MySDK.Api.Login.DoLogin(vendors[0]);
        }

        /// <summary>
        /// This is one of the MySDKHelpers.MyGamezObserver's methods. Called when player's LoginState has changed.
        /// </summary>
        /// <param name="loginState">Current updated State</param>
        public override void OnLoginStateChanged(MySDK.Api.Login.LoginState loginState)
        {
            Debug.Log("mysdk DemoUIController::OnLoginStateChanged() loginState=" + loginState.ToString());

            switch (loginState)
            {
                case MySDK.Api.Login.LoginState.LOGGED_IN:
                    InitializeAntiaddiction();
                    break;
                case MySDK.Api.Login.LoginState.LOGIN_FAILED:
                    ToastMessage.Show("Failed to login");
                    if (!playing) // Not in game yet, open login again to retry
                    {
                        Login();
                    }
                    break;
                case MySDK.Api.Login.LoginState.LOGIN_CANCELED:
                    ToastMessage.Show("User canceled login");
                    break;
                case MySDK.Api.Login.LoginState.LOGGED_OUT:
                    ToastMessage.Show("Successfully logged out");
                    // MySDK does not request restart but demo will restart to show login.
                    MySDK.Api.App.RestartApp();
                    break;
                case MySDK.Api.Login.LoginState.LOGOUT_RESTART:
                    ToastMessage.Show("Logout restart");
                    MySDK.Api.App.RestartApp();
                    break;
                case MySDK.Api.Login.LoginState.LOGOUT_FAILED:
                    ToastMessage.Show("Failed to logout");
                    break;
                case MySDK.Api.Login.LoginState.LOGOUT_CANCELED:
                    ToastMessage.Show("User canceled logout");
                    break;

            }
        }

        private int aaInitCounter = 0;
        private void InitializeAntiaddiction()
        {
            aaInitCounter++;
            string playerId = MySDK.Api.Login.GetLoginInfo().PlayerID;
            if (playerId == null || playerId.Length == 0)
            {
                Debug.Log("Error: failed to get player id, can not initialize antiaddiction without it");
                return;
            }
            Debug.Log("Going to initialize antiaddiction for playerId: " + playerId);
            MySDK.Api.AntiAddiction.Initialize(playerId, new MySDKHelpers.AntiAddictionCallback(this));
        }

        

        /// <summary>
        /// This is one of the MySDKHelpers.MyGamezObserver's methods. Called when AntiAddiction has been initialised.
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="msg"></param>
        public override void OnAntiAddictionInitResult(ResultCode resultCode, string msg)
        {
            Debug.Log("Antiaddiction init resultCode: " + resultCode.ToString());
            Debug.Log("Antiaddiction init msg: " + msg);
            switch (resultCode)
            {
                case ResultCode.SUCCESS:
                    RequestGameStart();
                    break;
                case ResultCode.NEED_TO_SHOW_DIALOG:
                    ShowRIDCheckDialog();
                    break;
                case ResultCode.TIME_OVER:
                case ResultCode.DISALLOWED:
                    ShowTimeOutDialog();
                    break;
                case ResultCode.SDK_BUSY:
                    InitializeAntiaddiction();
                    break;
                case ResultCode.GENERAL_ERROR:
                    ToastMessage.Show("Unexpected error, trying again", 1);
                    if (aaInitCounter < 2)
                        InitializeAntiaddiction();
                    else
                        ShowErrorDialog();
                    break;
            }
        }

        private void ShowTimeOutDialog()
        {
            // Get Prompt data
            MySDK.Api.AntiAddiction.PromptDialogData data = MySDK.Api.AntiAddiction.GetTimeOfDayConstraintPromptDialogData();

            // Show dialog to the player.
            dialogWindow.setTitleText(data.Title);
            dialogWindow.setMessageText(data.Body);
            dialogWindow.setLeftText(data.Button);
            dialogWindow.setLeftCallback(
                delegate
                {
                    dialogWindow.hide();
                    MySDK.Api.App.QuitApp();
                });
            dialogWindow.setRightButtonActive(false);
            dialogWindow.show();
        }

        private void ShowRIDCheckDialog()
        {
            RIDCheckDialog.SetValidateClickedCallback(
                delegate {
                    MySDK.Api.AntiAddiction.AttemptRidCheck(RIDCheckDialog.GetName(), RIDCheckDialog.GetRIN(), new MySDKHelpers.RIDCheckValidationListener(this));
                    RIDCheckDialog.Hide();
                });
            RIDCheckDialog.Show();
        }

        /// <summary>
        /// This is one of the MySDKHelpers.MyGamezObserver's methods. Called when RID Check has been completed.
        /// </summary>
        /// <param name="rid">Residental ID</param>
        /// <param name="name">Player Name</param>
        /// <param name="resultCode">RID Check result</param>
        /// <param name="msg">Additional info on the result</param>
        public override void OnRIDCheckResult(string rid, string name, ResultCode resultCode, string msg)
        {
            ToastMessage.Show("RID Check result is " + resultCode.ToString(), ToastMessage.LENGTH_LONG);
            Debug.Log("RID Check result is " + resultCode.ToString());

            switch (resultCode)
            {
                case ResultCode.SUCCESS:
                    // RID Check was completed successfully - Request Game Start
                    RequestGameStart();
                    break;
                case ResultCode.INVALID:
                    // Player has typed invalid name or ID. Indicate something was wrong and show RID Check dialog again.
                    ShowRIDCheckDialog();
                    break;

                case ResultCode.DISALLOWED:
                case ResultCode.TIME_OVER:
                    // Minor players are not allowed to play during these hours.
                    // Show dialog to player and quit app
                    ShowTimeOutDialog();
                    break;
                case ResultCode.CONFIG_ERROR:
                case ResultCode.GENERAL_ERROR:
                    // Something went wrong. Indicate this to the player and start over from MySDK Initialisation.
                    MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
                    break;  
            }
        }

        private int requestGameStartCounter = 0;
        private void RequestGameStart()
        {
            requestGameStartCounter++;
            // Player can start to play!
            // First notify MySDK that player is starting to play.
            MySDK.Api.AntiAddiction.RequestGameStart(new MySDKHelpers.AntiAddictionEventListener(this));

        }

        /// <summary>
        /// This is one of the MySDKHelpers.MyGamezObserver's methods. Called when RequestGameStart has been processed.
        /// </summary>
        /// <param name="resultCode"></param>
        public override void OnAntiAddictionEvent(ResultCode resultCode, string msg)
        {
            Debug.Log("OnAntiAddictionEvent, resultCode " + resultCode.ToString());
            Debug.Log("OnAntiAddictionEvent, msg " + msg);
            switch (resultCode)
            {
                case ResultCode.SUCCESS:
                    ShowRestrictions();
                    break;
                case ResultCode.NEED_TO_SHOW_DIALOG:
                    ShowRIDCheckDialog();
                    break;
                case ResultCode.TIME_OVER:
                case ResultCode.DISALLOWED:
                    ShowTimeOutDialog();
                    break;
                case ResultCode.SDK_BUSY:
                    // Should wait a second or two first
                    MySDK.Api.AntiAddiction.RequestGameStart(new MySDKHelpers.AntiAddictionEventListener(this));
                    break;
                case ResultCode.CONFIG_ERROR:
                case ResultCode.GENERAL_ERROR:
                default:
                    if (requestGameStartCounter < 2)
                        MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
                    else
                        ShowErrorDialog();
                    break;
            }
        }

        private void ShowRestrictions()
        {
            if (MySDK.Api.AntiAddiction.IsAdult())
                StartGame();
            else
            {
                MySDK.Api.AntiAddiction.PromptDialogData data = MySDK.Api.AntiAddiction.GetPlayerIdentificationCompletedPromptDialogData();
                // Show dialog to the player.
                dialogWindow.setTitleText(data.Title);
                dialogWindow.setMessageText(data.Body);
                dialogWindow.setLeftText(data.Button);
                dialogWindow.setLeftCallback(
                    delegate
                    {
                        dialogWindow.hide();
                        StartGame();
                    });
                dialogWindow.setRightButtonActive(false);
                dialogWindow.show();
            }

        }


        

        

        

        

        


        private void StartGame()
        {
            Debug.Log("Starting game");
            playing = true;
            // Set Payment callback to MySDK (for Android only, iOS has different payment related methods)
            // Callback will be triggered when player exits payment process
            // This example callback is defined in ClassesToUseMySDK.cs
            MySDKHelpers.PayCallbackExample exampleCallbackPayment = new MySDKHelpers.PayCallbackExample(this);
            MySDK.Api.Billing.SetPayCallback(exampleCallbackPayment);

            // Get player LoginInfo
            MySDK.Api.Login.LoginInfo loginInfo = MySDK.Api.Login.GetLoginInfo();
            // Use loginInfo.PlayerID to load & save progress - this demo does not save progress.

            // update total gold
            OnGoldUpdated();

            // Show menu items
            SetMenuItemsVisibility(true);
        }

        public override void OnGoldUpdated(int gold = 0)
        {
            Debug.Log("mysdk DemoUIController::updateTotalGold() gold=" + gold.ToString());
            int totalGold = PlayerPrefs.GetInt("gold", 0);
            totalGold += gold;
            goldAmount.text = totalGold.ToString();
            PlayerPrefs.SetInt("gold", totalGold);
            PlayerPrefs.Save();
        }

        private void SetMenuItemsVisibility(bool IsVisible)
        {
            // Get the Content transform of the ScrollView
            Transform contentPanel = menuItems.content;

            // Find all buttons under the Content panel
            Button[] buttons = contentPanel.GetComponentsInChildren<Button>(true);

            foreach (var button in buttons)
            {
                button.gameObject.SetActive(IsVisible);
            }


            Toggle[] toggles = contentPanel.GetComponentsInChildren<Toggle>(true);
            foreach (var toggle in toggles)
            {
                toggle.gameObject.SetActive(IsVisible);
            }
        }

        public void OnBuy50GoldButtonClicked()
        {
            // This code demonstrates how to trigger payment in Android MySDK (iOS below).
            // Step 1: Create IAPInfo
            // IAPInfo is for player. It has basic information of this purchase (price, name and description).
            // Payment providers usually show IAP Info to the player in their payment UI.
            // NOTE: Price is in Fens (Chinese cents). 100 Fens = 1 Chinese Yuan. Use only numbers that are divisible by 10.
            // NOTE: Use Chinese in Name and Description
            int price = 100;
            string name = "50 Gold";
            string description = "50 shining gold pieces";
            MySDK.Api.Billing.IAPInfo iapInfo = new MySDK.Api.Billing.IAPInfo(price, name, description);

            // Step 2: Create PayInfo
            // PayInfo is for the game itself. It is used to identify purchase in PayCallback.
            // CustomID can be unique ID for this purchase for example
            // ExtraInfo can be whatever extra info you wish to add to this purchase. Some tracking ID for example.
            // NOTE: CustomID can be max. 128 characters long. ExtraInfo can be max. 65535 characters long.
            string customID = "iap-50-gold-1234567890";
            string extraInfo = "Some extra info about this purchase. Whatever data you need when payment is completed. Maybe some ID for logging for example. Can be pretty long.";
            MySDK.Api.Billing.PayInfo payInfo = new MySDK.Api.Billing.PayInfo(iapInfo, customID, extraInfo);

            // Step 3: Figure out which biller to use. ISBN version has only one biller.
            // Step 4: Start the billing process with selected biller and payInfo.
            // MySDK will take control.
            // Registered PayCallback will be triggered when player finishes payment process.
            // NOTE: MySDK will popup necessary biller dialogs on top of game UI.
            List<MySDK.Api.Billing.Biller> billers = MySDK.Api.Billing.GetAvailableBillers();
            MySDK.Api.Billing.DoBilling(billers[0], payInfo);
        }





        public void OnValidateTextButtonClicked()
        {
            // Callback example is in ClassesToUseMySDK.cs
            MySDK.Api.Features.TextValidation.ITextValidationCallback callback = new MyGamez.Demo.MySDKHelpers.TextValidationCallbackExample();
            // Trigger Text Validation
            MySDK.Api.Features.TextValidation.ValidateText("my little teddy bear", callback);
        }

        public void OnStoreEnteredButtonClicked()
        {
            Debug.Log("mysdk OnRequestStorePromptDataClicked()");
            MySDK.Api.AntiAddiction.PromptDialogData data = MySDK.Api.AntiAddiction.GetStoreEnterPromptDialogData();
            if (data != null)
            {
                dialogWindow.setTitleText(data.Title);
                dialogWindow.setMessageText(data.Body);
                dialogWindow.setLeftText(data.Button);
                dialogWindow.setLeftCallback(
                    delegate
                    {
                        dialogWindow.hide();

                    });
                dialogWindow.setRightButtonActive(false);
                dialogWindow.show();
            }
            else
            {
                ToastMessage.Show("Player is adult. No need to show Store Prompt.", ToastMessage.LENGTH_LONG);
            }
        }

        public void OnPrivacyPolicyButtonClicked()
        {
            Debug.Log("mysdk onPrivacyPolicyButtonClicked()");
            ShowPrivacyPolicyAndTosDialog();
        }

        public void OnGetLoginInfoButtonClicked()
        {
            Debug.Log("mysdk onGetLoginInfoButtonClicked()");
            MySDK.Api.Login.LoginInfo loginInfo = MySDK.Api.Login.GetLoginInfo();
            ToastMessage.Show(MySDKHelpers.LoginInfoHelper.LoginInfoToString(loginInfo), ToastMessage.LENGTH_LONG);
        }

        public void OnLogoutButtonClicked()
        {
            MySDK.Api.Login.DoLogout();
        }

        public void OnGetRemainingBalanceButtonClicked()
        {
            Debug.Log("mysdk OnGetRemainingBalanceButtonClicked()");
            int balance = MySDK.Api.AntiAddiction.GetIAPCreditLeft();
            if (balance == int.MaxValue)
                ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
            else
                ToastMessage.Show("Remaining balance is " + balance, ToastMessage.LENGTH_LONG);
        }

        public void OnGetRemainingPlaytimeButtonClicked()
        {
            Debug.Log("mysdk OnGetRemainingPlaytimeButtonClicked()");
            long playtime = MySDK.Api.AntiAddiction.GetPlaytimeLeft();
            if (playtime == long.MaxValue)
                ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
            else
                ToastMessage.Show("Remaining playtime in ms is " + playtime, ToastMessage.LENGTH_LONG);
        }
    }
}