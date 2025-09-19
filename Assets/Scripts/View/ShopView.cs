using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
using static AdsControl;
using MyGamez.MySDK.Api;
using MyGamez.Demo.MySDKHelpers;

public class ShopView : BaseView
{
    public ShopTab[] shopTab;

    public int totalItem;

    public ItemShopData[] bottleDataList;

    public ItemShopData[] wallPaperDataList;

    public ItemShopData[] palettesDataList;

    public ItemShopUI itemShopUIPrefab;

    public Transform productRootTrs;

    public List<ItemShopUI> itemShopUIList;

    public Vector2[] itemSize;

    public Sprite selectSpr, unSelectSpr;

    public Sprite strokeLockSpr, strokeNormal, strokeSelect;

    public int currentTab;

    public int currentItemID;

    public ItemShopUI focusItem, currentItem;

    public TextMeshProUGUI coinTxt;

    public Transform coinIconInBoard;
    

    public override void InitView()
    {
        itemShopUIList = new List<ItemShopUI>();
        currentTab = 0;
        currentItemID = 0;
        InitShopItem();
        SelectFlaskTab();
        GetCurrentSelectID();
        SelectedItem();
        coinTxt.text = GameManager.instance.currentCoin.ToString();
    }

    public override void ShowView()
    {
        base.ShowView();
    }

    public override void HideView()
    {
        base.HideView();
        AudioManager.instance.clickBtn.Play();
        //GameManager.instance.levelGen.UpdateBottleSkin(PlayerPrefs.GetInt("CurrentBottle"));
        //GameManager.instance.levelGen.RefreshColor(GameManager.instance.levelGen.colorConfig[PlayerPrefs.GetInt("CurrentPalette")]);
    }

    void GetCurrentSelectID()
    {
        if (currentTab == 0)
        {
            currentItemID = PlayerPrefs.GetInt("CurrentBottle");
            currentItem = itemShopUIList[currentItemID];

        }
        else if (currentTab == 1)
        {
            currentItemID = PlayerPrefs.GetInt("CurrentWall");
            currentItem = itemShopUIList[currentItemID];

        }
        else if (currentTab == 2)
        {
            currentItemID = PlayerPrefs.GetInt("CurrentPalette");
            currentItem = itemShopUIList[currentItemID];

        }
    }

    void InitShopItem()
    {

        if (itemShopUIList.Count > 0)
            return;


        itemShopUIList.Add(itemShopUIPrefab);
        for (int i = 1; i < totalItem; i++)
        {
            ItemShopUI itemShopUI = Instantiate(itemShopUIPrefab, Vector3.zero, Quaternion.identity);
            itemShopUI.transform.parent = productRootTrs;
            itemShopUI.transform.localPosition = Vector3.zero;
            itemShopUI.transform.localScale = Vector3.one;
            itemShopUIList.Add(itemShopUI);
            itemShopUI.itemID = i;
        }
    }

    public override void Start()
    {
        // Set up MySDK payment callback for ShopView
        SetupMySDKPaymentCallback();
    }
    
    /// <summary>
    /// Set up MySDK payment callback for ShopView purchases
    /// </summary>
    private void SetupMySDKPaymentCallback()
    {
        // Create a ShopView-specific payment callback
        ShopViewPayCallback shopPayCallback = new ShopViewPayCallback(this);
        
        // Set the callback for MySDK billing
        Billing.SetPayCallback(shopPayCallback);
        
        Debug.Log("ShopView: MySDK payment callback set up successfully");
    }

    public override void Update()
    {

    }

    private void SelectTab(bool select, ShopTab shopTab)
    {

        if (select)
            shopTab.SelectTab.color = new Color(1, 1, 1, 1);
        else
            shopTab.SelectTab.color = new Color(1, 1, 1, 0);

        shopTab.SelectContent.SetActive(select);
        shopTab.UnSelectTab.SetActive(!select);
        shopTab.isSelect = select;


    }

