using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Networking;
using System;
using System.Text;
using MiniJSON;
using MyGamez.Demo;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    private static IAPManager instance;
    public static IAPManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("IAPManager");
                instance = go.AddComponent<IAPManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    private bool isInitialized = false;
    private bool initializationInProgress = false;
    private InitializationFailureReason? lastInitFailure = null;
    // Mapping from package ID to (product ID, product type)
    // IMPORTANT: Replace these with your actual product IDs from App Store Connect
    // Product IDs must match exactly what you configured in App Store Connect
    // Product types: NonConsumable for permanent items (like Remove Ads), Consumable for one-time purchases
    private Dictionary<Config.IAPPackageID, (string id, ProductType type)> productMap = new Dictionary<Config.IAPPackageID, (string, ProductType)>
    {
        { Config.IAPPackageID.NoAds, ("com.mygamez.magicwatersort.removeads", ProductType.NonConsumable) },
        { Config.IAPPackageID.GoldPack1, ("com.mygamez.magicwatersort.goldpack1", ProductType.Consumable) },
        { Config.IAPPackageID.GoldPack2, ("com.mygamez.magicwatersort.goldpack2", ProductType.Consumable) },
        { Config.IAPPackageID.GoldPack3, ("com.mygamez.magicwatersort.goldpack3", ProductType.Consumable) },
        { Config.IAPPackageID.GoldPack4, ("com.mygamez.magicwatersort.goldpack4", ProductType.Consumable) },
        { Config.IAPPackageID.GoldPack5, ("com.mygamez.magicwatersort.goldpack5", ProductType.Consumable) },
        { Config.IAPPackageID.GoldPack6, ("com.mygamez.magicwatersort.goldpack6", ProductType.Consumable) },
    };

    // Callback for purchase completion
    private Action<Config.IAPPackageID, bool, string> onPurchaseComplete;
    
    // Server URL for receipt verification
    private string serverBaseUrl = "https://weixin.mygamez.cn";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[IAPManager] Awake");
            Debug.Log("[IAPManager] Initializing purchasing...");
            InitializePurchasing();
            Debug.Log("[IAPManager] Purchasing initialized");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializePurchasing()
    {
        if (isInitialized)
        {
            Debug.Log("[IAPManager] Already initialized");
            return;
        }
        
        if (initializationInProgress)
        {
            Debug.Log("[IAPManager] Initialization already in progress");
            return;
        }

        initializationInProgress = true;
        lastInitFailure = null;
        Debug.Log("[IAPManager] Starting store initialization...");

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add all products to the builder with their correct types
        foreach (var kvp in productMap)
        {
            var (productId, productType) = kvp.Value;
            builder.AddProduct(productId, productType);
            Debug.Log($"[IAPManager] Added product: {productId} (type: {productType}) for package {kvp.Key}");
        }

        UnityPurchasing.Initialize(this, builder);
        Debug.Log("[IAPManager] Store initialization completed");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAPManager] Initialization successful");
        storeController = controller;
        extensionProvider = extensions;
        isInitialized = true;
        initializationInProgress = false;
        lastInitFailure = null;
        
        // Log product availability
        foreach (var kvp in productMap)
        {
            var (productId, _) = kvp.Value;
            Product product = controller.products.WithID(productId);
            if (product != null)
            {
                Debug.Log($"[IAPManager] Product {productId} available: {product.availableToPurchase}, price: {product.metadata.localizedPriceString}");
            }
        }
        
        // Notify any waiting purchase attempts
        if (pendingPurchasePackageID.HasValue && pendingPurchaseCallback != null)
        {
            var packageID = pendingPurchasePackageID.Value;
            var callback = pendingPurchaseCallback;
            pendingPurchasePackageID = null;
            pendingPurchaseCallback = null;
            Debug.Log($"[IAPManager] Retrying purchase for {packageID} after initialization");
            PurchaseProductInternal(packageID, callback);
        }
    }
    
    // Pending purchase info for retry after initialization
    private Config.IAPPackageID? pendingPurchasePackageID = null;
    private Action<Config.IAPPackageID, bool, string> pendingPurchaseCallback = null;
    
    private IEnumerator WaitForInitializationAndPurchase(Config.IAPPackageID packageID, Action<Config.IAPPackageID, bool, string> onComplete)
    {
        pendingPurchasePackageID = packageID;
        pendingPurchaseCallback = onComplete;
        
        float timeout = 10f; // 10 seconds timeout
        float elapsed = 0f;
        
        while (!isInitialized && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (!isInitialized)
        {
            Debug.LogError("[IAPManager] Initialization timeout, purchase failed");
            pendingPurchasePackageID = null;
            pendingPurchaseCallback = null;
            onComplete?.Invoke(packageID, false, "Store initialization timeout");
        }
        // If initialization completed, OnInitialized will handle the retry
    }
    
    private void PurchaseProductInternal(Config.IAPPackageID packageID, Action<Config.IAPPackageID, bool, string> onComplete)
    {
        if (!productMap.ContainsKey(packageID))
        {
            Debug.LogError($"[IAPManager] Product ID not found for package: {packageID}");
            onComplete?.Invoke(packageID, false, $"Product ID not found for package: {packageID}");
            return;
        }

        var (productId, productType) = productMap[packageID];
        Product product = storeController.products.WithID(productId);

        if (product == null || !product.availableToPurchase)
        {
            Debug.LogError($"[IAPManager] Product not available: {productId}");
            onComplete?.Invoke(packageID, false, $"Product not available: {productId}");
            return;
        }

        Debug.Log($"[IAPManager] Purchasing product: {productId} for package: {packageID}");
        onPurchaseComplete = onComplete;
        storeController.InitiatePurchase(product);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAPManager] Initialization failed: {error}");
        isInitialized = false;
        initializationInProgress = false;
        lastInitFailure = error;
        
        // Notify any waiting purchase attempts
        if (pendingPurchasePackageID.HasValue && pendingPurchaseCallback != null)
        {
            var packageID = pendingPurchasePackageID.Value;
            var callback = pendingPurchaseCallback;
            pendingPurchasePackageID = null;
            pendingPurchaseCallback = null;
            callback?.Invoke(packageID, false, $"Store initialization failed: {error}");
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAPManager] Initialization failed: {error}, {message}");
        isInitialized = false;
        initializationInProgress = false;
        lastInitFailure = error;
        
        // Notify any waiting purchase attempts
        if (pendingPurchasePackageID.HasValue && pendingPurchaseCallback != null)
        {
            var packageID = pendingPurchasePackageID.Value;
            var callback = pendingPurchaseCallback;
            pendingPurchasePackageID = null;
            pendingPurchaseCallback = null;
            callback?.Invoke(packageID, false, $"Store initialization failed: {error} - {message}");
        }
    }

    /// <summary>
    /// Initiate a purchase for the given package ID
    /// If not initialized yet, will wait and retry
    /// </summary>
    /// <param name="packageID">The package to purchase</param>
    /// <param name="onComplete">Callback: (packageID, success, errorMessage)</param>
    public void PurchaseProduct(Config.IAPPackageID packageID, Action<Config.IAPPackageID, bool, string> onComplete)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[IAPManager] Store not initialized yet, waiting for initialization...");
            // Wait for initialization and retry
            StartCoroutine(WaitForInitializationAndPurchase(packageID, onComplete));
            return;
        }

        PurchaseProductInternal(packageID, onComplete);
    }
    
    /// <summary>
    /// Ensure IAPManager is initialized early
    /// Call this from app startup to pre-initialize
    /// </summary>
    public static void InitializeEarly()
    {
        // Accessing Instance will trigger creation and initialization
        var _ = Instance;
        Debug.Log("[IAPManager] Early initialization triggered");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"[IAPManager] Purchase successful: {args.purchasedProduct.definition.id}");

        // Find the package ID from the product ID
        Config.IAPPackageID? packageID = null;
        foreach (var kvp in productMap)
        {
            if (kvp.Value.id == args.purchasedProduct.definition.id)
            {
                packageID = kvp.Key;
                break;
            }
        }

        if (!packageID.HasValue)
        {
            Debug.LogError($"[IAPManager] Could not find package ID for product: {args.purchasedProduct.definition.id}");
            onPurchaseComplete?.Invoke(Config.IAPPackageID.GoldPack1, false, "Unknown product");
            return PurchaseProcessingResult.Complete;
        }

        // Extract receipt data for verification
        string receiptData = ExtractReceiptData(args.purchasedProduct);
        if (string.IsNullOrEmpty(receiptData))
        {
            Debug.LogError("[IAPManager] Failed to extract receipt data");
            onPurchaseComplete?.Invoke(packageID.Value, false, "Failed to extract receipt");
            return PurchaseProcessingResult.Complete;
        }

        // Verify receipt with backend server
        StartCoroutine(VerifyReceiptWithServer(packageID.Value, receiptData, args.purchasedProduct.definition.id));

        // Return Pending - we'll complete after server verification
        return PurchaseProcessingResult.Pending;
    }

    /// <summary>
    /// Extract receipt data from the purchased product
    /// </summary>
    private string ExtractReceiptData(Product product)
    {
        try
        {
            // For iOS, the receipt is in product.receipt
            // Unity IAP provides the receipt as a JSON string
            if (!string.IsNullOrEmpty(product.receipt))
            {
                Debug.Log($"[IAPManager] Extracted receipt data, length: {product.receipt.Length}");
                return product.receipt;
            }

            Debug.LogWarning("[IAPManager] No receipt data found in product");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAPManager] Error extracting receipt: {e}");
            return null;
        }
    }

    /// <summary>
    /// Verify receipt with backend server
    /// </summary>
    private IEnumerator VerifyReceiptWithServer(Config.IAPPackageID packageID, string receiptData, string productId)
    {
        string url = serverBaseUrl + "/api/apple/verify_receipt";
        
        // Get session_token from PlayerPrefs if available
        string sessionToken = PlayerPrefs.GetString("session_token", "");
        
        // Prepare payload
        var payload = new Dictionary<string, object>
        {
            {"receipt_data", receiptData},
            {"product_id", productId},
            {"package_id", packageID.ToString()},
            {"env", ServerConfig.iOSEnv}
        };
        
        // Add session_token if available (server will resolve player_id from it)
        if (!string.IsNullOrEmpty(sessionToken))
        {
            payload["session_token"] = sessionToken;
            Debug.Log($"[IAPManager] Including session_token in receipt verification request");
        }
        else
        {
            Debug.LogWarning("[IAPManager] No session_token found in PlayerPrefs, purchase record may not be saved with player_id");
        }

        string json = Json.Serialize(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        Debug.Log($"[IAPManager] Verifying receipt with server: {url}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[IAPManager] Receipt verification request failed: {request.error}");
                onPurchaseComplete?.Invoke(packageID, false, $"Verification request failed: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[IAPManager] Receipt verification response: {responseText}");

            try
            {
                var response = Json.Deserialize(responseText) as Dictionary<string, object>;
                if (response != null && response.ContainsKey("code"))
                {
                    int code = Convert.ToInt32(response["code"]);
                    if (code == 0)
                    {
                        // Verification successful
                        Debug.Log("[IAPManager] Receipt verified successfully by server");
                        onPurchaseComplete?.Invoke(packageID, true, null);
                        
                        // Complete the purchase
                        if (storeController != null)
                        {
                            Product product = storeController.products.WithID(productId);
                            if (product != null)
                            {
                                storeController.ConfirmPendingPurchase(product);
                            }
                        }
                    }
                    else
                    {
                        string errorMsg = response.ContainsKey("message") ? response["message"].ToString() : "Verification failed";
                        Debug.LogError($"[IAPManager] Receipt verification failed: {errorMsg}");
                        onPurchaseComplete?.Invoke(packageID, false, errorMsg);
                    }
                }
                else
                {
                    Debug.LogError("[IAPManager] Invalid response format from server");
                    onPurchaseComplete?.Invoke(packageID, false, "Invalid server response");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAPManager] Error parsing verification response: {e}");
                onPurchaseComplete?.Invoke(packageID, false, $"Parse error: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Set the server base URL for receipt verification
    /// </summary>
    public void SetServerBaseUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            serverBaseUrl = url.TrimEnd('/');
            Debug.Log($"[IAPManager] Server base URL set to: {serverBaseUrl}");
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"[IAPManager] Purchase failed: {product.definition.id}, Reason: {failureReason}");

        // Find the package ID
        Config.IAPPackageID? packageID = null;
        foreach (var kvp in productMap)
        {
            if (kvp.Value.id == product.definition.id)
            {
                packageID = kvp.Key;
                break;
            }
        }

        if (packageID.HasValue)
        {
            string errorMessage = failureReason.ToString();
            onPurchaseComplete?.Invoke(packageID.Value, false, errorMessage);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"[IAPManager] Purchase failed: {product.definition.id}, Reason: {failureDescription.reason}, Message: {failureDescription.message}");

        // Find the package ID
        Config.IAPPackageID? packageID = null;
        foreach (var kvp in productMap)
        {
            if (kvp.Value.id == product.definition.id)
            {
                packageID = kvp.Key;
                break;
            }
        }

        if (packageID.HasValue)
        {
            onPurchaseComplete?.Invoke(packageID.Value, false, failureDescription.message);
        }
    }

    /// <summary>
    /// Get the Apple product ID for a package
    /// </summary>
    public string GetProductId(Config.IAPPackageID packageID)
    {
        return productMap.ContainsKey(packageID) ? productMap[packageID].id : null;
    }
    
    /// <summary>
    /// Get the product type for a package
    /// </summary>
    public ProductType? GetProductType(Config.IAPPackageID packageID)
    {
        return productMap.ContainsKey(packageID) ? productMap[packageID].type : (ProductType?)null;
    }
    
    /// <summary>
    /// Check if a non-consumable product has been purchased
    /// </summary>
    public bool IsProductOwned(Config.IAPPackageID packageID)
    {
        if (!isInitialized || storeController == null)
            return false;
            
        if (!productMap.ContainsKey(packageID))
            return false;
            
        var (productId, productType) = productMap[packageID];
        if (productType != ProductType.NonConsumable)
            return false; // Only non-consumables can be "owned"
            
        Product product = storeController.products.WithID(productId);
        return product != null && product.hasReceipt;
    }
    
    /// <summary>
    /// Get localized price string for a product
    /// </summary>
    public string GetLocalizedPrice(Config.IAPPackageID packageID)
    {
        if (!isInitialized || storeController == null)
            return null;
            
        if (!productMap.ContainsKey(packageID))
            return null;
            
        var (productId, _) = productMap[packageID];
        Product product = storeController.products.WithID(productId);
        return product?.metadata?.localizedPriceString;
    }
    
    /// <summary>
    /// Get initialization status
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    /// <summary>
    /// Get last initialization failure reason (if any)
    /// </summary>
    public InitializationFailureReason? GetLastInitFailure()
    {
        return lastInitFailure;
    }
    
    /// <summary>
    /// Retry initialization if it previously failed
    /// </summary>
    public void RetryInitialization()
    {
        if (!isInitialized && !initializationInProgress)
        {
            Debug.Log("[IAPManager] Retrying initialization...");
            InitializePurchasing();
        }
    }

    /// <summary>
    /// Check if the store is initialized and ready
    /// </summary>
    public bool IsReady()
    {
        return isInitialized && storeController != null;
    }
}

