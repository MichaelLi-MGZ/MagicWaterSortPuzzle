#ifndef _MYGAMEZ_NATIVE_PLUGIN_H_
#define _MYGAMEZ_NATIVE_PLUGIN_H_

#if __cplusplus
extern "C" {
#endif

// ErrorCode
/*
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
SdkBusy = -10
*/

// EventCode
/*
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
GeneralError = 40
*/

// used to notify about status of asynchronous operations
// @param eventCode - EventCode (see above)
typedef void (*callback_function)(int eventCode);
// used to retrieve prompt data
typedef void (*callback_function_prompt)(const char* title, const char* body, const char* button);

// @return ErrorCode (see above)
int mygamez_initiaize(const char* cpid, const char* authParams, const char* backendUrlPrefix, callback_function cbk);
int mygamez_request_guest_mode(callback_function cbk);
int mygamez_request_game_start(callback_function cbk);
int mygamez_attempt_rid_check(const char* name, const char* rid, callback_function cbk);
int mygamez_verify_identity(const char* name, const char* rid, callback_function cbk);
int mygamez_request_inapp_purchase(float price, callback_function cbk);
int mygamez_announce_completed_inapp_purchase(float price, callback_function cbk);
void mygamez_request_guest_mode_timeup_prompt(callback_function_prompt cbk);
void mygamez_request_rid_check_fails_prompt(callback_function_prompt cbk);
void mygamez_request_guest_mode_prompt(callback_function_prompt cbk);
void mygamez_request_player_identification_completed_prompt(callback_function_prompt cbk);
void mygamez_request_daily_game_time_depletion_prompt(callback_function_prompt cbk);
void mygamez_request_time_of_day_constraint_prompt(callback_function_prompt cbk);
void mygamez_request_store_enter_prompt(callback_function_prompt cbk);
void mygamez_request_single_purhcase_limit_exceeded_prompt(callback_function_prompt cbk);
void mygamez_request_monthly_purhcase_limit_exceeded_prompt(callback_function_prompt cbk);
int mygamez_delete_user_account();
// @return 1 - TRUE, 0 - FALSE
int mygamez_is_guest_mode();
int mygamez_is_adult();
// @return play time
int mygamez_get_playtime_left();
// @return credit
float mygamez_get_iap_credit_left();
// @return mygamez player ID
const char* mygamez_get_current_mygamez_id();

#if __cplusplus
}
#endif

#endif
