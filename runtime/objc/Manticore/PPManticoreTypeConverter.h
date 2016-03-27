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

@class PPManticoreEngine;

@interface PPManticoreTypeConverter : NSObject

-(id)initWithEngine:(PPManticoreEngine*)engine;

@property (nonatomic,assign) Class errorClass;

-(int)toNativeInt:(JSValue *)value;
-(JSValue*)toJsInt:(int)integerValue;

-(BOOL)toNativeBool:(JSValue *)value;
-(JSValue*)toJsBool:(BOOL)boolValue;

-(NSDecimalNumber *)toNativeDecimal:(JSValue *)value;
-(JSValue *)toJsDecimal:(NSDecimalNumber *)decimalValue;

-(NSDictionary *)toNativeObject:(JSValue *)value;
-(JSValue *)toJsObject:(NSDictionary *)value;

-(NSError*)toNativeError:(JSValue *)error;

-(NSArray *)toNativeArray:(JSValue *)value withConverter:(id (^)(id))converter;
-(JSValue *)toJsArray:(NSArray *)array withConverter:(id (^)(id))converter;
@end