    public void SelectFlaskTab()
    {
        if (isShow)
            AudioManager.instance.clickBtn.Play();

        if (shopTab[0].isSelect)
            return;

        //AudioManager.instance.clickBtn.Play();
        SelectTab(true, shopTab[0]);
        SelectTab(false, shopTab[1]);
        SelectTab(false, shopTab[2]);
        currentTab = 0;
        LoadBottleShopList();
        GetCurrentSelectID();
        SelectedItem();
    }

    public void SelecWallTab()
    {
        if (isShow)
            AudioManager.instance.clickBtn.Play();

        if (shopTab[1].isSelect)
            return;

        //AudioManager.instance.clickBtn.Play();
        SelectTab(false, shopTab[0]);
        SelectTab(true, shopTab[1]);
        SelectTab(false, shopTab[2]);
        currentTab = 1;
        LoadWallShopList();
        GetCurrentSelectID();
        SelectedItem();
    }

    public void SelecPalletesTab()
    {
        if (isShow)
            AudioManager.instance.clickBtn.Play();

        if (shopTab[2].isSelect)
            return;

        //AudioManager.instance.clickBtn.Play();
        SelectTab(false, shopTab[0]);
        SelectTab(false, shopTab[1]);
        SelectTab(true, shopTab[2]);
        currentTab = 2;
        LoadPalettesShopList();
        GetCurrentSelectID();
        SelectedItem();
    }

    public void LoadBottleShopList()
    {
        for (int i = 0; i < totalItem; i++)
        {
            itemShopUIList[i].itemIcon.sprite = bottleDataList[i].itemSpr;
            itemShopUIList[i].itemIcon.rectTransform.sizeDelta = itemSize[0];
            itemShopUIList[i].itemTxt.gameObject.SetActive(false);

            if (PlayerPrefs.GetInt("Bottle" + i.ToString()) == 1)
            {
                itemShopUIList[i].selectIcon.sprite = unSelectSpr;
                itemShopUIList[i].strokeImage.sprite = strokeNormal;
                itemShopUIList[i].strokeImage.SetNativeSize();
                itemShopUIList[i].itemLockMask.SetActive(false);
                itemShopUIList[i].itemLock.SetActive(false);

            }

            else
            {
                itemShopUIList[i].selectIcon.sprite = unSelectSpr;
                itemShopUIList[i].strokeImage.sprite = strokeLockSpr;
                itemShopUIList[i].strokeImage.SetNativeSize();
                itemShopUIList[i].itemLockMask.SetActive(true);
                itemShopUIList[i].itemLock.SetActive(true);
            }

        }
    }

    public void LoadWallShopList()
    {
        for (int i = 0; i < totalItem; i++)
        {
            itemShopUIList[i].itemIcon.sprite = wallPaperDataList[i].itemSpr;
            itemShopUIList[i].itemIcon.rectTransform.sizeDelta = itemSize[1];
            itemShopUIList[i].itemTxt.gameObject.SetActive(false);

            if (PlayerPrefs.GetInt("Wall" + i.ToString()) == 1)
            {
                itemShopUIList[i].selectIcon.sprite = unSelectSpr;
                itemShopUIList[i].strokeImage.sprite = strokeNormal;
                itemShopUIList[i].strokeImage.SetNativeSize();
                itemShopUIList[i].itemLockMask.SetActive(false);
                itemShopUIList[i].itemLock.SetActive(false);
            }

            else
            {
                itemShopUIList[i].selectIcon.sprite = unSelectSpr;
                itemShopUIList[i].strokeImage.sprite = strokeLockSpr;
                itemShopUIList[i].strokeImage.SetNativeSize();
                itemShopUIList[i].itemLockMask.SetActive(true);
                itemShopUIList[i].itemLock.SetActive(true);
            }
        }
    }

