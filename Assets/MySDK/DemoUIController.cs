using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyGamez.MySDK.Api;
using MyGamez.Demo.MySDKHelpers;
using System;


namespace MyGamez.Demo
{
    public class DemoUIController : MyGamezObserver
    {
        
        public DialogWindowController dialogWindow;
        public LoginSelectDialogController loginSelectDialogController;
        public SingleButtonDialogWindowController singleButtonDialogWindow;
        public DialogWindowController warningDialogWindow;
        public RIDCheckDialogController RIDCheckDialog;
        public NotificationBackground notificationBackground;
        public AgeAppropriateWindowController ageAppropriateWindowController; // Reference to window controller
        public GameObject appleSignInButton; // Apple Sign-In button (hidden by default)

        private bool playing = false;
        private MySDK.Api.Login.ILoginStateListener loginStateListener;
        
        // Window visibility monitoring
        private bool previousWindowVisible = false;
        private float visibilityCheckInterval = 0.5f; // Check every 0.5 seconds
        private float lastVisibilityCheckTime = 0f;
        private bool loginPending = false; // Track if login is waiting for window to be hidden

        private bool gcTried = false;

        private IOSLoginController iosLoginController;
        private AndroidLoginController androidLoginController;

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
            iosLoginController?.Update();
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

        private void Start()
        {

#if UNITY_IOS
            TryAuthenticateGameCenter();
            //TODO: Remove this after testing
            UserStatusSync.ClearUserStatus(this);
            iosLoginController = new IOSLoginController(this, MyGamez.Demo.ServerConfig.BaseUrl, InitializeIOSMySDKWithAppleAuth);
            Debug.Log("[DemoUIController][iOS] IOSLoginController created with baseUrl=" + MyGamez.Demo.ServerConfig.BaseUrl);
            
            // Initialize Apple Sign-In button visibility
            CheckSessionTokenAndUpdateButton();
#elif UNITY_ANDROID
            appleSignInButton.SetActive(false);
            androidLoginController = new AndroidLoginController(this, MyGamez.Demo.ServerConfig.BaseUrl);
            Debug.Log("[DemoUIController][Android] AndroidLoginController created with baseUrl=" + MyGamez.Demo.ServerConfig.BaseUrl);
#endif

            Debug.Log("[Startup] HasAcceptedPrivacyPolicy=" + HasAcceptedPrivacyPolicy());
            if (HasAcceptedPrivacyPolicy())
            {
                Debug.Log("[Startup] Privacy Policy accepted, initializing SDK...");
                InitializeMySDK();
            }
            else
            {
                Debug.Log("[Startup] Privacy Policy not accepted, showing dialog...");
                ShowPrivacyPolicyAndTosDialog();
            }
        }

        private void TryAuthenticateGameCenter()
        {
            if (gcTried) return;
            gcTried = true;

#if UNITY_IOS
            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate(success =>
                {
                    Debug.Log("Game Center Auth: " + success);
                });
            }
#endif
        }

        private bool HasAcceptedPrivacyPolicy()
        {
#if UNITY_IOS
                return PlayerPrefs.HasKey("IsPpAccepted") && PlayerPrefs.GetInt("IsPpAccepted") == 1;
#else
                return MySDK.Api.Features.PrivacyPolicy.IsPpAccepted();
#endif
        }

        private void InitializeMySDK()
        {
#if UNITY_IOS
                Debug.Log("[Startup] Initializing iOS SDK...");
                // Check sessionToken before proceeding
                string sessionToken = PlayerPrefs.GetString("session_token", string.Empty);
                if (string.IsNullOrEmpty(sessionToken))
                {
                    Debug.Log("[Startup] No session token found, showing Apple Sign-In button");
                    // Button visibility is already set in CheckSessionTokenAndUpdateButton()
                    // Don't proceed with login flow yet - wait for user to click the button
                    return;
                }
                else
                {
                    Debug.Log("[Startup] Session token found, proceeding with login flow");
                    Debug.Log("[Startup] Triggering iOS login flow via IOSLoginController.BeginLoginFlow()");
                    iosLoginController.BeginLoginFlow();
                }
#else
                Debug.Log("[Startup] Initializing Android/Editor SDK...");
                MySDK.Api.MySDKInit.Initialize(new MySDKHelpers.MySDKInitCallback(this));
#endif
        }

#if UNITY_IOS

