using MyGamez.MySDK.Api;
using UnityEngine;
using static MyGamez.MySDK.Api.AntiAddiction;

// This namespace has examples of listeners/callbacks that implement MySDK Interfaces.
namespace MyGamez.Demo.MySDKHelpers
{
    public abstract class MyGamezObserver : MonoBehaviour
    {
        public abstract void OnGoldUpdated(int gold);
        public abstract void OnLoginStateChanged(MySDK.Api.Login.LoginState loginState);
        public abstract void OnMySDKInit(MySDKInit.MySDKInitResult result);
        public abstract void OnRIDCheckResult(string rid, string name, MySDK.Api.ResultCode resultCode, string msg);
        public abstract void OnAntiAddictionEvent(MySDK.Api.ResultCode resultCode, string msg);
        public abstract void OnAntiAddictionInitResult(MySDK.Api.ResultCode resultCode, string msg);
    }

    
    public class AntiAddictionEventListener : MySDK.Api.AntiAddiction.IAntiAddictionCallback
    {
        private MyGamezObserver observer;
        public AntiAddictionEventListener(MyGamezObserver obs)
        {
            this.observer = obs;
        }

        public void OnAntiAddictionResult(ResultCode resultCode, string msg)
        {
            this.observer.OnAntiAddictionEvent(resultCode, msg);
        }
    }
   
    public class AntiAddictionCallback : MySDK.Api.AntiAddiction.IAntiAddictionCallback
    {
        private MyGamezObserver observer;
        public AntiAddictionCallback(MyGamezObserver obs)
        {
            this.observer = obs;
        }

        public void OnAntiAddictionResult(ResultCode resultCode, string msg)
        {
            this.observer.OnAntiAddictionInitResult(resultCode, msg);
        }
    }

    public class RIDCheckValidationListener : MySDK.Api.AntiAddiction.IRidCheckCallback
    {
        private MyGamezObserver observer;
        public RIDCheckValidationListener(MyGamezObserver obs)
        {
            this.observer = obs;
        }

        public void OnResult(string rid, string name, ResultCode resultCode, string msg)
        {
            this.observer.OnRIDCheckResult(rid, name, resultCode, msg);
        }
    }

    public class MySDKInitCallback : MySDK.Api.MySDKInit.IMySDKInitCallback
    {
        private MyGamezObserver observer;
        public MySDKInitCallback(MyGamezObserver obs)
        {
            this.observer = obs;
        }
        public void OnResult(MySDKInit.MySDKInitResult result)
        {
            observer.OnMySDKInit(result);
        }
    }

    #region Payment

    // This is example of PayCallback
    public class PayCallbackExample : MyGamez.MySDK.Api.Billing.IPayCallback
    {
        private MyGamezObserver observer;
        public PayCallbackExample(MyGamezObserver obs)
        {
            observer = obs;
        }

        public void OnBillingResult(MyGamez.MySDK.Api.Billing.BillingResult result)
        {
            // This method is triggered when player exits payment process. 
            // Implement your own logic here: Check if payment was successful, Send verification data to game server, provide items to player etc.
            // This implementation is for demonstration purposes only


            // Set Status Text to Demo UI
            ToastMessage.Show("Billing Result is " + result.ResultCode);

            // Check if payment was successful
            if (ResultCode.SUCCESS.Equals(result.ResultCode))
            {
                Debug.Log("Payment was successful. " +
                    "Payment ResultCode is " + result.ResultCode +
                    ", ResultMsg is " + result.ResultMsg +
                    ", Biller is " + result.Biller);


                // PaymentResult.PayInfo has information about this payment
                // PayInfo.CustomID - This ID is set when creating payment. See Button("Buy 50 Gold") in DemoUIController.cs for example.
                // PayInfo.ExtraInfo - This info is set when creating payment. See Button("Buy 50 Gold") in DemoUIController.cs for example.
                // PayInfo.CreateTime - MySDK sets this before opening payment dialog. Uses MyGamez server time (UTC time).
                // PayInfo.OrderID - MyGamez ID for this payment.
                Debug.Log("Payment PayInfo.CustomID: " + result.PayInfo.CustomID);
                Debug.Log("Payment PayInfo.ExtraInfo: " + result.PayInfo.ExtraInfo);
                Debug.Log("Payment PayInfo.CreateTime: " + result.PayInfo.CreateTime);
                Debug.Log("Payment PayInfo.OrderID: " + result.PayInfo.OrderID);

                // PaymentResult.PayInfo.IapInfo has price (AmountFen), name and description of this IAP.
                Debug.Log("Payment IapInfo.AmountFen: " + result.PayInfo.IapInfo.AmountFen);
                Debug.Log("Payment IapInfo.Name: " + result.PayInfo.IapInfo.Name);
                Debug.Log("Payment IapInfo.Desc: " + result.PayInfo.IapInfo.Desc);

                // Successful payment has Verification data that can be sent to game server for payment validation.
                // This demo will only print verification data to log. 
                LocalVerificationHelper.PrintVerificationData(result.Verification);

                // Give items to player
                if (observer != null)
                    observer.OnGoldUpdated(50);

                // Confirm to MySDK that player has received what they purchased
                result.ConfirmGoodsGiven();
            }
            else
            {
                Debug.Log("Payment was NOT successful. " +
                    "Payment ResultCode is " + result.ResultCode +
                    ", ResultMsg is " + result.ResultMsg +
                    ", Biller is " + result.Biller);
            }

        }
    }

