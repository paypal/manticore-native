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

#import <Foundation/Foundation.h>
#import <JavaScriptCore/JavaScriptCore.h>

#import "PPManticoreTypeConverter.h"
#import "PPManticoreEventHolder.h"

@interface PPManticoreEngine : NSObject

@property (nonatomic,strong) PPManticoreTypeConverter *converter;
@property (nonatomic,readonly) JSContext *context;
@property (nonatomic,readonly) JSGlobalContextRef globalContext;

- (void)loadScript:(NSString *)script withName:(NSString *)name;

-(JSValue*)createJSObject:(NSString*)jsClassName withArguments:(NSArray*)args;
-(id)attachNativeObject:(JSValue*)value ofType:(Class)nativeType;
-(id)resolveJSObject:(id)object ofType:(Class)nativeType;
-(JSValue*)resolveJSClass:(NSString*)jsClassName;

@end
