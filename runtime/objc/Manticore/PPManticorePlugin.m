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

#import "PPManticorePlugin.h"
#import "PPManticoreEngine+Private.h"

@interface PPManticorePlugin ()
// Convention is not to retain the delegate, but it would be very strange here IMHO, because it would force you
// to keep both the plugin object and the delegate separately, which seems annoying.
@property (nonatomic,strong) id<PPManticorePluginDelegate> delegate;
@end

@implementation PPManticorePlugin
-(instancetype)initWithDelegate:(id<PPManticorePluginDelegate>)delegate forEngine:(PPManticoreEngine *)engine {
    if ((self = [super init])) {
        self.delegate = delegate;
        if ([delegate respondsToSelector:@selector(didLoadPolyfill:)]) {
            [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(willLoadPolyfill:) name:WILL_LOAD_POLYFILL_NOTIFICATION object:engine];
        }
        if ([delegate respondsToSelector:@selector(willLoadPolyfill:)]) {
            [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(didLoadPolyfill:) name:DID_LOAD_POLYFILL_NOTIFICATION object:engine];
        }
        if ([delegate respondsToSelector:@selector(engine:willLoadScript:withName:)]) {
            [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(willLoadScript:) name:WILL_LOAD_SCRIPT_NOTIFICATION object:engine];
        }
        if ([delegate respondsToSelector:@selector(engine:didLoadScript:withName:)]) {
            [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(didLoadScript:) name:DID_LOAD_SCRIPT_NOTIFICATION object:engine];
        }
    }
    return self;
}

-(void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

-(void)willLoadPolyfill:(NSNotification *)notification {
    [self.delegate willLoadPolyfill:notification.object];
}

-(void)didLoadPolyfill:(NSNotification *)notification {
    [self.delegate didLoadPolyfill:notification.object];
}

-(void)willLoadScript:(NSNotification *)notification {
    [self.delegate engine:notification.object
           willLoadScript:notification.userInfo[SCRIPT_KEY]
                 withName:notification.userInfo[SCRIPT_NAME_KEY]];
}

-(void)didLoadScript:(NSNotification *)notification {
    [self.delegate engine:notification.object
            didLoadScript:notification.userInfo[SCRIPT_KEY]
                 withName:notification.userInfo[SCRIPT_NAME_KEY]];
}

@end
