using UnityEngine;

public class PaymentManager : MonoBehaviour
{
    public static PaymentManager Instance { get; private set; }

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
        // Set up payment listener as early as possible
        SetupPaymentListener();
    }

    private void SetupPaymentListener()
    {
        Debug.Log("Setting up payment listener");
        try
        {
            MyGamez.MySDK.Api.Billing.SetPayCallback(new PaymentCallback());
            Debug.Log("Payment listener setup completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to setup payment listener: " + e.Message);
            Debug.Log("Game will continue without payment functionality");
        }
    }

    public void StartPayment(string itemId, int priceFen, string itemName, string itemDescription)
    {
        Debug.Log("Starting payment for item: " + itemId + ", price: " + priceFen + " fen");
        
        // Check if we need to show restrictions dialog when entering shop
        if (MySDKManager.Instance != null && !MySDKManager.Instance.IsPlayerAdult())
        {
            Debug.Log("Minor player entering shop, showing restrictions dialog");
            ShowStoreEnterDialog();
        }
        
        // Create IAP info (price in fens, name and description in Chinese)
        var iapInfo = new MyGamez.MySDK.Api.Billing.IAPInfo(priceFen, itemName, itemDescription);
        
        // Create payment info with custom ID for tracking
        string customId = "purchase_" + itemId + "_" + System.DateTime.Now.Ticks;
        var payInfo = new MyGamez.MySDK.Api.Billing.PayInfo(iapInfo, customId, "extra_info_" + itemId);
        
        // Check available payment methods
        var billers = MyGamez.MySDK.Api.Billing.GetAvailableBillers();
        var preferredBiller = MyGamez.MySDK.Api.Billing.GetPreferredBiller();
        
        Debug.Log("Available billers: " + billers.Count);
        Debug.Log("Preferred biller: " + preferredBiller.ToString());
        
        if (billers.Count == 1)
        {
            // Only one payment method
            Debug.Log("Using single available biller: " + billers[0]);
            MyGamez.MySDK.Api.Billing.DoBilling(billers[0], payInfo);
        }
        else if (billers.Contains(preferredBiller))
        {
            // Multiple payment methods, but preferred biller is available
            Debug.Log("Using preferred biller: " + preferredBiller);
            MyGamez.MySDK.Api.Billing.DoBilling(preferredBiller, payInfo);
        }
        else
        {
            // Multiple payment methods, no preferred biller, show selection dialog
            Debug.Log("Multiple billers available, showing selection dialog");
            ShowPaymentSelectionDialog(billers, payInfo);
        }
    }

    private void ShowPaymentSelectionDialog(System.Collections.Generic.List<MyGamez.MySDK.Api.Billing.Biller> billers, MyGamez.MySDK.Api.Billing.PayInfo payInfo)
    {
        // For demo, just use the first available biller
        // In a real implementation, you would show a proper selection dialog
        Debug.Log("Auto-selecting first biller: " + billers[0]);
        MyGamez.MySDK.Api.Billing.DoBilling(billers[0], payInfo);
    }

    private void ShowStoreEnterDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetStoreEnterPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Store Enter Dialog - Title: " + dialogData.Title);
            Debug.Log("Store Enter Dialog - Body: " + dialogData.Body);
            Debug.Log("Store Enter Dialog - Button: " + dialogData.Button);
            
            // In a real implementation, you would show a proper dialog with this data
            // The dialog should inform minor players about spending restrictions
            // When user clicks the button, dismiss dialog and allow shop access
        }
    }

    // Store prompt dialog for when player opens shop
    public void OnPlayerOpensShop()
    {
        Debug.Log("Player opening shop");
        
        // Check if player is adult
        if (MySDKManager.Instance != null && MySDKManager.Instance.IsPlayerAdult())
        {
            Debug.Log("Adult player, no restrictions, opening shop");
            OpenShop();
            return;
        }
        
        Debug.Log("Minor player, showing payment restrictions dialog");
        ShowStoreEnterDialog();
    }

    private void OpenShop()
    {
        Debug.Log("Opening shop");
        // In a real implementation, you would open the shop UI here
    }

    // Payment helper methods for common scenarios
    public void PurchaseCoins(int amount, int priceFen)
    {
        string itemName = "游戏币";
        string itemDescription = "购买 " + amount + " 游戏币";
        StartPayment("coins_" + amount, priceFen, itemName, itemDescription);
    }

    public void PurchaseRemoveAds(int priceFen)
    {
        string itemName = "移除广告";
        string itemDescription = "永久移除游戏内广告";
        StartPayment("remove_ads", priceFen, itemName, itemDescription);
    }

    public void PurchaseUnlimitedUndo(int priceFen)
    {
        string itemName = "无限撤销";
        string itemDescription = "获得无限撤销次数";
        StartPayment("unlimited_undo", priceFen, itemName, itemDescription);
    }

    public void PurchaseHint(int priceFen)
    {
        string itemName = "提示";
        string itemDescription = "获得游戏提示";
        StartPayment("hint", priceFen, itemName, itemDescription);
    }

    public void PurchaseLevelSkip(int priceFen)
    {
        string itemName = "跳过关卡";
        string itemDescription = "跳过当前关卡";
        StartPayment("level_skip", priceFen, itemName, itemDescription);
    }

    // Check available payment methods
    public void CheckAvailablePaymentMethods()
    {
        var billers = MyGamez.MySDK.Api.Billing.GetAvailableBillers();
        var preferredBiller = MyGamez.MySDK.Api.Billing.GetPreferredBiller();
        
        Debug.Log("Available payment methods:");
        foreach (var biller in billers)
        {
            Debug.Log("- " + biller.ToString());
        }
        
        Debug.Log("Preferred payment method: " + preferredBiller.ToString());
    }

    private void GiveItemsToPlayer(string customId)
    {
        Debug.Log("Giving items to player for purchase: " + customId);
        
        // Parse custom ID to get item information
        // In a real implementation, you would:
        // 1. Parse the custom ID to determine what items to give
        // 2. Add items to player's inventory
        // 3. Update UI to show new items
        // 4. Save player data
        
        // For demo, just add some coins
        if (GameManager.instance != null)
        {
            GameManager.instance.currentCoin += 100;
            GameManager.instance.SaveCoin();
            Debug.Log("Added 100 coins to player. Total coins: " + GameManager.instance.currentCoin);
        }
    }

    private void SendPaymentVerificationToServer(MyGamez.MySDK.Api.Security.Verification verification)
    {
        Debug.Log("Sending payment verification to server");
        
        // In a real implementation, you would:
        // 1. Send verification data to your game server
        // 2. Server validates using MyGamez public key
        // 3. Server confirms payment and returns item data
        // 4. Client receives confirmation and gives items
        
        // For demo, just log the verification data
        if (verification != null)
        {
            Debug.Log("Verification data available for server validation");
        }
    }

    private class PaymentCallback : MyGamez.MySDK.Api.Billing.IPayCallback
    {
        public void OnBillingResult(MyGamez.MySDK.Api.Billing.BillingResult result)
        {
            Debug.Log("Payment result: " + result.ResultCode + " - " + result.ResultMsg);
            Debug.Log("Biller: " + result.Biller);
            Debug.Log("Custom ID: " + (result.PayInfo != null ? result.PayInfo.CustomID : "null"));
            
            switch (result.ResultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                    Debug.Log("Payment successful");
                    HandlePaymentSuccess(result);
                    break;
                case MyGamez.MySDK.Api.ResultCode.CANCELLED:
                    Debug.Log("User canceled payment");
                    break;
                case MyGamez.MySDK.Api.ResultCode.FAILED:
                case MyGamez.MySDK.Api.ResultCode.UNKNOWN:
                    Debug.LogError("Payment failed: " + result.ResultMsg);
                    ShowPaymentError("Payment failed: " + result.ResultMsg);
                    break;
                default:
                    Debug.LogError("Unexpected payment result: " + result.ResultCode + " - " + result.ResultMsg);
                    break;
            }
        }

        private void HandlePaymentSuccess(MyGamez.MySDK.Api.Billing.BillingResult result)
        {
            // Get verification data for server validation
            var verification = result.Verification;
            if (verification != null)
            {
                Debug.Log("Payment verification data available for server validation");
                // Send verification data to your game server
                // Your server should validate this data using MyGamez public key
                SendPaymentVerificationToServer(verification);
            }
            
            // Give items to player
            if (result.PayInfo != null)
            {
                GiveItemsToPlayer(result.PayInfo.CustomID);
            }
            
            // Confirm items given when player has received items
            result.ConfirmGoodsGiven();
            Debug.Log("Items confirmed successfully");
        }

        private void SendPaymentVerificationToServer(MyGamez.MySDK.Api.Security.Verification verification)
        {
            Debug.Log("Sending payment verification to server");
            
            // In a real implementation, you would:
            // 1. Send verification data to your game server
            // 2. Server validates using MyGamez public key
            // 3. Server confirms payment and returns item data
            // 4. Client receives confirmation and gives items
            
            // For demo, just log the verification data
            Debug.Log("Verification data available for server validation");
        }

        private void GiveItemsToPlayer(string customId)
        {
            Debug.Log("Giving items to player for purchase: " + customId);
            
            // Parse custom ID to get item information
            // In a real implementation, you would:
            // 1. Parse the custom ID to determine what items to give
            // 2. Add items to player's inventory
            // 3. Update UI to show new items
            // 4. Save player data
            
            // For demo, just add some coins
            if (GameManager.instance != null)
            {
                GameManager.instance.currentCoin += 100;
                GameManager.instance.SaveCoin();
                Debug.Log("Added 100 coins to player. Total coins: " + GameManager.instance.currentCoin);
            }
        }

        private void ShowPaymentError(string errorMessage)
        {
            Debug.LogError("Payment Error: " + errorMessage);
            // In a real implementation, you would show a proper error dialog
        }
    }
}