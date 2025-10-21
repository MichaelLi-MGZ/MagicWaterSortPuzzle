using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyGamez.MySDK.Api;
using MyGamez.Demo.MySDKHelpers;
using System;
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif

namespace MyGamez.Demo
{
    public class DemoUIController : MyGamezObserver
    {
        
        public TMPro.TMP_Text goldAmount;
        public DialogWindowController dialogWindow;
        public SingleButtonDialogWindowController singleButtonDialogWindow;
        public DialogWindowController warningDialogWindow;
        public RIDCheckDialogController RIDCheckDialog;
        public GameObject toastMessage;
        public NotificationBackground notificationBackground;
        public AgeAppropriateWindowController ageAppropriateWindowController; // Reference to window controller

        private bool playing = false;
        private MySDK.Api.Login.ILoginStateListener loginStateListener;
        
        // Window visibility monitoring
        private bool previousWindowVisible = false;
        private float visibilityCheckInterval = 0.5f; // Check every 0.5 seconds
        private float lastVisibilityCheckTime = 0f;
        private bool loginPending = false; // Track if login is waiting for window to be hidden

#if UNITY_IOS
        // Apple Sign-In
        private IAppleAuthManager appleAuthManager;
        private bool appleSignInCompleted = false;
        private string appleUserId = "";
        private string appleIdToken = "";
#endif

        private void Awake()
        {
            // Make this object persist across scene changes
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loading events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from scene loading events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void Update()
        {
            // Monitor window visibility changes
            MonitorWindowVisibility();
            
#if UNITY_IOS
            // Update Apple Sign-In manager
            appleAuthManager?.Update();
#endif
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            Debug.Log($"DemoUIController: Scene loaded - '{scene.name}' (mode: {mode})");
            
            // Hide notification background when Game scene loads
            if (scene.name == "Game" && notificationBackground != null)
            {
                Debug.Log($"DemoUIController: Hiding notification background for Game scene (currently visible: {notificationBackground.IsVisible()})");
                notificationBackground.Hide();
                Debug.Log($"DemoUIController: Notification background hidden (now visible: {notificationBackground.IsVisible()})");
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            toastMessage.SetActive(false);  // hide until called to show
            ToastMessage.SetToastObject(toastMessage, this);
            
#if UNITY_IOS
            // Initialize Apple Sign-In
            InitializeAppleSignIn();
#endif
            
            if (! MySDK.Api.Features.PrivacyPolicy.IsPpAccepted())
            {
                // Player has not accepted PP & ToS earlier
                ShowPrivacyPolicyAndTosDialog();
            }
            else
            {
                Debug.Log("Going to init MySDK");
                InitializeMySDK();
            }
        }

        private void InitializeMySDK()
        {
#if UNITY_ANDROID
            // Android SDK initialization
            MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
#elif UNITY_IOS
            // iOS SDK initialization - show Apple Sign-In first
            ShowAppleSignInDialog();
#else
            // Editor or other platforms - use Android SDK for testing
            MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
#endif
        }

        private void InitializeIOSMySDK()
        {
            Debug.Log("Initializing iOS MySDK");
            // iOS SDK configuration - you may need to adjust these values
            string cpid = "your_cpid_here"; // Replace with actual CPID
            string authParams = "{}"; // Replace with actual auth parameters
            string backendUrl = "https://your-backend-url.com"; // Replace with actual backend URL
            
            MyGamezGameObject.DoInitialize(cpid, backendUrl, authParams, OnIOSSDKInitialized);
        }

#if UNITY_IOS
        private void InitializeAppleSignIn()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                appleAuthManager = new AppleAuthManager(deserializer);
                Debug.Log("Apple Sign-In initialized successfully");
            }
            else
            {
                Debug.LogError("Apple Sign-In is not supported on this platform");
            }
        }

