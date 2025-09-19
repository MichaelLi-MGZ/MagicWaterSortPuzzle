# IAP Package Configuration Guide

## How to Add a New IAP Package

Adding a new IAP package is now super easy! You only need to make changes in **one file**: `Config.cs`

### Step 1: Add to Enum
Add your new package to the `IAPPackageID` enum:

```csharp
public enum IAPPackageID
{
    NoAds,
    GoldPack1,
    GoldPack2,
    GoldPack3,
    GoldPack4,
    GoldPack5,
    GoldPack6,
    GoldPack7, // ← Add your new package here
}
```

### Step 2: Add Configuration
Add the package configuration to the `GetIAPPackageConfigs()` method:

```csharp
new IAPPackageConfig
{
    packageID = IAPPackageID.GoldPack7,
    price = 700, // 7.00 CNY in fens
    name = "3000 Gold Coins",
    description = "Get 3000 gold coins to unlock new items",
    coinAmount = 3000,
    isRemoveAds = false
}
```

### That's it! 🎉

The system will automatically:
- ✅ Handle the purchase flow
- ✅ Process payment results
- ✅ Give the correct rewards
- ✅ Support all existing functionality

### Configuration Options

| Field | Description | Example |
|-------|-------------|---------|
| `packageID` | Must match enum value | `IAPPackageID.GoldPack7` |
| `price` | Price in fens (Chinese cents) | `700` = ¥7.00 |
| `name` | Display name for player | `"3000 Gold Coins"` |
| `description` | Description for player | `"Get 3000 gold coins..."` |
| `coinAmount` | Coins to give (0 for no coins) | `3000` |
| `isRemoveAds` | Whether this removes ads | `true` or `false` |

### Special Package Types

#### Gold Packs
```csharp
coinAmount = 1000,  // Give coins
isRemoveAds = false // Don't remove ads
```

#### Remove Ads
```csharp
coinAmount = 0,     // No coins
isRemoveAds = true  // Remove ads
```

#### Combo Packs
```csharp
coinAmount = 500,   // Give coins
isRemoveAds = true  // Also remove ads
```

## No More Hardcoding! 🚀

The old system required changes in multiple files with hardcoded switch statements. Now everything is centralized and configurable!

### Dynamic Package Processing

The system now automatically:
- ✅ **Dynamically extracts package IDs** from payment results
- ✅ **Matches enum values** without hardcoded mappings
- ✅ **Processes rewards** based on configuration
- ✅ **Handles new packages** automatically

No more ugly switch statements! Everything is clean and maintainable.