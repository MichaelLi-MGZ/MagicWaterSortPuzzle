using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Networking;
using System;
using System.Text;
using MiniJSON;

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

    // Mapping from package ID to Apple product ID
    // IMPORTANT: Replace these with your actual product IDs from App Store Connect
    // Product IDs must match exactly what you configured in App Store Connect
    private Dictionary<Config.IAPPackageID, string> productIdMap = new Dictionary<Config.IAPPackageID, string>
    {
        { Config.IAPPackageID.NoAds, "com.mygamez.magicwatersort.removeads" },
        { Config.IAPPackageID.GoldPack1, "com.mygamez.magicwatersort.goldpack1" },
        { Config.IAPPackageID.GoldPack2, "com.mygamez.magicwatersort.goldpack2" },
        { Config.IAPPackageID.GoldPack3, "com.mygamez.magicwatersort.goldpack3" },
        { Config.IAPPackageID.GoldPack4, "com.mygamez.magicwatersort.goldpack4" },
        { Config.IAPPackageID.GoldPack5, "com.mygamez.magicwatersort.goldpack5" },
        { Config.IAPPackageID.GoldPack6, "com.mygamez.magicwatersort.goldpack6" },
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
            InitializePurchasing();
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

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add all products to the builder
        foreach (var kvp in productIdMap)
        {
            builder.AddProduct(kvp.Value, ProductType.Consumable);
            Debug.Log($"[IAPManager] Added product: {kvp.Value} for package {kvp.Key}");
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAPManager] Initialization successful");
        storeController = controller;
        extensionProvider = extensions;
        isInitialized = true;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAPManager] Initialization failed: {error}");
        isInitialized = false;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAPManager] Initialization failed: {error}, {message}");
        isInitialized = false;
    }

    /// <summary>
    /// Initiate a purchase for the given package ID
    /// </summary>
    /// <param name="packageID">The package to purchase</param>
    /// <param name="onComplete">Callback: (packageID, success, errorMessage)</param>
    public void PurchaseProduct(Config.IAPPackageID packageID, Action<Config.IAPPackageID, bool, string> onComplete)
    {
        if (!isInitialized)
        {
            Debug.LogError("[IAPManager] Store not initialized yet");
            onComplete?.Invoke(packageID, false, "Store not initialized");
            return;
        }

        if (!productIdMap.ContainsKey(packageID))
        {
            Debug.LogError($"[IAPManager] Product ID not found for package: {packageID}");
            onComplete?.Invoke(packageID, false, $"Product ID not found for package: {packageID}");
            return;
        }

        string productId = productIdMap[packageID];
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

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"[IAPManager] Purchase successful: {args.purchasedProduct.definition.id}");

        // Find the package ID from the product ID
        Config.IAPPackageID? packageID = null;
        foreach (var kvp in productIdMap)
        {
            if (kvp.Value == args.purchasedProduct.definition.id)
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
        
        // Prepare payload
        var payload = new Dictionary<string, object>
        {
            {"receipt_data", receiptData},
            {"product_id", productId},
            {"package_id", packageID.ToString()}
        };

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
        foreach (var kvp in productIdMap)
        {
            if (kvp.Value == product.definition.id)
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
        foreach (var kvp in productIdMap)
        {
            if (kvp.Value == product.definition.id)
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
        return productIdMap.ContainsKey(packageID) ? productIdMap[packageID] : null;
    }

    /// <summary>
    /// Check if the store is initialized and ready
    /// </summary>
    public bool IsReady()
    {
        return isInitialized && storeController != null;
    }
}