    public void LoadPalettesShopList()
    {
        for (int i = 0; i < totalItem; i++)
        {
            itemShopUIList[i].itemIcon.sprite = palettesDataList[i].itemSpr;
            itemShopUIList[i].itemIcon.rectTransform.sizeDelta = itemSize[2];
            itemShopUIList[i].itemTxt.gameObject.SetActive(true);
            itemShopUIList[i].itemTxt.text = palettesDataList[i].itemName;

            if (PlayerPrefs.GetInt("Palette" + i.ToString()) == 1)
            {
                itemShopUIList[i].selectIcon.sprite = unSelectSpr;
                itemShopUIList[i].strokeImage.sprite = strokeNormal;
                itemShopUIList[i].strokeImage.SetNativeSize();
                itemShopUIList[i].itemLockMask.SetActive(false);
                itemShopUIList[i].itemLock.SetActive(false);
            }

            else
            {
                itemShopUIList[i].selectIcon.sprite = unSelectSpr;
                itemShopUIList[i].strokeImage.sprite = strokeLockSpr;
                itemShopUIList[i].strokeImage.SetNativeSize();
                itemShopUIList[i].itemLockMask.SetActive(true);
                itemShopUIList[i].itemLock.SetActive(true);
            }
        }
    }

    public void UnlockItem()
    {
        if (GameManager.instance.currentCoin < 400)
        {
            GameManager.instance.uiManager.warningView.ShowView("No Enough Coins");
            return;
        }


        if (currentTab == 0)
        {
            List<int> lockItemIndex = new List<int>();

            for (int i = 0; i < totalItem; i++)
            {


                if (PlayerPrefs.GetInt("Bottle" + i) == 0)
                {
                    lockItemIndex.Add(i);

                }



            }

            if (lockItemIndex.Count > 0)
            {
                int randomUnlockIndex = lockItemIndex[Random.Range(0, lockItemIndex.Count)];
                PlayerPrefs.SetInt("Bottle" + randomUnlockIndex, 1);
                GameManager.instance.SubCoin(400);
            }

            LoadBottleShopList();
        }
        else if (currentTab == 1)
        {
            List<int> lockItemIndex = new List<int>();

            for (int i = 0; i < totalItem; i++)
            {
                if (PlayerPrefs.GetInt("Wall" + i) == 0)
                {

                    lockItemIndex.Add(i);
                }
            }

            if (lockItemIndex.Count > 0)
            {
                int randomUnlockIndex = lockItemIndex[Random.Range(0, lockItemIndex.Count)];
                PlayerPrefs.SetInt("Wall" + randomUnlockIndex, 1);
                GameManager.instance.SubCoin(400);
            }

            LoadWallShopList();
        }
        else if (currentTab == 2)
        {
            List<int> lockItemIndex = new List<int>();

            for (int i = 0; i < totalItem; i++)
            {
                if (PlayerPrefs.GetInt("Palette" + i) == 0)
                {
                    lockItemIndex.Add(i);


                }
            }

            if (lockItemIndex.Count > 0)
            {
                int randomUnlockIndex = lockItemIndex[Random.Range(0, lockItemIndex.Count)];
                PlayerPrefs.SetInt("Palette" + randomUnlockIndex, 1);
                GameManager.instance.SubCoin(400);
            }

            LoadPalettesShopList();
        }

        AudioManager.instance.waterFull.Play();

        SelectedItem();
    }

    public void CheckSelectItem()
    {
        if (currentTab == 0)
        {
            if (PlayerPrefs.GetInt("Bottle" + focusItem.itemID) == 1)
            {
                currentItem = focusItem;
                SelectedItem();
                GameManager.instance.levelGen.UpdateBottleSkin(PlayerPrefs.GetInt("CurrentBottle"));
            }
        }
        else if (currentTab == 1)
        {
            if (PlayerPrefs.GetInt("Wall" + focusItem.itemID) == 1)
            {
                currentItem = focusItem;
                SelectedItem();
                GameManager.instance.bgManager.SetBG(PlayerPrefs.GetInt("CurrentWall"));
            }
        }
        else if (currentTab == 2)
        {
            if (PlayerPrefs.GetInt("Palette" + focusItem.itemID) == 1)
            {
                currentItem = focusItem;
                SelectedItem();
                GameManager.instance.levelGen.RefreshColor(GameManager.instance.levelGen.colorConfig[PlayerPrefs.GetInt("CurrentPalette")]);
            }
        }
    }

