using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
    #region IAP

    public enum IAPPackageID
    {
        NoAds,
        GoldPack1,
        GoldPack2,
        GoldPack3,
        GoldPack4,
        GoldPack5,
        GoldPack6, // Example: Adding a new package is this easy!
    }

    /// <summary>
    /// IAP Package configuration data
    /// </summary>
    [System.Serializable]
    public class IAPPackageConfig
    {
        public IAPPackageID packageID;
        public int price; // Price in fens (Chinese cents)
        public string name;
        public string description;
        public int coinAmount;
        public bool isRemoveAds;
    }

    /// <summary>
    /// Get all IAP package configurations
    /// To add a new package, just add it to the enum and add its config here
    /// </summary>
    public static IAPPackageConfig[] GetIAPPackageConfigs()
    {
        return new IAPPackageConfig[]
        {
            new IAPPackageConfig
            {
                packageID = IAPPackageID.NoAds,
                price = 100, // 1.00 CNY
                name = "移除广告",
                description = "移除游戏中的所有广告",
                coinAmount = 0,
                isRemoveAds = true
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack1,
                price = 100, // 1.00 CNY
                name = "250 金币",
                description = "获得250金币来解锁新物品",
                coinAmount = 250,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack2,
                price = 200, // 2.00 CNY
                name = "500 金币",
                description = "获得500金币来解锁新物品",
                coinAmount = 500,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack3,
                price = 300, // 3.00 CNY
                name = "750 金币",
                description = "获得750金币来解锁新物品",
                coinAmount = 750,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack4,
                price = 400, // 4.00 CNY
                name = "1000 金币",
                description = "获得1000金币来解锁新物品",
                coinAmount = 1000,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack5,
                price = 500, // 5.00 CNY
                name = "1500 金币",
                description = "获得1500金币来解锁新物品",
                coinAmount = 1500,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack6,
                price = 600, // 6.00 CNY
                name = "2000 金币",
                description = "获得2000金币来解锁新物品",
                coinAmount = 2000,
                isRemoveAds = false
            }
        };
    }

    /// <summary>
    /// Get package configuration by ID
    /// </summary>
    public static IAPPackageConfig GetPackageConfig(IAPPackageID packageID)
    {
        var configs = GetIAPPackageConfigs();
        foreach (var config in configs)
        {
            if (config.packageID == packageID)
                return config;
        }
        return null;
    }

    #endregion
}
