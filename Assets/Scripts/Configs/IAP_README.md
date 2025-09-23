# IAP Configuration System

## 🎯 Overview
This document explains the configurable IAP (In-App Purchase) system that uses MySDK billing.

## 📋 How It Works

### 1. Package Configuration
All IAP packages are defined in `Config.cs` using the `IAPPackageConfig` class:

```csharp
public class IAPPackageConfig
{
    public IAPPackageID packageID;    // Unique identifier
    public int price;                 // Price in fens (Chinese cents)
    public string name;               // Display name
    public string description;        // Description for player
    public int coinAmount;            // Coins to give player
    public bool isRemoveAds;          // Whether this removes ads
}
```

### 2. Package Enum
Packages are defined in the `IAPPackageID` enum:

```csharp
public enum IAPPackageID
{
    NoAds,      // Remove ads package
    GoldPack1,  // 250 coins
    GoldPack2,  // 500 coins
    GoldPack3,  // 750 coins
    GoldPack4,  // 1000 coins
    GoldPack5,  // 1500 coins
    GoldPack6,  // 2000 coins
}
```

### 3. Configuration Methods
- **`GetIAPPackageConfigs()`** - Returns all package configurations
- **`GetPackageConfig(packageID)`** - Returns specific package configuration

## 🛒 Adding New Packages

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
    GoldPack7,  // ← Add your new package here
}
```

### Step 2: Add Configuration
Add the package configuration to `GetIAPPackageConfigs()`:

```csharp
new IAPPackageConfig
{
    packageID = IAPPackageID.GoldPack7,
    price = 700, // 7.00 CNY
    name = "3000 Gold Coins",
    description = "Get 3000 gold coins to unlock new items",
    coinAmount = 3000,
    isRemoveAds = false
}
```

### Step 3: Use in ShopView
The package will automatically work in `ShopView` - no additional code needed!

## 💰 Pricing System

### Price Format
- **Price is in fens** (Chinese cents)
- **100 fens = 1.00 CNY**
- **Examples:**
  - 100 fens = ¥1.00
  - 200 fens = ¥2.00
  - 500 fens = ¥5.00

### Current Packages
| Package | Price (fens) | Price (CNY) | Coins | Description |
|---------|--------------|-------------|-------|-------------|
| NoAds | 100 | ¥1.00 | 0 | Remove all ads |
| GoldPack1 | 100 | ¥1.00 | 250 | Small coin pack |
| GoldPack2 | 200 | ¥2.00 | 500 | Medium coin pack |
| GoldPack3 | 300 | ¥3.00 | 750 | Large coin pack |
| GoldPack4 | 400 | ¥4.00 | 1000 | Extra large coin pack |
| GoldPack5 | 500 | ¥5.00 | 1500 | Mega coin pack |
| GoldPack6 | 600 | ¥6.00 | 2000 | Ultimate coin pack |

## 🔧 Technical Details

### MySDK Integration
The system uses MySDK billing with these components:

1. **IAPInfo** - Display information for player
2. **PayInfo** - Game identification and custom data
3. **Biller** - Payment provider (ISBN)
4. **PayCallback** - Handles payment results

### Payment Flow
1. **Player clicks purchase** button in ShopView
2. **ShopView calls** `PurchaseGoldPack(packageID)`
3. **MySDK billing** is initiated with package info
4. **Payment completes** and callback is triggered
5. **Player receives** coins/ads removal

### Custom ID Format
```
iap-{packageID}-{timestamp}
```
**Example:** `iap-goldpack1-638123456789012345`

## 🧪 Testing

### Test Purchases
1. **Run the game** and go to shop
2. **Click any purchase** button
3. **Check console logs** for payment flow
4. **Verify coins** are added to player account

### Debug Logs
Look for these messages:
```
ShopView: Starting MySDK billing for package: GoldPack1, using biller: ISBN
ShopView Payment Result: SUCCESS, Biller: ISBN
ShopView: Payment was successful!
ShopView: Extracted package ID: 'GoldPack1' from custom ID: 'iap-goldpack1-...'
ShopView: 250 Gold Coins - Processing purchase
ShopView: Adding 250 coins
```

## 🎉 Benefits

### For Developers:
- ✅ **Easy to add** new packages
- ✅ **Centralized configuration** in one file
- ✅ **Type-safe** enum system
- ✅ **Automatic integration** with ShopView

### For Players:
- ✅ **Consistent pricing** across all packages
- ✅ **Clear descriptions** of what they're buying
- ✅ **Reliable payment** processing via MySDK
- ✅ **Instant delivery** of purchased items

## 🚀 Future Enhancements

### Possible Additions:
- **Bundle packages** (multiple items in one purchase)
- **Limited time offers** with special pricing
- **Subscription packages** for recurring benefits
- **Currency conversion** for international markets

The IAP system is now fully configurable and ready for easy expansion! 🎮