        private void ShowAppleSignInDialog()
        {
            Debug.Log("Showing Apple Sign-In dialog");
            
            if (appleAuthManager == null)
            {
                Debug.LogError("Apple Sign-In manager not initialized");
                return;
            }

            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

            appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    Debug.Log("Apple Sign-In successful");
                    if (credential is IAppleIDCredential appleIdCredential)
                    {
                        appleUserId = appleIdCredential.User;
                        appleIdToken = System.Text.Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                        appleSignInCompleted = true;
                        
                        Debug.Log($"Apple User ID: {appleUserId}");
                        Debug.Log($"Apple ID Token: {appleIdToken}");
                        
                        // Now initialize MySDK with Apple authentication
                        InitializeIOSMySDKWithAppleAuth();
                    }
                },
                error =>
                {
                    Debug.LogError("Apple Sign-In failed: " + error);
                    ToastMessage.Show("Apple Sign-In failed. Please try again.");
                    // Show Apple Sign-In dialog again
                    ShowAppleSignInDialog();
                });
        }

        private void InitializeIOSMySDKWithAppleAuth()
        {
            Debug.Log("Initializing iOS MySDK with Apple authentication");
            // iOS SDK configuration with Apple authentication
            string cpid = "mygamez_pw"; // Replace with actual CPID
            string fake_authParams = $"{{\"pw\":\"3b68f6085d578ef0a9a5af47531d3e7a\",\"app\":\"test-app\",\"player_id\":\"{appleUserId}\"}}";
            //string authParams = $"{{\"appleUserId\":\"{appleUserId}\",\"appleIdToken\":\"{appleIdToken}\"}}"; // Include Apple auth data
            string backendUrl = "https://antiaddiction.dev.mygamez.cn/api/v1/usr"; // Replace with actual backend URL
            //url = "https://antiaddiction.myservicez.cn/api/v1/usr" // prod
            //url = "https://antiaddiction.dev.mygamez.cn/api/v1/usr" // dev
            
            MyGamezGameObject.DoInitialize(cpid, backendUrl, fake_authParams, OnIOSSDKInitialized);
        }