    public void SelectedItem()
    {
        if (isShow)
            AudioManager.instance.clickBtn.Play();

        UnSelectAllItem();

        currentItem.selectIcon.sprite = selectSpr;
        currentItem.strokeImage.sprite = strokeSelect;
        currentItem.strokeImage.SetNativeSize();

        if (currentTab == 0)
        {
            PlayerPrefs.SetInt("CurrentBottle", currentItem.itemID);

        }
        else if (currentTab == 1)
        {
            PlayerPrefs.SetInt("CurrentWall", currentItem.itemID);

        }
        else if (currentTab == 2)
        {
            PlayerPrefs.SetInt("CurrentPalette", currentItem.itemID);

        }

        //AudioManager.instance.clickBtn.Play();
    }

    public void UnSelectAllItem()
    {

        for (int i = 0; i < totalItem; i++)
        {
            itemShopUIList[i].selectIcon.sprite = unSelectSpr;

            if (currentTab == 0)
            {
                if (PlayerPrefs.GetInt("Bottle" + i) == 1)
                {
                    itemShopUIList[i].strokeImage.sprite = strokeNormal;
                    itemShopUIList[i].strokeImage.SetNativeSize();
                }
            }
            else if (currentTab == 1)
            {
                if (PlayerPrefs.GetInt("Wall" + i) == 1)
                {
                    itemShopUIList[i].strokeImage.sprite = strokeNormal;
                    itemShopUIList[i].strokeImage.SetNativeSize();
                }
            }
            else if (currentTab == 2)
            {
                if (PlayerPrefs.GetInt("Palette" + i) == 1)
                {
                    itemShopUIList[i].strokeImage.sprite = strokeNormal;
                    itemShopUIList[i].strokeImage.SetNativeSize();
                }
            }


        }

    }

    public void AddBonusCoin()
    {
        // Purchase 250 gold coins using MySDK billing
        PurchaseGoldPack(Config.IAPPackageID.GoldPack1);
    }
    
    /// <summary>
    /// Purchase a specific package using MySDK billing
    /// </summary>
    /// <param name="packageID">The package to purchase</param>
    private void PurchaseGoldPack(Config.IAPPackageID packageID)
    {
        // Get package configuration from Config
        var packageConfig = Config.GetPackageConfig(packageID);
        if (packageConfig == null)
        {
            Debug.LogError($"Package configuration not found for: {packageID}");
            return;
        }
        
        // Step 1: Create IAPInfo (for player display)
        MyGamez.MySDK.Api.Billing.IAPInfo iapInfo = new MyGamez.MySDK.Api.Billing.IAPInfo(
            packageConfig.price, 
            packageConfig.name, 
            packageConfig.description
        );
        
        // Step 2: Create PayInfo (for game identification)
        string customID = $"iap-{packageID.ToString().ToLower()}-{System.DateTime.Now.Ticks}";
        string extraInfo = $"Purchase: {packageConfig.name}, Amount: {packageConfig.coinAmount} coins";
        MyGamez.MySDK.Api.Billing.PayInfo payInfo = new MyGamez.MySDK.Api.Billing.PayInfo(iapInfo, customID, extraInfo);
        
        // Step 3: Get available billers and start billing
        List<MyGamez.MySDK.Api.Billing.Biller> billers = Billing.GetAvailableBillers();
        if (billers.Count > 0)
        {
            Debug.Log($"ShopView: Starting MySDK billing for package: {packageID}, using biller: {billers[0]}");
            Billing.DoBilling(billers[0], payInfo);
        }
        else
        {
            Debug.LogError("ShopView: No available billers found for MySDK billing");
        }
    }
    

