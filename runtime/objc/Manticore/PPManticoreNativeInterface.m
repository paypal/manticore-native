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
        _manticoreObject[@"_log"] = ^(NSString* level,  NSString* message) {
            [weakSelf log:level message:message];
        };
        
        _manticoreObject[@"_fetch"] = ^(JSValue *request, JSValue *callback) {
            return [weakSelf fetch:request callback:callback];
        };
        
        _manticoreObject[@"_setTimeout"] = ^(JSValue *callback, JSValue *timeout) {
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

-(JSValue*)fetch:(JSValue *)request callback:(JSValue *)callback {
    NSURL *url = [NSURL URLWithString:request[@"url"].toString];
    JSValueProtect(self.engine.jsEngine.JSGlobalContextRef, callback.JSValueRef);
    
    NSMutableURLRequest *urlRequest = [NSMutableURLRequest requestWithURL:url];
    if (request[@"headers"].isObject) {
        JSValue *headersRaw = [request[@"headers"] invokeMethod:@"raw" withArguments:nil];
        NSDictionary *values = [headersRaw toObjectOfClass:[NSDictionary class]];
        for (NSString *header in values) {
            id headerVal = values[header];
            if ([headerVal isKindOfClass:[NSArray class]]) {
                for (id arrVal in ((NSArray*)headerVal)) {
                    [urlRequest addValue:[arrVal stringValue] forHTTPHeaderField:header];
                }
            } else {
                [urlRequest setValue:[[values objectForKey:header] stringValue] forHTTPHeaderField:header];
            }
        }
    }
    if (request[@"method"].isString) {
        urlRequest.HTTPMethod = request[@"method"].toString;
    }
    
    if (request[@"body"].isString) {
        if (request[@"isBase64"].isBoolean && request[@"isBase64"].toBool) {
            urlRequest.HTTPBody = [[NSData alloc] initWithBase64EncodedString:request[@"body"].toString options:0];
        } else {
            urlRequest.HTTPBody = [request[@"body"].toString dataUsingEncoding:NSUTF8StringEncoding];
        }
    }
    
    [NSURLConnection sendAsynchronousRequest:urlRequest queue:[NSOperationQueue mainQueue] completionHandler:^(NSURLResponse *rawResponse, NSData *data, NSError *connectionError) {

        NSHTTPURLResponse *response = (NSHTTPURLResponse*) rawResponse;
        NSInteger code = response.statusCode;
        if (code == 0 && connectionError && connectionError.code == NSURLErrorUserCancelledAuthentication) {
            // Oh Apple you silly silly networkers. Why put your error in for the real one?
            code = 401;
            connectionError = nil;
        }

        // Now construct a Response-like object that lets us do the fetching
        JSValue *jsResponse = [JSValue valueWithNewObjectInContext:self.engine.jsEngine];
        if (response.allHeaderFields) {
            jsResponse[@"headers"] = response.allHeaderFields;
        }
        jsResponse[@"status"] = @(code);
        jsResponse[@"json"] = ^() {
            return [JSValue valueWithObject:[NSJSONSerialization JSONObjectWithData:data options:0 error:nil] inContext:self.engine.context];
        };
        jsResponse[@"text"] = ^() {
            return [JSValue valueWithObject:[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] inContext:self.engine.context];
        };
        jsResponse[@"body"] = ^() {
            return [JSValue valueWithObject:[data base64EncodedDataWithOptions:0] inContext:self.engine.context];
        };

        JSValue *jsError = nil;
        if (connectionError) {
            NSString *msg = [connectionError localizedDescription] ?: connectionError.domain;
            jsError = [JSValue valueWithNewErrorFromMessage:msg inContext:self.engine.context];
            jsError[@"domain"] = connectionError.domain;
            jsError[@"code"] = @(connectionError.code);
        }
        
        [callback callWithArguments:@[jsError?:[NSNull null], jsResponse]];
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
