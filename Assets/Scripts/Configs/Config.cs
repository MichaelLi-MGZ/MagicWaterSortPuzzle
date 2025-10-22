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
                price = 1000, // 10.00 CNY
                name = "移除广告",
                description = "移除游戏中的所有广告",
                coinAmount = 0,
                isRemoveAds = true
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack1,
                price = 1800, // 18.00 CNY
                name = "200 金币",
                description = "获得200金币来解锁新物品",
                coinAmount = 200,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack2,
                price = 5800, // 58.00 CNY
                name = "800 金币",
                description = "获得800金币来解锁新物品",
                coinAmount = 800,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack3,
                price = 12800, // 128.00 CNY
                name = "1500 金币",
                description = "获得1500金币来解锁新物品",
                coinAmount = 1500,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack4,
                price = 18800, // 188.00 CNY
                name = "3000 金币",
                description = "获得3000金币来解锁新物品",
                coinAmount = 3000,
                isRemoveAds = false
            },
            new IAPPackageConfig
            {
                packageID = IAPPackageID.GoldPack5,
                price = 38800, // 388.00 CNY
                name = "9000 金币",
                description = "获得9000金币来解锁新物品",
                coinAmount = 9000,
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