    #endregion

    #region Features

    #region Text Validation
    /// <summary>
    /// This is example of Text Validation Callback
    /// </summary>
    public class TextValidationCallbackExample : MySDK.Api.Features.TextValidation.ITextValidationCallback
    {
        // Callback method
        public void OnTextValidation(Features.TextValidation.TextValidationResult result)
        {
            ToastMessage.Show("Text validation result: " + result.ResultCode);
        }
    }
    #endregion

    #endregion

    #region Login

    /// <summary>
    /// This is example of Login State Listener
    /// </summary>
    public class LoginListenerExample: MySDK.Api.Login.ILoginStateListener
    {
        private MyGamezObserver observer;
        public LoginListenerExample(MyGamezObserver obs)
        {
            observer = obs;
        }

        public void OnLoginStateChanged(MySDK.Api.Login.LoginState loginState)
        {
            // This method is triggered when player logs in, logs in as a guest or logs out. 
            // Implement your own logic here.
            // This implementation is for demonstration purposes only
            Debug.Log("LoginState changed! LoginState is " + loginState.ToString());
            Debug.Log("observer is " + (observer != null));
            if (observer != null) {
                observer.OnLoginStateChanged(loginState);
            }
            else
            {
                Debug.Log("observer is null");
            }
        }
    }

    #endregion

    /// <summary>
    /// This is only for demonstration purposes!
    /// It is highly recommended to validate signature and process payload on server.
    /// </summary>
    static class LocalVerificationHelper
    {
        public static void PrintVerificationData(MySDK.Api.Security.Verification verification)
        {
            if (verification == null)
            {
                Debug.Log("Verification was null");
                return;
            }

            // Verification.KID - First 6 characters of SHA3-256(DER-PublicKey)
            // Verification.Payload - Base64 endcoded payment result json
            // Verification.Signature - Base64 encoded ECDSA-SHA3-256 signature, directly calculated from payload value.
            Debug.Log("Verification.KID: " + (verification.KID == null ? "null"  : verification.KID));
            Debug.Log("Verification.Payload: " + (verification.Payload == null ? "null" : verification.Payload));
            Debug.Log("Verification.Signature: " + (verification.Signature == null ? "null" : verification.Signature));
            if (verification.Payload != null)
                Debug.Log("Verification.Payload in clear text: " + System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(verification.Payload)));
        }
        public static string VerificationDataToString(MySDK.Api.Security.Verification verification)
        {
            string res;
            if (verification == null)
                res = "null";
            else
                res = "Verification {"
                    + "kId=" + (verification.KID == null ? "null" : verification.KID)
                    + ", payload=" + (verification.Payload == null ? "null" : verification.Payload)
                    + ", signature==" + (verification.Signature == null ? "null" : verification.Signature)
                    + "}";
            return res;
        }
    }

    static class LoginInfoHelper {
        public static string LoginInfoToString(MySDK.Api.Login.LoginInfo loginInfo)
        {
            string res;
            if (loginInfo == null)
                res = "null";
            else
                res = "LoginInfo {"
                    + "playerID=" + (loginInfo.PlayerID == null ? "null" : loginInfo.PlayerID)
                    + ", vendorName=" + (loginInfo.VendorName == null ? "null" : loginInfo.VendorName)
                    + ", loginState=" + loginInfo.LoginState.ToString()
                    + ", verification=" + (loginInfo.Verification == null ? "null" : LocalVerificationHelper.VerificationDataToString(loginInfo.Verification))
                    + "}";
            return res;
        }
    }
}