    public void AddBonusCoinCB()
    {
        // Purchase 250 gold coins using MySDK billing
        PurchaseGoldPack(Config.IAPPackageID.GoldPack1);
    }

    public void RemoveAds()
    {
        // Purchase remove ads using MySDK billing
        PurchaseGoldPack(Config.IAPPackageID.NoAds);
    }

    public void BuyIAPPackage(Config.IAPPackageID packageID)
    {
        IAPManager.instance.BuyConsumable(packageID, (string iapID, IAPManager.IAP_CALLBACK_STATE state) =>
        {
            if (state == IAPManager.IAP_CALLBACK_STATE.SUCCESS)
            {

                Debug.Log("SUCCESSSUCCESS " + iapID);

                if (iapID.Equals(Config.IAPPackageID.NoAds.ToString()))
                {
                    Debug.Log("REMOVE ADS");
                    AdsControl.Instance.RemoveAds();
                }
                else if (iapID.Equals(Config.IAPPackageID.GoldPack1.ToString()))
                {
                    Debug.Log("GOLD PACK 1");
                    GameManager.instance.AddCoin(250);
                }
                
                else if (iapID.Equals(Config.IAPPackageID.GoldPack2.ToString()))
                {
                    Debug.Log("GOLD PACK 2");
                    GameManager.instance.AddCoin(500);
                }
                
                else if (iapID.Equals(Config.IAPPackageID.GoldPack3.ToString()))
                {
                    Debug.Log("GOLD PACK 3");
                    GameManager.instance.AddCoin(750);
                }

                else if (iapID.Equals(Config.IAPPackageID.GoldPack4.ToString()))
                {
                    Debug.Log("GOLD PACK 3");
                    GameManager.instance.AddCoin(750);
                }
                
            }
            else
            {
                Debug.Log("Buy Fail!");

            }
        });
    }

     public void WatchAds()
    {
        AudioManager.instance.clickBtn.Play();
        if (AdsControl.Instance.currentAdsType == ADS_TYPE.ADMOB)
        {
            if (AdsControl.Instance.rewardedAd != null)
            {
                if (AdsControl.Instance.rewardedAd.CanShowAd())
                {
                    AdsControl.Instance.ShowRewardAd(EarnReward);
                }
            }
        }
        else if (AdsControl.Instance.currentAdsType == ADS_TYPE.UNITY)
        {
            ShowRWUnityAds();
        }
        else if (AdsControl.Instance.currentAdsType == ADS_TYPE.MEDIATION)
        {
            if (AdsControl.Instance.rewardedAd.CanShowAd())

                AdsControl.Instance.ShowRewardAd(EarnReward);

            else
                ShowRWUnityAds();
        }
    }

    public void EarnReward(Reward reward)
    {
      AddBonusCoinCB();
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
              AddBonusCoinCB();
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }
}

#region Payment

/// <summary>
/// ShopView-specific payment callback for MySDK billing
/// </summary>
public class ShopViewPayCallback : Billing.IPayCallback
{
    private ShopView shopView;
    
    public ShopViewPayCallback(ShopView shop)
    {
        shopView = shop;
    }
    
