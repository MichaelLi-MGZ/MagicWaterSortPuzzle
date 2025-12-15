using UnityEngine;
using AOT;
using System;
using System.Runtime.InteropServices;

//
// MyGamezGameObject
//
public class MyGamezGameObject : MonoBehaviour
{
    public delegate void PromptDataCallback(string title, string body, string button);

    // callbacks
    static MyGamezBridge.EventCodeCallback m_callbackInit;
    static MyGamezBridge.EventCodeCallback m_callbackStart;
    static MyGamezBridge.EventCodeCallback m_callbackRidck;
    static MyGamezBridge.EventCodeCallback m_callbackGuest;
    static MyGamezBridge.EventCodeCallback m_callbackInapp;
    static MyGamezBridge.EventCodeCallback m_callbackAnnon;
    static MyGamezBridge.EventCodeCallback m_callbackIdent;
    static PromptDataCallback m_callbackPrompt;

    // wrap calls without callback parameter
    public static string GetCurrentMyGamezId()
    {
        return MyGamezBridge.getCurrentMyGamezId();
    }
    public static bool IsAdult()
    {
        return MyGamezBridge.IsAdult();
    }
    public static bool IsGuestMode()
    {
        return MyGamezBridge.IsGuestMode();
    }
    public static int GetPlaytimeLeft()
    {
        return MyGamezBridge.GetPlaytimeLeft();
    }
    public static float GetIapCreditLeft()
    {
        return MyGamezBridge.GetIapCreditLeft();
    }

    //
    // MyGamez API helper
    //
    public static MyGamezBridge.ErrorCode DoInitialize(string cpid, string url, string authParams, MyGamezBridge.EventCodeCallback callback)
    {
        Debug.Log("DoInitialize() url=" + url + " authParams=" + authParams + " cpid=" + cpid); // DEBUG

        m_callbackInit = callback; // copy
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.Initialize(cpid, authParams, url, InitializeCallback);
        return errorCode;
    }

    public static MyGamezBridge.ErrorCode DoStart(MyGamezBridge.EventCodeCallback callback)
    {
        m_callbackStart = callback;
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.RequestGameStart(RequestGameStartCallback);
        return errorCode;
    }


    public static MyGamezBridge.ErrorCode DoVerifyIdentity(string name, string rid, MyGamezBridge.EventCodeCallback callback)
    {
        m_callbackIdent = callback;
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.VerifyIdentity(name, rid, VerifyIdentityCallback);
        return errorCode;
    }

    public static MyGamezBridge.ErrorCode DoAttemptRidCheck(string name, string rid, MyGamezBridge.EventCodeCallback callback)
    {
        m_callbackRidck = callback;
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.AttemptRidCheck(name, rid, AttemptRidCheckCallback);
        return errorCode;
    }

    public static MyGamezBridge.ErrorCode DoRequestGuestMode(MyGamezBridge.EventCodeCallback callback)
    {
        m_callbackGuest = callback;
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.RequestGuestMode(RequestGuestModeCallback);
        return errorCode;
    }

    public static MyGamezBridge.ErrorCode DoRequestInappPurchase(float price, MyGamezBridge.EventCodeCallback callback)
    {
        m_callbackInapp = callback;
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.RequestInappPurchase(price, RequestInappPurchaseCallback);
        return errorCode;
    }

    public static MyGamezBridge.ErrorCode DoAnnounceCompletedInappPurchase(float price, MyGamezBridge.EventCodeCallback callback)
    {
        m_callbackAnnon = callback;
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.AnnounceCompletedInappPurchase(price, AnnounceCompletedInappPurchaseCallback);
        return errorCode;
    }

    public static MyGamezBridge.ErrorCode DoDeleteUserAccount()
    {
        MyGamezBridge.ErrorCode errorCode = MyGamezBridge.DeleteUserAccount();
        return errorCode;
    }

