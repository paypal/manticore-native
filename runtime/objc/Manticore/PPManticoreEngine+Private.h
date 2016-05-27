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

#import <JavaScriptCore/JavaScriptCore.h>
#import "PPManticoreEngine.h"
#import "PPManticoreNativeInterface.h"



static NSString *WILL_LOAD_POLYFILL_NOTIFICATION = @"Manticore.WillLoadPolyfill";
static NSString *DID_LOAD_POLYFILL_NOTIFICATION = @"Manticore.DidLoadPolyfill";
static NSString *WILL_LOAD_SCRIPT_NOTIFICATION = @"Manticore.WillLoadScript";
static NSString *DID_LOAD_SCRIPT_NOTIFICATION = @"Manticore.DidLoadScript";

#define SCRIPT_NAME_KEY @"ScriptName"
#define SCRIPT_KEY      @"Script"

@interface PPManticoreEngine (Private)

@property (nonatomic,strong) JSContext *jsEngine;
@property (nonatomic,strong) JSValue *exports;
@property (nonatomic,strong) PPManticoreNativeInterface *native;

@end
