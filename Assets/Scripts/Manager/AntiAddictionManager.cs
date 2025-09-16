using UnityEngine;

public class AntiAddictionManager : MonoBehaviour
{
    public static AntiAddictionManager Instance { get; private set; }

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

    // Anti-Addiction helper methods
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

    public bool IsPlayerAdult()
    {
        bool isAdult = MyGamez.MySDK.Api.AntiAddiction.IsAdult();
        Debug.Log("Is player adult: " + isAdult);
        return isAdult;
    }

    public void RequestInAppPurchase(float price)
    {
        Debug.Log("Requesting IAP permission for price: " + price);
        MyGamez.MySDK.Api.AntiAddiction.RequestInAppPurchase(price, new IAPCallback());
    }

    public void AnnounceCompletedPurchase(float price)
    {
        Debug.Log("Announcing completed purchase: " + price);
        MyGamez.MySDK.Api.AntiAddiction.AnnounceCompletedInappPurchase(price, new IAPCallback());
    }

    // Identity Verification helper methods
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

    // Dialog data helper methods
    public void ShowRidCheckFailsDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetRidCheckFailsPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("RID Check Fails Dialog - Title: " + dialogData.Title);
            Debug.Log("RID Check Fails Dialog - Body: " + dialogData.Body);
            Debug.Log("RID Check Fails Dialog - Button: " + dialogData.Button);
        }
    }

    public void ShowPlayerIdentificationCompletedDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetPlayerIdentificationCompletedPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Player Identification Completed Dialog - Title: " + dialogData.Title);
            Debug.Log("Player Identification Completed Dialog - Body: " + dialogData.Body);
            Debug.Log("Player Identification Completed Dialog - Button: " + dialogData.Button);
        }
    }

    public void ShowTimeOfDayConstraintDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetTimeOfDayConstraintPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Time Of Day Constraint Dialog - Title: " + dialogData.Title);
            Debug.Log("Time Of Day Constraint Dialog - Body: " + dialogData.Body);
            Debug.Log("Time Of Day Constraint Dialog - Button: " + dialogData.Button);
        }
    }

    public void ShowStoreEnterDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetStoreEnterPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Store Enter Dialog - Title: " + dialogData.Title);
            Debug.Log("Store Enter Dialog - Body: " + dialogData.Body);
            Debug.Log("Store Enter Dialog - Button: " + dialogData.Button);
        }
    }

    public void ShowSinglePurchaseLimitExceededDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetSinglePurchaseLimitExceededPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Single Purchase Limit Exceeded Dialog - Title: " + dialogData.Title);
            Debug.Log("Single Purchase Limit Exceeded Dialog - Body: " + dialogData.Body);
            Debug.Log("Single Purchase Limit Exceeded Dialog - Button: " + dialogData.Button);
        }
    }

    public void ShowMonthlyPurchaseLimitExceededDialog()
    {
        var dialogData = MyGamez.MySDK.Api.AntiAddiction.GetMonthlyPurchaseLimitExceededPromptDialogData();
        if (dialogData != null)
        {
            Debug.Log("Monthly Purchase Limit Exceeded Dialog - Title: " + dialogData.Title);
            Debug.Log("Monthly Purchase Limit Exceeded Dialog - Body: " + dialogData.Body);
            Debug.Log("Monthly Purchase Limit Exceeded Dialog - Button: " + dialogData.Button);
        }
    }

    private class IAPCallback : MyGamez.MySDK.Api.AntiAddiction.IAntiAddictionCallback
    {
        public void OnAntiAddictionResult(MyGamez.MySDK.Api.ResultCode resultCode, string msg)
        {
            Debug.Log("IAP Anti-Addiction result: " + resultCode + " - " + msg);
            
            switch (resultCode)
            {
                case MyGamez.MySDK.Api.ResultCode.SUCCESS:
                    Debug.Log("IAP request approved");
                    break;
                case MyGamez.MySDK.Api.ResultCode.LIMITED:
                    Debug.Log("IAP request limited - purchase limit exceeded");
                    break;
                case MyGamez.MySDK.Api.ResultCode.DISALLOWED:
                    Debug.Log("IAP request disallowed");
                    break;
                default:
                    Debug.LogError("IAP request failed: " + resultCode + " - " + msg);
                    break;
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
                case MyGamez.MySDK.Api.ResultCode.GENERAL_ERROR:
                default:
                    Debug.LogError("Identity verification error: " + resultCode + " - " + msg);
                    break;
            }
        }
    }
}