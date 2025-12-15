using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MyGamezBridge : MonoBehaviour
{
#if UNITY_IOS
    const string dll = "__Internal";
#else
    const string dll = "mygamez_ios-sdk"; // name of the native plugin
#endif
    public enum ErrorCode : int
    {
        Success = 0,
        RidInIncorrectFormat = -1,
        NameLengthInvalid = -2,
        NameNotInChineseLetters = -3,
        UserRightsNotDetermined = -4,
        InvalidParameterError = -5,
        AlreadyPlaying = -6,
        AlreadyInGuestMode = -7,
        SdkNotInitilized = -8,
        InternalSdkError = -9,
        SdkBusy = -10,
        UnknownError = -1000 // not part of the SDK (problems in error code mapping)
    };

    public enum EventCode : int
    {
        // original API
        UserRightsDetermined = 0,
        RidCheckRequired,
        GuestModeNotGranted,
        GameStartAllowed,
        GuestModeGameTimeDepleted,
        DailyGameTimeDepleted,
        PlayingNotAllowedDueToTimeOfDayConstraints,
        IapAllowed,
        IapNotAllowedSinglePurchaseLimitExceeded,
        IapNotAllowedMonthlyPurchaseLimitExceeded,
        IapNotAllowedInGuestMode,
        IapNotAllowedAgeCriteriaNotMet,
        CompletedIapAcknowledged,
        // back-end error codes
        BackendAuthUnknownProvider,
        BackendAuthParams,
        BackendAuthFail,
        BackendRinMalformed,
        BackendRinChecksum,
        BackendIdInvalid,
        BackendNameInvalid,
        BackendUnknownApp,
        BackendUnauthorized,
        BackendUninit,
        BackendUnprocessable,
        BackendSystem,
        BackendTimeout,
        BackendSession,
        BackendGovtConn,
        BackendGovtSys,
        BackendRefuse,
        BackendCheckPending,
        // original API
        GeneralError = 40
    };

    //
    // From native to Unity callbacks
    //
    // @param EventCode - one of the codes above
    public delegate void EventCodeCallback(EventCode eventCode);

    // @param title - prompt's title text
    // @param body - prompt's body text
    // @param button - prompt's button text
    public delegate void PromptDataCallback(IntPtr title, IntPtr body, IntPtr button);

    //
    // native API
    //
    // @return ErrorCode
    // Success
    // InvalidParameterError
    // SdkBusy
    //
    // @param cbk
    // UserRightsDetermined
    // RidCheckRequired
    // DailyGameTimeDepleted
    // PlayingNotAllowedDueToTimeOfDayConstraints
    // GeneralError
    // BackendAuthUnknownProvider
    // BackendAuthParams
    // BackendAuthFail
    // BackendRinMalformed
    // BackendRinChecksum
    // BackendIdInvalid
    // BackendNameInvalid
    // BackendUnknownApp
    // BackendUnauthorized
    // BackendUninit
    // BackendUnprocessable
    // BackendSystem
    // BackendTimeout
    // BackendSession
    // BackendGovtConn
    // BackendGovtSys
    // BackendRefuse
    [DllImport(dll)]
    private static extern int mygamez_initiaize(string cpid, string authParams, string backendUrlPrefix, EventCodeCallback cbk);

    // @return ErrorCode
    // Success
    // AlreadyInGuestMode
    // SdkNotInitilized
    // SdkBusy
    //
    // @param cbk
    // UserRightsDetermined
    // GuestModeNotGranted
    // GeneralError
    [DllImport(dll)]
    private static extern int mygamez_request_guest_mode(EventCodeCallback cbk);

    // @return ErrorCode
    // Success
    // UserRightsNotDetermined
    // AlreadyPlaying
    // SdkNotInitilized
    // SdkBusy
    //
    // @param cbk
    // GameStartAllowed
    // GuestModeGameTimeDepleted
    // DailyGameTimeDepleted
    // PlayingNotAllowedDueToTimeOfDayConstraints
    // GeneralError
    [DllImport(dll)]
    private static extern int mygamez_request_game_start(EventCodeCallback cbk);

    // @return ErrorCode
    // Success
    // RidInIncorrectFormat
    // NameLengthInvalid
    // NameNotInChineseLetters
    // SdkNotInitilized
    // SdkBusy
    //
    // @param cbk
    // UserRightsDetermined
    // RidCheckRequired
    // DailyGameTimeDepleted
    // PlayingNotAllowedDueToTimeOfDayConstraints
    // GeneralError
    // BackendAuthUnknownProvider
    // BackendAuthParams
    // BackendAuthFail
    // BackendRinMalformed
    // BackendRinChecksum
    // BackendIdInvalid
    // BackendNameInvalid
    // BackendUnknownApp
    // BackendUnauthorized
    // BackendUninit
    // BackendUnprocessable
    // BackendSystem
    // BackendTimeout
    // BackendSession
    // BackendGovtConn
    // BackendGovtSys
    // BackendRefuse
    [DllImport(dll)]
    private static extern int mygamez_attempt_rid_check(string name, string rid, EventCodeCallback cbk);

    [DllImport(dll)]
    private static extern int mygamez_verify_identity(string name, string rid, EventCodeCallback cbk);

    // @return ErrorCode
    // Success
    // UserRightsNotDetermined
    // SdkNotInitilized
    // SdkBusy
    //
    // @param cbk
    // IapAllowed
    // IapNotAllowedSinglePurchaseLimitExceeded
    // IapNotAllowedMonthlyPurchaseLimitExceeded
    // IapNotAllowedInGuestMode
    // IapNotAllowedAgeCriteriaNotMet
    [DllImport(dll)]
    private static extern int mygamez_request_inapp_purchase(float price, EventCodeCallback cbk);

    // @return ErrorCode
    // Success
    // UserRightsNotDetermined
    // SdkNotInitilized
    // SdkBusy
    //
    // @param cbk
    // CompletedIapAcknowledged
    [DllImport(dll)]
    private static extern int mygamez_announce_completed_inapp_purchase(float price, EventCodeCallback cbk);

    //
    // These methods return in callback three Chinese localised strings being used in prompts (see PromptDataCallback definition)
    //
    [DllImport(dll)]
    private static extern void mygamez_request_guest_mode_timeup_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_rid_check_fails_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_guest_mode_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_player_identification_completed_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_daily_game_time_depletion_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_time_of_day_constraint_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_store_enter_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_single_purhcase_limit_exceeded_prompt(PromptDataCallback cbk);
    [DllImport(dll)]
    private static extern void mygamez_request_monthly_purhcase_limit_exceeded_prompt(PromptDataCallback cbk);

    [DllImport(dll)]
    private static extern int mygamez_is_guest_mode();
    [DllImport(dll)]
    private static extern int mygamez_get_playtime_left();
    [DllImport(dll)]
    private static extern float mygamez_get_iap_credit_left();
    [DllImport(dll)]
    private static extern int mygamez_is_adult();
    [DllImport(dll)]
    private static extern int mygamez_delete_user_account();

    [DllImport(dll)]
    private static extern IntPtr mygamez_get_current_mygamez_id();

    //
    // Unity plugin API
    //
    public static ErrorCode Initialize(string cpid, string json, string url, EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("Initialize() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_initiaize(cpid, json, url, cbk);
#endif
        return SdkError(ret);
    }

    public static ErrorCode RequestGuestMode(EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("RequestGuestMode() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_request_guest_mode(cbk);
#endif
        return SdkError(ret);
    }

    public static ErrorCode RequestGameStart(EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("RequestGameStart() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_request_game_start(cbk);
#endif
        return SdkError(ret);
    }


    public static ErrorCode VerifyIdentity(string name, string rid, EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("VerifyIdentity() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_verify_identity(name, rid, cbk);
#endif
        return SdkError(ret);
    }

    public static ErrorCode AttemptRidCheck(string name, string rid, EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("AttemptRidCheck() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_attempt_rid_check(name, rid, cbk);
#endif
        return SdkError(ret);
    }

    public static ErrorCode RequestInappPurchase(float price, EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("RequestInappPurchase() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_request_inapp_purchase(price, cbk);
#endif
        return SdkError(ret);
    }

    public static ErrorCode AnnounceCompletedInappPurchase(float price, EventCodeCallback cbk)
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("AnnounceCompletedInappPurchase() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_announce_completed_inapp_purchase(price, cbk);
#endif
        return SdkError(ret);
    }

    public static ErrorCode DeleteUserAccount()
    {
        int ret = (int)ErrorCode.UnknownError;
#if UNITY_EDITOR
        Debug.Log("DeleteUserAccount() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_delete_user_account();
#endif
        return SdkError(ret);
    }

    public static void RequestGuestModeTimeUpPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestGuestModeTimeUpPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_guest_mode_timeup_prompt(cbk);
#endif
    }

    public static void RequestRidCheckFailsPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestRidCheckFailsPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_rid_check_fails_prompt(cbk);
#endif
    }

    public static void RequestGuestModePrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestGuestModePrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_guest_mode_prompt(cbk);
#endif
    }

    public static void RequestPlayerIdentificationCompletedPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestPlayerIdentificationCompletedPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_player_identification_completed_prompt(cbk);
#endif
    }

    public static void RequestDailyGameTimeDepletionPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestDailyGameTimeDepletionPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_daily_game_time_depletion_prompt(cbk);
#endif
    }

    public static void RequestTimeOfDayConstraintPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestTimeOfDayConstraintPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_time_of_day_constraint_prompt(cbk);
#endif
    }

    public static void RequestStoreEnterPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestStoreEnterPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_store_enter_prompt(cbk);
#endif
    }

    public static void RequestSinglePurchaseLimitExceededPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestSinglePurchaseLimitExceededPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_single_purhcase_limit_exceeded_prompt(cbk);
#endif
    }

    public static void RequestMonthlyPurchaseLimitExceededPrompt(PromptDataCallback cbk)
    {
#if UNITY_EDITOR
        Debug.Log("RequestMonthlyPurchaseLimitExceededPrompt() not implemented on Unity Editor");
#elif UNITY_IOS
        mygamez_request_monthly_purhcase_limit_exceeded_prompt(cbk);
#endif
    }

    public static bool IsGuestMode()
    {
        bool ret = false;
#if UNITY_EDITOR
        Debug.Log("IsGuestMode() not implemented on Unity Editor");
#elif UNITY_IOS
        int flag = mygamez_is_guest_mode();
        ret = (flag == 1);
#endif
        return ret;
    }

    public static int GetPlaytimeLeft()
    {
        int ret = 0;
#if UNITY_EDITOR
        Debug.Log("GetPlaytimeLeft() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_get_playtime_left();
#endif
        return ret;
    }

    public static float GetIapCreditLeft()
    {
        float ret = 0;
#if UNITY_EDITOR
        Debug.Log("GetIapCreditLeft() not implemented on Unity Editor");
#elif UNITY_IOS
        ret = mygamez_get_iap_credit_left();
#endif
        return ret;
    }

    public static bool IsAdult()
    {
        bool ret = false;
#if UNITY_EDITOR
        Debug.Log("IsAdult() not implemented on Unity Editor");
#elif UNITY_IOS
        int flag = mygamez_is_adult();
        ret = (flag == 1);
#endif
        return ret;
    }

    //

    public static string getCurrentMyGamezId()
    {
        string ret = "";
#if UNITY_EDITOR
        Debug.Log("getCurrentMyGamezId() not implemented on Unity Editor");
#elif UNITY_IOS
        IntPtr str = mygamez_get_current_mygamez_id();
        ret = Marshal.PtrToStringAnsi(str);
#endif
        return ret;
    }

    public static ErrorCode SdkError(int errorCode)
    {
        if (errorCode == 0)
            return ErrorCode.Success;
        else if (errorCode == -1)
            return ErrorCode.RidInIncorrectFormat;
        else if (errorCode == -2)
            return ErrorCode.NameLengthInvalid;
        else if (errorCode == -3)
            return ErrorCode.NameNotInChineseLetters;
        else if (errorCode == -4)
            return ErrorCode.UserRightsNotDetermined;
        else if (errorCode == -5)
            return ErrorCode.InvalidParameterError;
        else if (errorCode == -6)
            return ErrorCode.AlreadyPlaying;
        else if (errorCode == -7)
            return ErrorCode.AlreadyInGuestMode;
        else if (errorCode == -8)
            return ErrorCode.SdkNotInitilized;
        else if (errorCode == -9)
            return ErrorCode.InternalSdkError;
        else if (errorCode == -10)
            return ErrorCode.SdkBusy;

        return ErrorCode.UnknownError;
    }
}