#endif

        private void OnIOSSDKInitialized(MyGamezBridge.EventCode eventCode)
        {
            Debug.Log("iOS MySDK initialization result: " + eventCode);
            
            switch (eventCode)
            {
                case MyGamezBridge.EventCode.UserRightsDetermined:
                    Debug.Log("iOS MySDK: User rights determined, starting game flow");
                    RequestIOSGameStart();
                    break;
                case MyGamezBridge.EventCode.RidCheckRequired:
                    Debug.Log("iOS MySDK: RID check required");
                    ShowRIDCheckDialog();
                    break;
                case MyGamezBridge.EventCode.GuestModeNotGranted:
                    Debug.Log("iOS MySDK: Guest mode not granted");
                    ShowErrorDialog();
                    break;
                case MyGamezBridge.EventCode.GeneralError:
                    Debug.Log("iOS MySDK: General error occurred");
                    ShowErrorDialog();
                    break;
                default:
                    Debug.Log("iOS MySDK: Unknown event code: " + eventCode);
                    break;
            }
        }

        private void RequestIOSGameStart()
        {
            Debug.Log("Requesting iOS game start");
            MyGamezGameObject.DoStart(OnIOSGameStartResult);
        }

        private void OnIOSGameStartResult(MyGamezBridge.EventCode eventCode)
        {
            Debug.Log("iOS game start result: " + eventCode);
            
            switch (eventCode)
            {
                case MyGamezBridge.EventCode.GameStartAllowed:
                    Debug.Log("iOS MySDK: Game start allowed");
                    StartGame();
                    break;
                case MyGamezBridge.EventCode.GuestModeGameTimeDepleted:
                case MyGamezBridge.EventCode.DailyGameTimeDepleted:
                    Debug.Log("iOS MySDK: Game time depleted");
                    ShowTimeOutDialog();
                    break;
                case MyGamezBridge.EventCode.PlayingNotAllowedDueToTimeOfDayConstraints:
                    Debug.Log("iOS MySDK: Playing not allowed due to time constraints");
                    ShowTimeOutDialog();
                    break;
                case MyGamezBridge.EventCode.RidCheckRequired:
                    Debug.Log("iOS MySDK: RID check required");
                    ShowRIDCheckDialog();
                    break;
                default:
                    Debug.Log("iOS MySDK: Unknown game start event: " + eventCode);
                    break;
            }
        }

        public void ShowPrivacyPolicyAndTosDialog()
        {
            // Show dialog to the player.
            dialogWindow.setLeftCallback(
                delegate
                {
                    ShowWarningDialog();
                });
            dialogWindow.setRightButtonActive(true);
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
#if UNITY_IOS
                        // Show Apple Sign-In dialog first on iOS
                        ShowAppleSignInDialog();
#else
                        InitializeMySDK();
#endif
                    }

                });
            Debug.Log("Show PP Dialog");
            dialogWindow.show();
        }

        private void ShowWarningDialog()
        {
            // Show dialog to the player.
            warningDialogWindow.setLeftCallback(
                delegate
                {
                    warningDialogWindow.hide();
                    // show warning dialog
                    MySDK.Api.App.QuitApp();
                });
            warningDialogWindow.setRightButtonActive(true);
            warningDialogWindow.setRightCallback(
                delegate
                {
                    // Demo code
                    warningDialogWindow.hide();
                });
            Debug.Log("Show PP Dialog");
            warningDialogWindow.show();
        }

        private int mySdkInitCounter = 0;
        /// <summary>
        /// This is one of the MySDKHelpers.MyGamezObserver's methods. Called when MySDK has been initialised.
        /// </summary>
        /// <param name="result">Initialisation result</param>
        public override void OnMySDKInit(MySDKInit.MySDKInitResult result)
        {
            mySdkInitCounter++;
            //ToastMessage.Show("MySDK Init result is " + result.ResultCode + " " + result.ResultMsg, 1);
            Debug.Log("MySDK Init result is " + result.ResultCode + " " + result.ResultMsg);

            switch (result.ResultCode)
            {
                case ResultCode.SUCCESS:
                case ResultCode.ALREADY_DONE:
                    // Initialisation is completed. Show progress bar before login.
                    ShowProgressAndLogin();
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

        /// <summary>
        /// Monitor window visibility and handle login accordingly
        /// </summary>
        private void MonitorWindowVisibility()
        {
            // Check at specified intervals
            if (Time.time - lastVisibilityCheckTime < visibilityCheckInterval)
                return;
                
            lastVisibilityCheckTime = Time.time;
            
            // Get the current window controller (auto-find if needed)
            AgeAppropriateWindowController controller = GetAgeAppropriateWindowController();
            if (controller == null)
                return;
            
            bool currentWindowVisible = controller.IsWindowVisible();
            
            // Check for state changes
            if (currentWindowVisible != previousWindowVisible)
            {
                // If window became hidden and login is pending, proceed with login
                if (!currentWindowVisible && previousWindowVisible && loginPending)
                {
                    loginPending = false;
                    Login();
                }
                
                previousWindowVisible = currentWindowVisible;
            }
        }
        
        /// <summary>
        /// Show progress bar for 8 seconds then call Login() (with window visibility check)
        /// </summary>
        private void ShowProgressAndLogin()
        {
            if (notificationBackground != null)
            {
                // Show the notification background with progress bar
                notificationBackground.Show();
                
                // Start the 8-second progress bar, then check window visibility before login
                notificationBackground.StartProgress(() => {
                    CheckWindowVisibilityAndLogin();
                });
            }
            else
            {
                CheckWindowVisibilityAndLogin();
            }
        }
        
        /// <summary>
        /// Get the AgeAppropriateWindowController instance, auto-find if not assigned
        /// </summary>
        private AgeAppropriateWindowController GetAgeAppropriateWindowController()
        {
            // If manually assigned, verify it's the correct one by checking if it has an active window
            if (ageAppropriateWindowController != null)
            {
                bool hasActiveWindow = ageAppropriateWindowController.ageAppropriateWindow != null && 
                                     ageAppropriateWindowController.ageAppropriateWindow.activeInHierarchy;
                
                // If the manually assigned one has an active window, use it
                if (hasActiveWindow)
                {
                    return ageAppropriateWindowController;
                }
            }
            
            // Auto-find instances and try to find the one with an active window first
            AgeAppropriateWindowController[] allControllers = FindObjectsOfType<AgeAppropriateWindowController>();
            
            foreach (var controller in allControllers)
            {
                if (controller.ageAppropriateWindow != null && controller.ageAppropriateWindow.activeInHierarchy)
                {
                    return controller;
                }
            }
            
            // If no active window, return the first one found
            if (allControllers.Length > 0)
            {
                return allControllers[0];
            }
            
            return null;
        }
        
        /// <summary>
        /// Check window visibility and either login immediately or wait for window to be hidden
        /// </summary>
        private void CheckWindowVisibilityAndLogin()
        {
            AgeAppropriateWindowController controller = GetAgeAppropriateWindowController();
            
            if (controller == null)
            {
                Login();
                return;
            }
            
            bool isWindowVisible = controller.IsWindowVisible();
            
            if (!isWindowVisible)
            {
                // Window is not visible, proceed with login immediately
                Login();
            }
            else
            {
                // Window is visible, wait for it to become hidden
                loginPending = true;
                previousWindowVisible = true; // Set initial state
            }
        }

        private void ShowErrorDialog()
        {
            // Show dialog to the player.
            singleButtonDialogWindow.setTitleText("发生错误");
            singleButtonDialogWindow.setMessageText("系统反复出现错误。请检查网络连接，稍后再试。");
            singleButtonDialogWindow.setLeftText("确定");
            singleButtonDialogWindow.setLeftCallback(
                delegate
                {
                    singleButtonDialogWindow.hide();
                    MySDK.Api.App.QuitApp();
                    
                });
            singleButtonDialogWindow.show();
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
                    Debug.Log("DemoUIController: User logged in successfully, initializing anti-addiction");
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
            Debug.Log($"DemoUIController: Initializing anti-addiction (attempt {aaInitCounter})");
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
                case ResultCode.LIMITED:
                    ShowLimitedDialog();
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

        private void ShowLimitedDialog()
        {
            //No Limited Restrictions for now. just use timeout for temporary use.
            ShowTimeOutDialog();
        }

        private void ShowTimeOutDialog()
        {
            // Get Prompt data
            MySDK.Api.AntiAddiction.PromptDialogData data = MySDK.Api.AntiAddiction.GetTimeOfDayConstraintPromptDialogData();

            // Show dialog to the player.
            singleButtonDialogWindow.setTitleText(data.Title);
            singleButtonDialogWindow.setMessageText(data.Body);
            singleButtonDialogWindow.setLeftText(data.Button);
            singleButtonDialogWindow.setLeftCallback(
                delegate
                {
                    singleButtonDialogWindow.hide();
                    MySDK.Api.App.QuitApp();
                });
            singleButtonDialogWindow.show();
        }

        private void ShowRIDCheckDialog()
        {
            RIDCheckDialog.SetValidateClickedCallback(
                delegate {
#if UNITY_ANDROID
                    MySDK.Api.AntiAddiction.AttemptRidCheck(RIDCheckDialog.GetName(), RIDCheckDialog.GetRIN(), new MySDKHelpers.RIDCheckValidationListener(this));
#elif UNITY_IOS
                    MyGamezGameObject.DoAttemptRidCheck(RIDCheckDialog.GetName(), RIDCheckDialog.GetRIN(), OnIOSRidCheckResult);
#endif
                    RIDCheckDialog.Hide();
                });
            RIDCheckDialog.Show();
        }

        private void OnIOSRidCheckResult(MyGamezBridge.EventCode eventCode)
        {
            Debug.Log("iOS RID check result: " + eventCode);
            
            switch (eventCode)
            {
                case MyGamezBridge.EventCode.UserRightsDetermined:
                    Debug.Log("iOS MySDK: RID check successful, user rights determined");
                    RequestIOSGameStart();
                    break;
                case MyGamezBridge.EventCode.RidCheckRequired:
                    Debug.Log("iOS MySDK: RID check failed, showing dialog again");
                    ShowRIDCheckDialog();
                    break;
                case MyGamezBridge.EventCode.DailyGameTimeDepleted:
                case MyGamezBridge.EventCode.PlayingNotAllowedDueToTimeOfDayConstraints:
                    Debug.Log("iOS MySDK: Playing not allowed due to time constraints");
                    ShowTimeOutDialog();
                    break;
                case MyGamezBridge.EventCode.GeneralError:
                    Debug.Log("iOS MySDK: General error during RID check");
                    ShowErrorDialog();
                    break;
                default:
                    Debug.Log("iOS MySDK: Unknown RID check result: " + eventCode);
                    break;
            }
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
                case ResultCode.LIMITED:
                    // Minor players are not allowed to play during these hours.
                    // Show dialog to player and quit app
                    ShowLimitedDialog();
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
                case ResultCode.LIMITED:
                    ShowLimitedDialog();
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
            Debug.Log("DemoUIController: Showing restrictions check");
            if (MySDK.Api.AntiAddiction.IsAdult())
            {
                Debug.Log("DemoUIController: Player is adult, starting game");
                StartGame();
            }
            else
            {
                MySDK.Api.AntiAddiction.PromptDialogData data = MySDK.Api.AntiAddiction.GetPlayerIdentificationCompletedPromptDialogData();
                // Show dialog to the player.
                Debug.Log("ShowRestrictions(): MessageText: " + data.Body);
                Debug.Log("ShowRestrictions(): Title: " + data.Title);
                singleButtonDialogWindow.setTitleText(data.Title);
                singleButtonDialogWindow.setMessageText(data.Body);
                singleButtonDialogWindow.setLeftText(data.Button);
                singleButtonDialogWindow.setLeftCallback(
                    delegate
                    {
                        singleButtonDialogWindow.hide();
                        StartGame();
                    });
                singleButtonDialogWindow.show();
            }

        }

        private void StartGame()
        {
            Debug.Log("DemoUIController: Starting game - setting up payment callbacks and loading Game scene");
            playing = true;
            // Set Payment callbacks per platform
#if UNITY_ANDROID
            // Android: use MySDK billing callback
            MySDKHelpers.PayCallbackExample exampleCallbackPayment = new MySDKHelpers.PayCallbackExample(this);
            MySDK.Api.Billing.SetPayCallback(exampleCallbackPayment);
#elif UNITY_IOS
            // iOS: prepare MyGamez iOS IAP acknowledgements (purchase is requested via OnBuy50GoldButtonClicked)
            // No explicit callback registration API exposed in MyGamez iOS bridge; we will acknowledge after purchase succeeds
            // Example: MyGamezGameObject.DoAnnounceCompletedInappPurchase(price, OnIOSPurchaseAcknowledged);
#endif

            // Get player identity per platform
#if UNITY_ANDROID
            MySDK.Api.Login.LoginInfo loginInfo = MySDK.Api.Login.GetLoginInfo();
            // Use loginInfo.PlayerID to load & save progress - this demo does not save progress.
            Debug.Log("DemoUIController: ANDROID mygamez player id (LoginInfo.PlayerID) = " + (loginInfo != null ? loginInfo.PlayerID : "<null loginInfo>"));
#elif UNITY_IOS
            string mygamezPlayerId = MyGamezGameObject.GetCurrentMyGamezId();
            // Use mygamezPlayerId to load & save progress - this demo does not save progress.
            Debug.Log("DemoUIController: IOS mygamez player id = " + (string.IsNullOrEmpty(mygamezPlayerId) ? "<empty>" : mygamezPlayerId));
#endif

            // Load the Game scene where GameManager is located
            Debug.Log("MySDK initialization complete, loading Game scene...");
            SceneRouter.LoadGameScene();
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

        public void OnBuy50GoldButtonClicked()
        {
#if UNITY_ANDROID
            // Android payment implementation
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
#elif UNITY_IOS
            // iOS payment implementation
            float price = 1.0f; // Price in Chinese Yuan for iOS
            MyGamezGameObject.DoRequestInappPurchase(price, OnIOSPurchaseResult);
#endif
        }

        private void OnIOSPurchaseResult(MyGamezBridge.EventCode eventCode)
        {
            Debug.Log("iOS purchase result: " + eventCode);
            
            switch (eventCode)
            {
                case MyGamezBridge.EventCode.IapAllowed:
                    Debug.Log("iOS MySDK: IAP allowed, processing purchase");
                    // Here you would integrate with your actual iOS IAP system
                    // After successful purchase, call:
                    // MyGamezGameObject.DoAnnounceCompletedInappPurchase(price, OnIOSPurchaseAcknowledged);
                    break;
                case MyGamezBridge.EventCode.IapNotAllowedSinglePurchaseLimitExceeded:
                    ToastMessage.Show("Single purchase limit exceeded");
                    break;
                case MyGamezBridge.EventCode.IapNotAllowedMonthlyPurchaseLimitExceeded:
                    ToastMessage.Show("Monthly purchase limit exceeded");
                    break;
                case MyGamezBridge.EventCode.IapNotAllowedInGuestMode:
                    ToastMessage.Show("IAP not allowed in guest mode");
                    break;
                case MyGamezBridge.EventCode.IapNotAllowedAgeCriteriaNotMet:
                    ToastMessage.Show("IAP not allowed - age criteria not met");
                    break;
                default:
                    Debug.Log("iOS MySDK: Unknown purchase result: " + eventCode);
                    break;
            }
        }

        private void OnIOSPurchaseAcknowledged(MyGamezBridge.EventCode eventCode)
        {
            Debug.Log("iOS purchase acknowledged: " + eventCode);
            
            if (eventCode == MyGamezBridge.EventCode.CompletedIapAcknowledged)
            {
                Debug.Log("iOS MySDK: Purchase successfully acknowledged");
                // Add gold to player's account
                OnGoldUpdated(50);
            }
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
                singleButtonDialogWindow.setTitleText(data.Title);
                singleButtonDialogWindow.setMessageText(data.Body);
                singleButtonDialogWindow.setLeftText(data.Button);
                singleButtonDialogWindow.setLeftCallback(
                    delegate
                    {
                        singleButtonDialogWindow.hide();

                    });
                singleButtonDialogWindow.show();
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
#if UNITY_ANDROID
            int balance = MySDK.Api.AntiAddiction.GetIAPCreditLeft();
            if (balance == int.MaxValue)
                ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
            else
                ToastMessage.Show("Remaining balance is " + balance, ToastMessage.LENGTH_LONG);
#elif UNITY_IOS
            float balance = MyGamezGameObject.GetIapCreditLeft();
            if (balance == float.MaxValue)
                ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
            else
                ToastMessage.Show("Remaining balance is " + balance, ToastMessage.LENGTH_LONG);
#endif
        }

        public void OnGetRemainingPlaytimeButtonClicked()
        {
            Debug.Log("mysdk OnGetRemainingPlaytimeButtonClicked()");
#if UNITY_ANDROID
            long playtime = MySDK.Api.AntiAddiction.GetPlaytimeLeft();
            if (playtime == long.MaxValue)
                ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
            else
                ToastMessage.Show("Remaining playtime in ms is " + playtime, ToastMessage.LENGTH_LONG);
#elif UNITY_IOS
            int playtime = MyGamezGameObject.GetPlaytimeLeft();
            if (playtime == int.MaxValue)
                ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
            else
                ToastMessage.Show("Remaining playtime in ms is " + playtime, ToastMessage.LENGTH_LONG);
#endif
        }
    }
}