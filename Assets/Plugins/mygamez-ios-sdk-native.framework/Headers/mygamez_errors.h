//
//  mygamez_errors.h
//  MyGamez-IOS-SDK
//
//  @copyright MyGamez Ltd
//

#ifndef MYGAMEZ_ERRORS_H
#define MYGAMEZ_ERRORS_H

typedef NS_ENUM(int, ErrorCode) {
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
};

typedef NS_ENUM(int, EventCode) {
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

#endif
