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

@property (nonatomic,strong,nonnull) PPManticoreTypeConverter *converter;
@property (nonatomic,readonly,nonnull) JSContext *context;
@property (nonatomic,readonly,nonnull) JSGlobalContextRef globalContext;

- (void)loadScript:(NSString* _Nonnull)script withName:(NSString* _Nullable)name;

-(JSValue* _Nullable)createJSObject:(NSString* _Nonnull)jsClassName withArguments:(NSArray* _Nullable)args;
-(id _Nullable)attachNativeObject:(JSValue* _Nonnull)value ofType:(Class _Nonnull)nativeType;
-(id _Nullable)resolveJSObject:(id _Nullable)object ofType:(Class _Nonnull)nativeType;
-(JSValue* _Nullable)resolveJSClass:(NSString* _Nonnull)jsClassName;
-(void)resolvePromise:(JSValue* _Nonnull)promise toCallback:(void (^_Nonnull)(JSValue* _Nullable,JSValue* _Nullable))callback;

@end