    public void OnBillingResult(MyGamez.MySDK.Api.Billing.BillingResult result)
    {
        Debug.Log($"ShopView Payment Result: {result.ResultCode}, Biller: {result.Biller}");
        
        if (result.ResultCode == MyGamez.MySDK.Api.ResultCode.SUCCESS)
        {
            Debug.Log("ShopView: Payment was successful!");
            Debug.Log($"Payment PayInfo.CustomID: {result.PayInfo.CustomID}");
            Debug.Log($"Payment PayInfo.ExtraInfo: {result.PayInfo.ExtraInfo}");
            Debug.Log($"Payment IapInfo.Name: {result.PayInfo.IapInfo.Name}");
            Debug.Log($"Payment IapInfo.AmountFen: {result.PayInfo.IapInfo.AmountFen}");
            

            
            // Print verification data for server validation
            if (result.Verification != null)
            {
                Debug.Log("ShopView: Payment verification data available for server validation");
                LocalVerificationHelper.PrintVerificationData(result.Verification);
            }

            // Extract package ID from custom ID
            string packageID = ExtractPackageIDFromCustomID(result.PayInfo.CustomID);
            Debug.Log($"ShopView: Extracted package ID: '{packageID}' from custom ID: '{result.PayInfo.CustomID}'");
            
            // Handle the successful purchase based on package ID
            HandleSuccessfulPurchase(packageID, result);
            
            // Confirm to MySDK that player has received what they purchased
            result.ConfirmGoodsGiven();
        }
        else
        {
            Debug.Log($"ShopView: Payment failed - {result.ResultCode}: {result.ResultMsg}");
        }
    }
    
    /// <summary>
    /// Extract package ID from custom ID format: "iap-{packageID}-{timestamp}"
    /// </summary>
    private string ExtractPackageIDFromCustomID(string customID)
    {
        if (string.IsNullOrEmpty(customID))
            return "unknown";
            
        // Format: "iap-{packageID}-{timestamp}"
        string[] parts = customID.Split('-');
        if (parts.Length >= 2)
        {
            string packageID = parts[1];
            
            // Try to find matching enum value dynamically
            foreach (Config.IAPPackageID enumValue in System.Enum.GetValues(typeof(Config.IAPPackageID)))
            {
                if (string.Equals(enumValue.ToString(), packageID, System.StringComparison.OrdinalIgnoreCase))
                {
                    return enumValue.ToString();
                }
            }
            
            // Fallback: try to convert from lowercase to PascalCase
            if (packageID.Length > 0)
            {
                return char.ToUpper(packageID[0]) + packageID.Substring(1);
            }
        }
        
        return "unknown";
    }
    
    /// <summary>
    /// Handle successful purchase by giving items to player
    /// </summary>
    private void HandleSuccessfulPurchase(string packageID, MyGamez.MySDK.Api.Billing.BillingResult result)
    {
        Debug.Log($"ShopView: Processing successful purchase for package: {packageID}");
        
        // Try to parse the package ID to enum
        if (System.Enum.TryParse<Config.IAPPackageID>(packageID, out Config.IAPPackageID packageEnum))
        {
            // Get package configuration
            var packageConfig = Config.GetPackageConfig(packageEnum);
            if (packageConfig != null)
            {
                Debug.Log($"ShopView: {packageConfig.name} - Processing purchase");
                
                // Handle remove ads
                if (packageConfig.isRemoveAds)
                {
                    Debug.Log("ShopView: REMOVE ADS");
                    if (AdsControl.Instance != null)
                    {
                        AdsControl.Instance.RemoveAds();
                    }
                }
                
                // Handle coin rewards
                if (packageConfig.coinAmount > 0)
                {
                    Debug.Log($"ShopView: Adding {packageConfig.coinAmount} coins");
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.AddCoin(packageConfig.coinAmount);
                    }
                }
            }
            else
            {
                Debug.LogError($"ShopView: Package configuration not found for: {packageID}");
            }
        }
        else
        {
            Debug.LogWarning($"ShopView: Unknown package ID: {packageID}");
        }
    }
}

#endregion


[System.Serializable]
public class ShopTab
{

    public Image SelectTab;

    public GameObject UnSelectTab;

    public GameObject SelectContent;

    public bool isSelect;

}

[System.Serializable]
public class ItemShopData
{

    public string itemName;

    public Sprite itemSpr;

}

