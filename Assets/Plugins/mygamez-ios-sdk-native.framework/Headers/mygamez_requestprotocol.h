//
//  mygamez_requestprotocol.h
//  MyGamez-IOS-SDK
//
//  @copyright MyGamez Ltd
//

#ifndef MYGAMEZ_REQUEST_PROTOCOL_H
#define MYGAMEZ_REQUEST_PROTOCOL_H

@class MygamezRequest;

@protocol MygamezRequestProtocol <NSObject>
@required
-(void)onRequestCompleted:(MygamezRequest*)request responseData:(NSDictionary*)responseData errorCode:(NSInteger)errorCode;
@end

#endif
