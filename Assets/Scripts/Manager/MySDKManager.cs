using UnityEngine;

public class MySDKManager : MonoBehaviour
{
    public static MySDKManager Instance { get; private set; }

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
        // Check privacy policy first, then initialize MySDK
        CheckPrivacyPolicyAndInit();
    }

    private void CheckPrivacyPolicyAndInit()
    {
        // Check if privacy policy is already accepted
        if (!MyGamez.MySDK.Api.Features.PrivacyPolicy.IsPpAccepted())
        {
            Debug.Log("Privacy Policy not accepted, showing dialog");
            ShowPrivacyPolicyDialog();
        }
        else
        {
            Debug.Log("Privacy Policy already accepted, initializing MySDK");
            InitializeMySDK();
        }
    }

    private void ShowPrivacyPolicyDialog()
    {
        try
        {
            // Get privacy policy data from MySDK
            var ppData = MyGamez.MySDK.Api.Features.PrivacyPolicy.GetPpData();
            var tosData = MyGamez.MySDK.Api.Features.PrivacyPolicy.GetTosData();
            
            if (ppData != null)
            {
                Debug.Log("Privacy Policy - Title: " + ppData.title);
                Debug.Log("Privacy Policy - Content: " + ppData.content);
                Debug.Log("Privacy Policy - Button: " + ppData.button);
            }
            
            if (tosData != null)
            {
                Debug.Log("Terms of Service - Title: " + tosData.title);
                Debug.Log("Terms of Service - Content: " + tosData.content);
                Debug.Log("Terms of Service - Button: " + tosData.button);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get privacy policy data: " + e.Message);
        }
        
        // For now, just accept privacy policy and continue
        // In a real implementation, you would show a proper dialog
        Debug.Log("Accepting privacy policy automatically for demo");
        MyGamez.MySDK.Api.Features.PrivacyPolicy.SetPpAccepted();
        InitializeMySDK();
    }

    private void InitializeMySDK()
    {
        Debug.Log("Initializing MySDK...");
        MyGamez.MySDK.Api.MySDKInit.Initialize(new MySDKInitCallback());
    }

    private void StartLogin()
    {
        Debug.Log("Starting login process...");
        
        // Register login state listener
        MyGamez.MySDK.Api.Login.RegisterLoginStateListener(new LoginStateListener());
        
        // Check available login methods
        var vendors = MyGamez.MySDK.Api.Login.GetAvailableVendors();
        var preferredVendor = MyGamez.MySDK.Api.Login.GetPreferredVendor();
        
        Debug.Log("Available vendors: " + vendors.Count);
        Debug.Log("Preferred vendor: " + preferredVendor.ToString());
        
        if (vendors.Count == 1)
        {
            // Only one login method, use it
            Debug.Log("Using single available vendor: " + vendors[0]);
            MyGamez.MySDK.Api.Login.DoLogin(vendors[0]);
        }
        else if (vendors.Contains(preferredVendor))
        {
            // Multiple login methods, but preferred vendor is available
            Debug.Log("Using preferred vendor: " + preferredVendor);
            MyGamez.MySDK.Api.Login.DoLogin(preferredVendor);
        }
        else
        {
            // Multiple login methods, no preferred vendor, show selection dialog
            Debug.Log("Multiple vendors available, showing selection dialog");
            ShowLoginSelectionDialog(vendors);
        }
    }

    private void ShowLoginSelectionDialog(System.Collections.Generic.List<MyGamez.MySDK.Api.Login.Vendor> vendors)
    {
        // For now, just use the first available vendor
        // In a real implementation, you would show a proper selection dialog
        Debug.Log("Auto-selecting first vendor: " + vendors[0]);
        MyGamez.MySDK.Api.Login.DoLogin(vendors[0]);
    }

    private void InitializeAntiAddiction(string playerId)
    {
        Debug.Log("Initializing anti-addiction for player: " + playerId);
        
        // Initialize anti-addiction system with player ID
        MyGamez.MySDK.Api.AntiAddiction.Initialize(playerId, new AntiAddictionCallback());
    }

    private void ShowRestrictionsDialogIfNeeded()
    {
        // Check if player is adult
        bool isAdult = MyGamez.MySDK.Api.AntiAddiction.IsAdult();
        Debug.Log("Is player adult: " + isAdult);
        
        if (isAdult)
        {
            Debug.Log("Adult player, no need to show restrictions dialog");
            // Adult player, continue directly to game
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
            return;
        }
        
        Debug.Log("Minor player, showing restrictions dialog");
        ShowRestrictionsDialog();
    }

    private void ShowRestrictionsDialog()
    {
        // Get dialog data from MySDK for minor players
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetPlayerIdentificationCompletedPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Restrictions Dialog - Title: " + dialogData.Title);
            Debug.Log("Restrictions Dialog - Body: " + dialogData.Body);
            Debug.Log("Restrictions Dialog - Button: " + dialogData.Button);
            
            // In a real implementation, you would show a proper dialog with this data
            // The dialog should be non-cancelable and show restrictions for minor players
            // When user clicks the button, dismiss dialog and continue to game
            
            Debug.Log("User acknowledged restrictions, continuing to game");
        }
        else
        {
            Debug.Log("No dialog data available, using fallback restrictions message");
        }
        
        // Continue to game after showing restrictions
        if (GameManager.instance != null)
        {
            GameManager.instance.InitGame();
        }
    }

    // Public methods for other managers
    public bool IsPlayerAdult()
    {
        bool isAdult = MyGamez.MySDK.Api.AntiAddiction.IsAdult();
        Debug.Log("Is player adult: " + isAdult);
        return isAdult;
    }

    public void CheckPlaytimeLeft()
    {
        long playtimeLeft = MyGamez.MySDK.Api.AntiAddiction.GetPlaytimeLeft();
        Debug.Log("Playtime left: " + playtimeLeft + " seconds");
    }

    public void CheckIAPCreditLeft()
    {
        int creditLeft = MyGamez.MySDK.Api.AntiAddiction.GetIAPCreditLeft();
        Debug.Log("IAP credit left: " + creditLeft);
    }

    public void VerifyIdentity(string name, string idNumber)
    {
        Debug.Log("Verifying identity: " + name + ", " + idNumber);
        
        // Check if ID format is valid
        bool isValidFormat = MyGamez.MySDK.Api.AntiAddiction.CheckIdFormat(idNumber);
        if (!isValidFormat)
        {
            Debug.LogError("Invalid ID format");
            return;
        }
        
        MyGamez.MySDK.Api.AntiAddiction.VerifyIdentity(name, idNumber, new IdentityVerificationCallback());
    }

    public bool CheckIdFormat(string idNumber)
    {
        bool isValid = MyGamez.MySDK.Api.AntiAddiction.CheckIdFormat(idNumber);
        Debug.Log("ID format check for " + idNumber + ": " + (isValid ? "Valid" : "Invalid"));
        return isValid;
    }

    // Callback classes
    private class MySDKInitCallback : MyGamez.MySDK.Api.MySDKInit.IMySDKInitCallback
    {
        public void OnResult(MyGamez.MySDK.Api.MySDKInit.MySDKInitResult result)
        {
            switch (result.ResultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                case MyGamez.MySDK.Api.ResultCode.ALREADY_DONE:
                    Debug.Log("MySDK initialized successfully");
                    // Start login process
                    if (MySDKManager.Instance != null)
                    {
                        MySDKManager.Instance.StartLogin();
                    }
                    break;
                case MyGamez.MySDK.Api.ResultCode.PP_AND_TOS_NOT_ACCEPTED:
                    Debug.Log("Privacy Policy not accepted, showing dialog");
                    if (MySDKManager.Instance != null)
                    {
                        MySDKManager.Instance.ShowPrivacyPolicyDialog();
                    }
                    break;
                default:
                    Debug.LogError("MySDK initialization failed: " + result.ResultCode + " - " + result.ResultMsg);
                    // Continue with game anyway
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.InitGame();
                    }
                    break;
            }
        }
    }

    private class LoginStateListener : MyGamez.MySDK.Api.Login.ILoginStateListener
    {
        public void OnLoginStateChanged(MyGamez.MySDK.Api.Login.LoginState loginState)
        {
            Debug.Log("Login state changed: " + loginState);
            
            switch (loginState)
            {
                case MyGamez.MySDK.Api.Login.LoginState.LOGGED_IN:
                    Debug.Log("User logged in successfully");
                    HandleLoginSuccess();
                    break;
                case MyGamez.MySDK.Api.Login.LoginState.LOGIN_FAILED:
                    Debug.Log("Login failed, try again");
                    break;
                case MyGamez.MySDK.Api.Login.LoginState.LOGIN_CANCELED:
                    Debug.Log("User canceled login");
                    break;
                case MyGamez.MySDK.Api.Login.LoginState.LOGGED_OUT:
                    Debug.Log("User logged out");
                    break;
                case MyGamez.MySDK.Api.Login.LoginState.LOGOUT_RESTART:
                    Debug.Log("User logged out and app must restart");
                    MyGamez.MySDK.Api.App.QuitApp();
                    break;
                case MyGamez.MySDK.Api.Login.LoginState.LOGOUT_FAILED:
                    Debug.Log("Logout failed");
                    break;
                case MyGamez.MySDK.Api.Login.LoginState.LOGOUT_CANCELED:
                    Debug.Log("User canceled logout");
                    break;
            }
        }

        private void HandleLoginSuccess()
        {
            // Get login info
            var loginInfo = MyGamez.MySDK.Api.Login.GetLoginInfo();
            if (loginInfo != null)
            {
                string playerId = loginInfo.PlayerID;
                Debug.Log("Player ID: " + playerId);
                
                // Get verification data for server validation
                var verification = loginInfo.Verification;
                if (verification != null)
                {
                    Debug.Log("Verification data available for server validation");
                }
                
                // Continue to anti-addiction
                if (MySDKManager.Instance != null)
                {
                    MySDKManager.Instance.InitializeAntiAddiction(playerId);
                }
            }
        }
    }

    private class AntiAddictionCallback : MyGamez.MySDK.Api.AntiAddiction.IAntiAddictionCallback
    {
        public void OnAntiAddictionResult(MyGamez.MySDK.Api.ResultCode resultCode, string msg)
        {
            Debug.Log("Anti-Addiction result: " + resultCode + " - " + msg);
            
            switch (resultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                    Debug.Log("User can play, requesting game start");
                    RequestGameStart();
                    break;
                case MyGamez.MySDK.Api.ResultCode.NEED_TO_SHOW_DIALOG:
                    Debug.Log("User needs to verify identity");
                    ShowIdentityVerificationDialog();
                    break;
                case MyGamez.MySDK.Api.ResultCode.TIME_OVER:
                case MyGamez.MySDK.Api.ResultCode.DISALLOWED:
                case MyGamez.MySDK.Api.ResultCode.LIMITED:
                    Debug.Log("User cannot play now - time over/disallowed/limited");
                    ShowPlaytimeOverDialog();
                    break;
                case MyGamez.MySDK.Api.ResultCode.SDK_BUSY:
                    Debug.Log("Previous request still processing, waiting...");
                    break;
                case MyGamez.MySDK.Api.ResultCode.CONFIG_ERROR:
                    Debug.LogError("Configuration error, check mysdk_conf.ini");
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.InitGame();
                    }
                    break;
                default:
                    Debug.LogError("Anti-addiction error: " + resultCode + " - " + msg);
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.InitGame();
                    }
                    break;
            }
        }

        private void RequestGameStart()
        {
            Debug.Log("Requesting game start permission");
            MyGamez.MySDK.Api.AntiAddiction.RequestGameStart(new GameStartCallback());
        }

        private void ShowIdentityVerificationDialog()
        {
            Debug.Log("Showing identity verification dialog");
            
            // For demo purposes, we'll use test data
            string testName = "Test User";
            string testIdNumber = "123456789012345678"; // 18-digit test ID
            
            Debug.Log("Attempting RID check with test data: " + testName + ", " + testIdNumber);
            MyGamez.MySDK.Api.AntiAddiction.AttemptRidCheck(testName, testIdNumber, new RIDCheckCallback());
        }

        private void ShowPlaytimeOverDialog()
        {
            Debug.Log("Showing playtime over dialog");
            
            // Get dialog data from MySDK
            var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetDailyGameTimeDepletionPromptDialogData();
            if (dialogData != null)
            {
                Debug.Log("Playtime Over Dialog - Title: " + dialogData.Title);
                Debug.Log("Playtime Over Dialog - Body: " + dialogData.Body);
                Debug.Log("Playtime Over Dialog - Button: " + dialogData.Button);
            }
            
            // For demo, continue anyway
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
        }
    }

    private class GameStartCallback : MyGamez.MySDK.Api.AntiAddiction.IAntiAddictionCallback
    {
        public void OnAntiAddictionResult(MyGamez.MySDK.Api.ResultCode resultCode, string msg)
        {
            Debug.Log("Game start request result: " + resultCode + " - " + msg);
            
            switch (resultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                    Debug.Log("Game start approved, initializing game");
                    StartGame();
                    break;
                case MyGamez.MySDK.Api.ResultCode.NEED_TO_SHOW_DIALOG:
                    Debug.Log("Need identity verification");
                    ShowIdentityVerificationDialog();
                    break;
                case MyGamez.MySDK.Api.ResultCode.TIME_OVER:
                case MyGamez.MySDK.Api.ResultCode.DISALLOWED:
                    Debug.Log("Cannot play now - time over/disallowed");
                    ShowCannotPlayDialog();
                    break;
                case MyGamez.MySDK.Api.ResultCode.SDK_BUSY:
                    Debug.Log("Previous request still processing, waiting...");
                    break;
                case MyGamez.MySDK.Api.ResultCode.CONFIG_ERROR:
                    Debug.LogError("Configuration error, check mysdk_conf.ini");
                    ShowErrorDialog("Configuration error, please contact support");
                    break;
                default:
                    Debug.LogError("Game start request failed: " + resultCode + " - " + msg);
                    ShowErrorDialog("Game start failed, please try again");
                    break;
            }
        }

        private void StartGame()
        {
            Debug.Log("Starting the game");
            
            // Check if we need to show restrictions dialog for minor players
            if (MySDKManager.Instance != null)
            {
                MySDKManager.Instance.ShowRestrictionsDialogIfNeeded();
            }

            // Start the actual game
            if (GameManager.instance != null)
            {
                //TODO
                //GameManager.instance.StartGame();
            }
        }

        private void ShowIdentityVerificationDialog()
        {
            Debug.Log("Showing identity verification dialog from game start request");
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
        }

        private void ShowCannotPlayDialog()
        {
            Debug.Log("Showing cannot play dialog");
            
            var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetDailyGameTimeDepletionPromptDialogData();
            if (dialogData != null)
            {
                Debug.Log("Cannot Play Dialog - Title: " + dialogData.Title);
                Debug.Log("Cannot Play Dialog - Body: " + dialogData.Body);
                Debug.Log("Cannot Play Dialog - Button: " + dialogData.Button);
            }
            
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
        }

        private void ShowErrorDialog(string errorMessage)
        {
            Debug.LogError("Game Start Error: " + errorMessage);
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
        }
    }

    private class RIDCheckCallback : MyGamez.MySDK.Api.AntiAddiction.IRidCheckCallback
    {
        public void OnResult(string name, string rid, MyGamez.MySDK.Api.ResultCode resultCode, string message)
        {
            Debug.Log("RID Check result: " + resultCode + " - " + message);
            
            switch (resultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                    Debug.Log("Identity verified successfully, continuing to game start");
                    RequestGameStart();
                    break;
                case MyGamez.MySDK.Api.ResultCode.INVALID:
                    Debug.LogError("Invalid name or ID number");
                    ShowError("Invalid name or ID number");
                    break;
                case MyGamez.MySDK.Api.ResultCode.EMPTY:
                    Debug.LogError("Name or ID is empty");
                    ShowError("Name and ID number cannot be empty");
                    break;
                case MyGamez.MySDK.Api.ResultCode.DISALLOWED:
                case MyGamez.MySDK.Api.ResultCode.TIME_OVER:
                    Debug.Log("User cannot play - disallowed or time over");
                    ShowPlaytimeOverDialog();
                    break;
                case MyGamez.MySDK.Api.ResultCode.CONFIG_ERROR:
                    Debug.LogError("Configuration error, check mysdk_conf.ini");
                    ShowError("Configuration error, please contact support");
                    break;
                default:
                    Debug.LogError("RID check error: " + resultCode + " - " + message);
                    ShowError("Identity verification failed, please try again");
                    break;
            }
        }

        private void RequestGameStart()
        {
            Debug.Log("Requesting game start after successful RID check");
            MyGamez.MySDK.Api.AntiAddiction.RequestGameStart(new GameStartCallback());
        }

        private void ShowError(string errorMessage)
        {
            Debug.LogError("RID Check Error: " + errorMessage);
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
        }

        private void ShowPlaytimeOverDialog()
        {
            Debug.Log("Showing playtime over dialog after RID check");
            if (GameManager.instance != null)
            {
                GameManager.instance.InitGame();
            }
        }
    }

    private class IdentityVerificationCallback : MyGamez.MySDK.Api.AntiAddiction.IAntiAddictionCallback
    {
        public void OnAntiAddictionResult(MyGamez.MySDK.Api.ResultCode resultCode, string msg)
        {
            Debug.Log("Identity Verification result: " + resultCode + " - " + msg);
            
            switch (resultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                    Debug.Log("Identity verification successful");
                    break;
                case MyGamez.MySDK.Api.ResultCode.INVALID:
                    Debug.LogError("Invalid identity information");
                    break;
                case MyGamez.MySDK.Api.ResultCode.EMPTY:
                    Debug.LogError("Identity information is empty");
                    break;
                case MyGamez.MySDK.Api.ResultCode.DISALLOWED:
                case MyGamez.MySDK.Api.ResultCode.TIME_OVER:
                    Debug.Log("Identity verification disallowed or time over");
                    break;
                case MyGamez.MySDK.Api.ResultCode.CONFIG_ERROR:
                    Debug.LogError("Configuration error during identity verification");
                    break;
                default:
                    Debug.LogError("Identity verification error: " + resultCode + " - " + msg);
                    break;
            }
        }
    }
}