    public static void DoRequestPromptCallback(int index, PromptDataCallback callback)
    {
        if (index < 0 || index > 8)
        {
            index = 0;
            Debug.Log("DoRequestPromptCallback() ERROR! index is out of range (0 - 8)");
        }

        m_callbackPrompt = callback;

        if (index == 0)
            MyGamezBridge.RequestGuestModeTimeUpPrompt(RequestPromptCallback);
        else if (index == 1)
            MyGamezBridge.RequestRidCheckFailsPrompt(RequestPromptCallback);
        else if (index == 2)
            MyGamezBridge.RequestGuestModePrompt(RequestPromptCallback);
        else if (index == 3)
            MyGamezBridge.RequestPlayerIdentificationCompletedPrompt(RequestPromptCallback);
        else if (index == 4)
            MyGamezBridge.RequestDailyGameTimeDepletionPrompt(RequestPromptCallback);
        else if (index == 5)
            MyGamezBridge.RequestTimeOfDayConstraintPrompt(RequestPromptCallback);
        else if (index == 6)
            MyGamezBridge.RequestStoreEnterPrompt(RequestPromptCallback);
        else if (index == 7)
            MyGamezBridge.RequestSinglePurchaseLimitExceededPrompt(RequestPromptCallback);
        else if (index == 8)
            MyGamezBridge.RequestMonthlyPurchaseLimitExceededPrompt(RequestPromptCallback);
    }

    //
    // Callbacks
    //
    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void InitializeCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("InitializeCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackInit != null)
        {
            m_callbackInit(eventCode);
        }
    }

    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void RequestGameStartCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("RequestGameStartCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackStart != null)
        {
            m_callbackStart(eventCode);
        }
    }


    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void VerifyIdentityCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("VerifyIdentityCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackIdent != null)
        {
            m_callbackIdent(eventCode);
        }
    }

    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void AttemptRidCheckCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("AttemptRidCheckCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackRidck != null)
        {
            m_callbackRidck(eventCode);
        }
    }

    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void RequestGuestModeCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("RequestGuestModeCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackGuest != null)
        {
            m_callbackGuest(eventCode);
        }
    }

    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void RequestInappPurchaseCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("RequestInappPurchaseCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackInapp != null)
        {
            m_callbackInapp(eventCode);
        }
    }

    [MonoPInvokeCallback(typeof(MyGamezBridge.EventCodeCallback))]
    private static void AnnounceCompletedInappPurchaseCallback(MyGamezBridge.EventCode eventCode)
    {
        Debug.Log("AnnounceCompletedInappPurchaseCallback() eventCode=" + eventCode); // DEBUG

        if (m_callbackAnnon != null)
        {
            m_callbackAnnon(eventCode);
        }
    }

    [MonoPInvokeCallback(typeof(MyGamezBridge.PromptDataCallback))]
    private static void RequestPromptCallback(IntPtr title, IntPtr body, IntPtr button)
    {
        string ttl = Marshal.PtrToStringAnsi(title);
        string bod = Marshal.PtrToStringAnsi(body);
        string btn = Marshal.PtrToStringAnsi(button);
        Debug.Log("RequestPromptCallback() \ntitle=" + ttl + "\nbody=" + bod + "\nbutton=" + btn); // DEBUG

        if (m_callbackPrompt != null)
        {
            m_callbackPrompt(ttl, bod, btn);
        }
    }

    public static string GetErrorCodeMessage(MyGamezBridge.ErrorCode code)
    {
        string msg = "Unknown Error";
        if (code == MyGamezBridge.ErrorCode.Success)
            msg = "Success";
        else if (code == MyGamezBridge.ErrorCode.RidInIncorrectFormat)
            msg = "Invalid RID";
        else if (code == MyGamezBridge.ErrorCode.NameLengthInvalid)
            msg = "Name length invalid";
        else if (code == MyGamezBridge.ErrorCode.NameNotInChineseLetters)
            msg = "Name is not Chinese";
        else if (code == MyGamezBridge.ErrorCode.UserRightsNotDetermined)
            msg = "User rights not yet determined";
        else if (code == MyGamezBridge.ErrorCode.InvalidParameterError)
            msg = "Invalid parameter error";
        else if (code == MyGamezBridge.ErrorCode.AlreadyPlaying)
            msg = "Already playing";
        else if (code == MyGamezBridge.ErrorCode.AlreadyInGuestMode)
            msg = "Already in guest mode";
        else if (code == MyGamezBridge.ErrorCode.SdkNotInitilized)
            msg = "SDK not initialized";
        else if (code == MyGamezBridge.ErrorCode.InternalSdkError)
            msg = "Internal SDK error";
        else if (code == MyGamezBridge.ErrorCode.SdkBusy)
            msg = "SDK busy";
        return msg;
    }

