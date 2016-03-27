/*-------------------------------------------------------------------------------------------------------------------*\
 |  Copyright (C) 2015 PayPal                                                                                          |
 |                                                                                                                     |
 |  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance     |
 |  with the License.                                                                                                  |
 |                                                                                                                     |
 |  You may obtain a copy of the License at                                                                            |
 |                                                                                                                     |
 |       http://www.apache.org/licenses/LICENSE-2.0                                                                    |
 |                                                                                                                     |
 |  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed   |
 |  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for  |
 |  the specific language governing permissions and limitations under the License.                                     |
 \*-------------------------------------------------------------------------------------------------------------------*/

#import "PPManticoreNativeInterface.h"
#import "PPManticoreEngine+Private.h"
#import "PPManticoreError.h"

@interface PPManticoreNativeInterface ()

@property (nonatomic, weak) PPManticoreEngine *engine;

@end

@implementation PPManticoreNativeInterface

- (instancetype)initWithEngine:(PPManticoreEngine *)engine {
    if ((self = [super init])) {
        self.engine = engine;
        
        __weak PPManticoreNativeInterface *weakSelf = self;
        
        [engine.jsEngine evaluateScript:@"global.manticore = {platform:{}};"];
        _manticoreObject = engine.jsEngine[@"manticore"];

        JSValue *platform = _manticoreObject[@"platform"];
        // TODO maybe fill out some more stuff...
#if TARGET_OS_IPHONE
        platform[@"name"] = @"iOS";
        if ([NSProcessInfo instancesRespondToSelector:@selector(isOperatingSystemAtLeastVersion:)]) {
            // conditionally check for any version >= iOS 8 using 'isOperatingSystemAtLeastVersion'
            platform[@"version"] = [NSProcessInfo processInfo].operatingSystemVersionString;
        } else {
            // we're on iOS 7 or below
            [[UIDevice currentDevice] systemVersion];
        }
#else
        platform[@"name"] = @"osx";
        platform[@"version"] = [NSProcessInfo processInfo].operatingSystemVersionString;
#endif
        _manticoreObject[@"log"] = ^(NSString* level,  NSString* message) {
            [weakSelf log:level message:message];
        };
        
        _manticoreObject[@"http"] = ^(JSValue *options, JSValue *callback) {
            return [weakSelf http:options callback:callback];
        };
        
        _manticoreObject[@"setTimeout"] = ^(JSValue *callback, JSValue *timeout) {
            [weakSelf setTimeout:^{
                [callback callWithArguments:nil];
            } after:timeout.toInt32];
        };
        
        engine.jsEngine.exceptionHandler = ^(JSContext *context, JSValue *error) {
            NSLog(@"Error %@\n%@", error[@"message"], error[@"stack"]);
            NSDictionary *errorDictionary = [weakSelf.engine.converter toNativeObject:error];
            NSMutableString *reason = [NSMutableString stringWithString:@"Manticore engine encountered an error."];
            if (error[@"message"]) {
                [reason appendFormat:@"\nMessage: %@", error[@"message"]];
            }
            if (error[@"stack"]) {
                [reason appendFormat:@"\nJS Stack:\n%@", error[@"stack"]];
            }
            @throw [NSException exceptionWithName:@"PPManticoreNativeInterfaceException"
                                           reason:reason
                                         userInfo:errorDictionary ? @{@"PPManticoreJSError":errorDictionary} : nil];
        };
    }
    return self;
}

-(void)log:(NSString *)level message:(NSString *)message {
    NSLog(@"%@: %@", level, message);
}

-(JSValue*)http:(JSValue *)options callback:(JSValue *)callback {
    NSURL *url = [NSURL URLWithString:options[@"url"].toString];
    JSValueProtect(self.engine.jsEngine.JSGlobalContextRef, callback.JSValueRef);
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:url];
    if (options[@"headers"].isObject) {
        NSDictionary *values = [options[@"headers"] toObjectOfClass:[NSDictionary class]];
        for (NSString *header in values) {
            [request setValue:[values objectForKey:header] forHTTPHeaderField:header];
        }
    }
    if (options[@"method"].isString) {
        request.HTTPMethod = options[@"method"].toString;
    }
    if (options[@"body"].isString) {
        if (options[@"base64Body"].isBoolean && options[@"base64Body"].toBool) {
            request.HTTPBody = [[NSData alloc] initWithBase64EncodedString:options[@"body"].toString options:0];
        } else {
            request.HTTPBody = [options[@"body"].toString dataUsingEncoding:NSUTF8StringEncoding];
        }
    }
    
    __block NSString *format = options[@"format"].toString ?: @"json";

    [NSURLConnection sendAsynchronousRequest:request queue:[NSOperationQueue mainQueue] completionHandler:^(NSURLResponse *rawResponse, NSData *data, NSError *connectionError) {
        id bodyResponse = nil;
        if (data.length) {
            if ([format caseInsensitiveCompare:@"json"] == NSOrderedSame) {
                bodyResponse = [NSJSONSerialization JSONObjectWithData:data options:0 error:nil];
            } else if ([format caseInsensitiveCompare:@"binary"] != NSOrderedSame) {
                bodyResponse = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
            } else {
                bodyResponse = [data base64EncodedStringWithOptions:0];
            }
        }

        NSHTTPURLResponse *response = (NSHTTPURLResponse*) rawResponse;
        NSInteger code = response.statusCode;
        if (code == 0 && connectionError && connectionError.code == NSURLErrorUserCancelledAuthentication) {
            // Oh Apple you silly silly networkers. Why put your error in for the real one?
            code = 401;
            connectionError = nil;
        }
        
        NSDictionary *responseInfo = @{
                                       @"headers": response.allHeaderFields ?: [NSNull null],
                                       @"statusCode": @(code),
                                       @"body": bodyResponse ?: [NSNull null]
                                       };
        [callback callWithArguments:@[connectionError?:[NSNull null], responseInfo?:[NSNull null]]];
        JSValueUnprotect(self.engine.jsEngine.JSGlobalContextRef, callback.JSValueRef);
    }];
    return nil;
}

-(void)setTimeout:(void (^)())function after:(NSInteger)milliseconds {
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(milliseconds * NSEC_PER_SEC / 1000)), dispatch_get_main_queue(), ^{
        function();
    });
}


@end