        private void InitializeIOSMySDKWithAppleAuth()
        {
            Debug.Log("[iOS] Initializing MySDK with Apple authentication (post-session)");
			// iOS SDK configuration with Apple authentication via JWT
			StartCoroutine(RequestJwtAndInit());
        }

		private System.Collections.IEnumerator RequestJwtAndInit()
		{
			// Delegate JWT fetching to IOSLoginController, keep only initialization here
			bool done = false;
            string appName = "magicwatersort";
            string env = ServerConfig.MygamezEnv;
			string receivedToken = null;
			iosLoginController.RequestJwtToken(ServerConfig.MygamezEnv, appName, token => { receivedToken = token; done = true; });
			while (!done) yield return null;
			if (string.IsNullOrEmpty(receivedToken))
			{
				Debug.LogError("[iOS] Failed to obtain JWT token");
				yield break;
			}
			//string cpid = "mygamez";
            string cpid = "mygamez_pw"; // Replace with actual CPID
            string authParams = $"{{\"pw\":\"3b68f6085d578ef0a9a5af47531d3e7a\",\"app\":\"test-app\",\"player_id\":\"073b657a-96ef-5d11-a139-644cd85\"}}"; // Replace with actual authParams
			string backendUrl =  ServerConfig.MygamezEnv == "dev" ? "https://antiaddiction.dev.mygamez.cn/api/v1/usr" : "https://antiaddiction.myservicez.cn/api/v1/usr";
			//string authParams = "{\"jwt\":\"" + receivedToken + "\"}";
            Debug.Log("[iOS] Calling MyGamezGameObject.DoInitialize with JWT (token=" + receivedToken + ")");
			Debug.Log("[iOS] Calling MyGamezGameObject.DoInitialize with JWT (len=" + receivedToken.Length + ")");
			MyGamezGameObject.DoInitialize(cpid, backendUrl, authParams, OnIOSSDKInitialized);
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
                case MyGamezBridge.EventCode.DailyGameTimeDepleted:
                    Debug.Log("iOS MySDK: Daily game time depleted");
                    ShowTimeOutDialog();
                    break;
                case MyGamezBridge.EventCode.PlayingNotAllowedDueToTimeOfDayConstraints:
                    Debug.Log("iOS MySDK: Player not allowed to play due to time of day constraints");               ShowTimeOutDialog();
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
                    if (!MyGamezGameObject.IsAdult())
                    {
                        Debug.Log("Player is not adult, show underage play time limit warning dialog");
                        MyGamezGameObject.DoRequestPromptCallback(3, ShowUnderagePlayTimeLimitWarningDialogCallback);
                    }else{
                        Debug.Log("Player is adult, start game");
                        ShowProgressAndLogin();
                    }
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


        private void ShowUnderagePlayTimeLimitWarningDialogCallback(string title, string body, string button)
        {
            if (singleButtonDialogWindow != null)
            {
                Debug.Log("ShowUnderagePlayTimeLimitWarningDialogCallback, Title: " + title);
                Debug.Log("ShowUnderagePlayTimeLimitWarningDialogCallback, Body: " + body);
                Debug.Log("ShowUnderagePlayTimeLimitWarningDialogCallbackg, Button: " + button);
                singleButtonDialogWindow.setTitleText(title);
                singleButtonDialogWindow.setMessageText(body);
                singleButtonDialogWindow.setLeftText(button);
                singleButtonDialogWindow.setLeftCallback(
                    delegate
                    {
                        singleButtonDialogWindow.hide();
                        ShowProgressAndLogin();
                    });
                singleButtonDialogWindow.show();
            }
            else
            {
                Debug.LogError("DialogWindowController not assigned in GameManager!");
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
 #if UNITY_IOS
                        PlayerPrefs.SetInt("IsPpAccepted", 1); // 1 for accepted, 0 for not accepted
                        PlayerPrefs.Save();
                        // Update button visibility after privacy policy is accepted
                        CheckSessionTokenAndUpdateButton();
#else
                        MySDK.Api.Features.PrivacyPolicy.SetPpAccepted();
#endif
                        // Initialise MySDK
                        Debug.Log("Going to init MySDK");
                        InitializeMySDK();
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
                    Debug.Log($"[DemoUIController] MySDK Init {result.ResultCode}, triggering ShowProgressAndLogin()");
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
                Debug.Log($"[DemoUIController] Window visibility changed: {previousWindowVisible} -> {currentWindowVisible}, loginPending: {loginPending}");
                
                // If window became hidden and login is pending, proceed with login
                if (!currentWindowVisible && previousWindowVisible && loginPending)
                {
                    Debug.Log("[DemoUIController] Window became hidden and login is pending, proceeding with login");
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
            Debug.Log("[DemoUIController] ShowProgressAndLogin() called");
            
            if (notificationBackground != null)
            {
                Debug.Log("[DemoUIController] NotificationBackground found, showing progress bar");
                Debug.Log($"[DemoUIController] NotificationBackground.IsVisible() before Show(): {notificationBackground.IsVisible()}");
                
                // Show the notification background with progress bar
                notificationBackground.Show();
                
                Debug.Log($"[DemoUIController] NotificationBackground.IsVisible() after Show(): {notificationBackground.IsVisible()}");
                Debug.Log("[DemoUIController] Starting progress bar animation (8 seconds)");
                
                // Start the 8-second progress bar, then check window visibility before login
                notificationBackground.StartProgress(() => {
                    Debug.Log("[DemoUIController] Progress bar animation completed, checking window visibility before login");
                    CheckWindowVisibilityAndLogin();
                });
            }
            else
            {
                Debug.LogWarning("[DemoUIController] NotificationBackground is null, skipping progress bar and proceeding directly to login check");
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
            Debug.Log("[DemoUIController] CheckWindowVisibilityAndLogin() called");
            
            AgeAppropriateWindowController controller = GetAgeAppropriateWindowController();
            
            if (controller == null)
            {
                Debug.Log("[DemoUIController] No AgeAppropriateWindowController found, proceeding with login immediately");
                Login();
                return;
            }
            
            bool isWindowVisible = controller.IsWindowVisible();
            Debug.Log($"[DemoUIController] AgeAppropriateWindowController found, window visible: {isWindowVisible}");
            
            if (!isWindowVisible)
            {
                // Window is not visible, proceed with login immediately
                Debug.Log("[DemoUIController] Window is not visible, proceeding with login immediately");
                Login();
            }
            else
            {
                // Window is visible, wait for it to become hidden
                Debug.Log("[DemoUIController] Window is visible, setting loginPending=true and waiting for window to be hidden");
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
#if UNITY_IOS
            StartGame();
#else
            if (loginStateListener == null)
            {
                // Set LoginStateListener
                // Listener's OnLoginStateChanged method will be triggered when player's LoginState changes.
                loginStateListener = new MySDKHelpers.LoginListenerExample(this);
                MySDK.Api.Login.RegisterLoginStateListener(loginStateListener);
            }

            // Check available login methods
            List<MySDK.Api.Login.Vendor> vendors = MySDK.Api.Login.GetAvailableVendors();
            Debug.Log("Vendors available: " + string.Join(", ", vendors));
            // Check if there's a preferred login vendor
            MySDK.Api.Login.Vendor preferredVendor = MySDK.Api.Login.GetPreferredVendor();
            if (vendors.Count == 1) {
                // Only one login method, use it
                MySDK.Api.Login.DoLogin(vendors[0]);
            } else if (preferredVendor != null) {
                // Multiple login methods, but preferred vendor is available
                MySDK.Api.Login.DoLogin(preferredVendor);
            } else {
                // Multiple login methods, no preferred vendor, show selection dialog
                ShowLoginSelectionDialog(vendors);
            }
            
#endif
        }

        private void ShowLoginSelectionDialog(List<MySDK.Api.Login.Vendor> vendors)
        {

            loginSelectDialogController.setWechatLoginCallback(
                delegate
                {
                    MySDK.Api.Login.DoLogin(MySDK.Api.Login.Vendor.WECHAT3);
                });
            loginSelectDialogController.setPhoneLoginCallback(
            delegate
            {
                    MySDK.Api.Login.DoLogin(MySDK.Api.Login.Vendor.AURORA);
            });
            loginSelectDialogController.setWechatLoginButtonActive(true);
            loginSelectDialogController.setPhoneLoginButtonActive(true);
            
            loginSelectDialogController.show();
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
                    // User logged in successfully
                    MySDK.Api.Login.LoginInfo loginInfo = MySDK.Api.Login.GetLoginInfo();
                    string playerId = loginInfo.PlayerID;
                    // Get verification data for server validation
                    MySDK.Api.Security.Verification verification = loginInfo.Verification;
                    // Send verification data to your game server.
                    // Your server should validate this data using MyGamez public key
                    //SendVerificationToServer(playerId, verification);
                    // Continue to anti-addiction
                    Debug.Log("DemoUIController: User logged in successfully, initializing anti-addiction");
                    InitializeAntiaddiction();
                    break;
                case MySDK.Api.Login.LoginState.LOGIN_FAILED:
                    //ToastMessage.Show("Failed to login");
                    Debug.Log("Failed to login");
                    if (!playing) // Not in game yet, open login again to retry
                    {
                        Login();
                    }
                    break;
                case MySDK.Api.Login.LoginState.LOGIN_CANCELED:
                    //ToastMessage.Show("User canceled login");
                    Debug.Log("User canceled login");
                    break;
                case MySDK.Api.Login.LoginState.LOGGED_OUT:
                    //ToastMessage.Show("Successfully logged out");
                    Debug.Log("Successfully logged out");
                    // MySDK does not request restart but demo will restart to show login.
                    MySDK.Api.App.RestartApp();
                    break;
                case MySDK.Api.Login.LoginState.LOGOUT_RESTART:
                    //ToastMessage.Show("Logout restart");
                    Debug.Log("Logout restart");
                    MySDK.Api.App.RestartApp();
                    break;
                case MySDK.Api.Login.LoginState.LOGOUT_FAILED:
                    //ToastMessage.Show("Failed to logout");
                    Debug.Log("Failed to logout");
                    break;
                case MySDK.Api.Login.LoginState.LOGOUT_CANCELED:
                    //ToastMessage.Show("User canceled logout");
                    Debug.Log("User canceled logout");
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

            if (androidLoginController == null)
            {
                androidLoginController = new AndroidLoginController(this, MyGamez.Demo.ServerConfig.BaseUrl);
            }
            Debug.Log("Going to load user status for playerId: " + playerId);
            androidLoginController.LoadUserStatus(playerId, () =>
            {
                Debug.Log("User status loaded, going to initialize antiaddiction");
                UserStatusSync.PrintAllPlayerPrefs();
                MySDK.Api.AntiAddiction.Initialize(playerId, new MySDKHelpers.AntiAddictionCallback(this));
            });
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
                    //ToastMessage.Show("Unexpected error, trying again", 1);
                    Debug.Log("Unexpected error, trying again");
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

#if UNITY_IOS
            Debug.Log("IOS: Show Time Out Dialog");
            MyGamezGameObject.DoRequestPromptCallback(5, ShowPromptDialogCallback);
#else
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
#endif
        }

        private void ShowPromptDialogCallback(string title, string body, string button)
        {
            if (singleButtonDialogWindow != null)
            {
                Debug.Log("Show PromptDialog, Title: " + title);
                Debug.Log("Show PromptDialog, Body: " + body);
                Debug.Log("Show PromptDialog, Button: " + button);
                singleButtonDialogWindow.setTitleText(title);
                singleButtonDialogWindow.setMessageText(body);
                singleButtonDialogWindow.setLeftText(button);
                singleButtonDialogWindow.setLeftCallback(
                    delegate
                    {
                        singleButtonDialogWindow.hide();
                        Application.Quit();
                    });
                singleButtonDialogWindow.show();
            }
            else
            {
                Debug.LogError("DialogWindowController not assigned in GameManager!");
            }
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
                    Debug.Log("IOS: MyGamez player id is " + MyGamezGameObject.GetCurrentMyGamezId());
                    RequestIOSGameStart();
                    break;
                case MyGamezBridge.EventCode.RidCheckRequired:
                    Debug.Log("iOS MySDK: RID check failed, showing dialog again");
                    ShowRIDCheckDialog();
                    break;
                case MyGamezBridge.EventCode.DailyGameTimeDepleted:
                case MyGamezBridge.EventCode.PlayingNotAllowedDueToTimeOfDayConstraints:
                    Debug.Log("iOS MySDK: Playing not allowed due to time constraints");
                    Debug.Log("IOS: MyGamez player id is " + MyGamezGameObject.GetCurrentMyGamezId());
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
            //ToastMessage.Show("RID Check result is " + resultCode.ToString(), ToastMessage.LENGTH_LONG);
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
            PlayerPrefs.SetInt("gold", totalGold);
            PlayerPrefs.Save();
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
                //ToastMessage.Show("Player is adult. No need to show Store Prompt.", ToastMessage.LENGTH_LONG);
                Debug.Log("Player is adult. No need to show Store Prompt.");
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
            //ToastMessage.Show(MySDKHelpers.LoginInfoHelper.LoginInfoToString(loginInfo), ToastMessage.LENGTH_LONG);
            Debug.Log(MySDKHelpers.LoginInfoHelper.LoginInfoToString(loginInfo));
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
                //ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
                Debug.Log("Player is adult.");
            else
                //ToastMessage.Show("Remaining balance is " + balance, ToastMessage.LENGTH_LONG);
                Debug.Log("Remaining balance is " + balance);
#elif UNITY_IOS
            float balance = MyGamezGameObject.GetIapCreditLeft();
            if (balance == float.MaxValue)
                //ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
                Debug.Log("Player is adult.");
            else
                //ToastMessage.Show("Remaining balance is " + balance, ToastMessage.LENGTH_LONG);
                Debug.Log("Remaining balance is " + balance);
#endif
        }

        public void OnGetRemainingPlaytimeButtonClicked()
        {
            Debug.Log("mysdk OnGetRemainingPlaytimeButtonClicked()");
#if UNITY_ANDROID
            long playtime = MySDK.Api.AntiAddiction.GetPlaytimeLeft();
            if (playtime == long.MaxValue)
                //ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
                Debug.Log("Player is adult.");
            else
                //ToastMessage.Show("Remaining playtime in ms is " + playtime, ToastMessage.LENGTH_LONG);
                Debug.Log("Remaining playtime in ms is " + playtime);
#elif UNITY_IOS
            int playtime = MyGamezGameObject.GetPlaytimeLeft();
            if (playtime == int.MaxValue)
                //ToastMessage.Show("Player is adult.", ToastMessage.LENGTH_LONG);
                Debug.Log("Player is adult.");
            else
                //ToastMessage.Show("Remaining playtime in ms is " + playtime, ToastMessage.LENGTH_LONG);
                Debug.Log("Remaining playtime in ms is " + playtime);
#endif
        }

#if UNITY_IOS
        /// <summary>
        /// Check sessionToken and update Apple Sign-In button visibility
        /// </summary>
        private void CheckSessionTokenAndUpdateButton()
        {
            if (appleSignInButton == null)
            {
                Debug.LogWarning("[DemoUIController] Apple Sign-In button is not assigned");
                return;
            }

            string sessionToken = PlayerPrefs.GetString("session_token", string.Empty);
            if (string.IsNullOrEmpty(sessionToken))
            {
                Debug.Log("[DemoUIController] Session token is empty, showing Apple Sign-In button");
                appleSignInButton.SetActive(true);
            }
            else
            {
                Debug.Log("[DemoUIController] Session token exists, hiding Apple Sign-In button");
                appleSignInButton.SetActive(true);
            }
        }

        /// <summary>
        /// Called when Apple Sign-In button is clicked
        /// </summary>
        public void OnAppleSignInButtonClicked()
        {
            Debug.Log("[DemoUIController] Apple Sign-In button clicked");
            // Hide the button when clicked to prevent multiple clicks
            if (appleSignInButton != null)
            {
                appleSignInButton.SetActive(false);
            }
            
            if (iosLoginController != null)
            {
                iosLoginController.ShowAppleSignInDialog();
            }
            else
            {
                Debug.LogError("[DemoUIController] IOSLoginController is null, cannot show Apple Sign-In dialog");
            }
        }
#endif
    }
}