    public static string GetEventCodeMessage(MyGamezBridge.EventCode code)
    {
        string msg = "Unknown Error";
        if (code == MyGamezBridge.EventCode.UserRightsDetermined)
            msg = "Authentication successful";
        else if (code == MyGamezBridge.EventCode.RidCheckRequired)
            msg = "Authentication error. RID Check required";
        else if (code == MyGamezBridge.EventCode.GuestModeNotGranted)
            msg = "Guest mode not allowed";
        else if (code == MyGamezBridge.EventCode.GameStartAllowed)
            msg = "Game start allowed";
        else if (code == MyGamezBridge.EventCode.GuestModeGameTimeDepleted)
            msg = "Guest mode time depleted";
        else if (code == MyGamezBridge.EventCode.DailyGameTimeDepleted)
            msg = "Daily game time depleted";
        else if (code == MyGamezBridge.EventCode.PlayingNotAllowedDueToTimeOfDayConstraints)
            msg = "Playing not allowed due to time of day constraints";
        else if (code == MyGamezBridge.EventCode.IapAllowed)
            msg = "IAP Allowed";
        else if (code == MyGamezBridge.EventCode.IapNotAllowedSinglePurchaseLimitExceeded)
            msg = "IAP not allowed (single purchase limit exceeded)";
        else if (code == MyGamezBridge.EventCode.IapNotAllowedMonthlyPurchaseLimitExceeded)
            msg = "IAP not allowed (monthly purhcase limit exceeded)";
        else if (code == MyGamezBridge.EventCode.IapNotAllowedInGuestMode)
            msg = "IAP not allowed (in guest mode)";
        else if (code == MyGamezBridge.EventCode.IapNotAllowedAgeCriteriaNotMet)
            msg = "IAP not allowed (age criteria not met)";
        else if (code == MyGamezBridge.EventCode.CompletedIapAcknowledged)
            msg = "Completed IAP acknowledged";
        else if (code == MyGamezBridge.EventCode.GeneralError)
            msg = "SDK Error occurred";
        else if (code == MyGamezBridge.EventCode.BackendAuthUnknownProvider)
            msg = "BackendAuthUnknownProvider";
        else if (code == MyGamezBridge.EventCode.BackendAuthParams)
            msg = "BackendAuthParams";
        else if (code == MyGamezBridge.EventCode.BackendAuthFail)
            msg = "BackendAuthFail";
        else if (code == MyGamezBridge.EventCode.BackendRinMalformed)
            msg = "BackendRinMalformed";
        else if (code == MyGamezBridge.EventCode.BackendRinChecksum)
            msg = "BackendRinChecksum";
        else if (code == MyGamezBridge.EventCode.BackendIdInvalid)
            msg = "BackendIdInvalid";
        else if (code == MyGamezBridge.EventCode.BackendNameInvalid)
            msg = "BackendNameInvalid";
        else if (code == MyGamezBridge.EventCode.BackendUnknownApp)
            msg = "BackendUnknownApp";
        else if (code == MyGamezBridge.EventCode.BackendUnauthorized)
            msg = "BackendUnauthorized";
        else if (code == MyGamezBridge.EventCode.BackendUninit)
            msg = "BackendUninit";
        else if (code == MyGamezBridge.EventCode.BackendUnprocessable)
            msg = "BackendUnprocessable";
        else if (code == MyGamezBridge.EventCode.BackendSystem)
            msg = "BackendSystem";
        else if (code == MyGamezBridge.EventCode.BackendTimeout)
            msg = "BackendTimeout";
        else if (code == MyGamezBridge.EventCode.BackendSession)
            msg = "BackendSession";
        else if (code == MyGamezBridge.EventCode.BackendGovtConn)
            msg = "BackendGovtConn";
        else if (code == MyGamezBridge.EventCode.BackendGovtSys)
            msg = "BackendGovtSys";
        else if (code == MyGamezBridge.EventCode.BackendRefuse)
            msg = "BackendRefuse";
        else if (code == MyGamezBridge.EventCode.BackendCheckPending)
            msg = "BackendCheckPending";
        return msg;
    }
}
