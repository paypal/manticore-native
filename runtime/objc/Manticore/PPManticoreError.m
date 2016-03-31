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

#import "PPManticoreError.h"
#import "PPManticoreJSBackedObject.h"

@interface PPManticoreError () <
    PPManticoreNativeObjectProtocol
>
@property (nonatomic,strong) JSValue *impl;
@end

@implementation PPManticoreError
-(id)initFromJavascript:(JSValue *)value {
    if ((self = [super initWithDomain:[value[@"domain"] toString]
                                 code:[value[@"code"] toInt32]
                             userInfo:@{
                                        NSLocalizedDescriptionKey: [value[@"message"] toString]
                                        }])) {
    }
    return self;
}

+(PPManticoreEngine *)engine {
    return [PPManticoreJSBackedObject engine];
}

+(Class)nativeClassForObject:(JSValue *)value {
    return [PPManticoreError class];
}
@end
