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

#import "PPManticoreTypeConverter.h"
#import "PPManticoreEngine+Private.h"
#import "PPManticoreError.h"

@interface PPManticoreTypeConverter()
@property (nonatomic,strong) PPManticoreEngine *engine;
@end

@implementation PPManticoreTypeConverter
-(id)initWithEngine:(PPManticoreEngine *)engine {
    if ((self = [super init])) {
        self.engine = engine;
        self.errorClass = [PPManticoreError class];
    }
    return self;
}
-(JSValue *)toJsInt:(int)integerValue {
    return [JSValue valueWithInt32:integerValue inContext:self.engine.jsEngine];
}

-(int)toNativeInt:(JSValue *)value {
    return value.toInt32;
}

-(JSValue *)toJsBool:(BOOL)boolValue {
    return [JSValue valueWithBool:boolValue inContext:self.engine.jsEngine];
}

-(BOOL)toNativeBool:(JSValue *)value {
    return value.toBool;
}

-(JSValue *)toJsDecimal:(NSDecimalNumber *)decimalValue {
    return [JSValue valueWithObject:decimalValue.stringValue inContext:self.engine.jsEngine];
}

-(NSDecimalNumber *)toNativeDecimal:(JSValue *)value {
    if (value.isNull || value.isUndefined) {
        return nil;
    }
    return [NSDecimalNumber decimalNumberWithString:[value toString]];
}

-(JSValue *)toJsArray:(NSArray *)array withConverter:(id (^)(id))converter {
    NSMutableArray *translatedArray = [NSMutableArray new];
    NSUInteger length = array.count;
    for (int i = 0; i < length; i++) {
        [translatedArray addObject:converter(array[i])];
    }
    return [JSValue valueWithObject:translatedArray inContext:self.engine.jsEngine];
}

-(NSArray *)toNativeArray:(JSValue *)value withConverter:(id (^)(id))converter {
    if (value.isNull || value.isUndefined) {
        return nil;
    }
    NSMutableArray *translatedArray = [NSMutableArray new];
    NSUInteger length = [value[@"length"] toUInt32];
    for (int i = 0; i < length; i++) {
        [translatedArray addObject:converter(value[i])];
    }
    return translatedArray;
}

-(NSDictionary *)toNativeObject:(JSValue *)value {
    if (value.isNull || value.isUndefined) {
        return nil;
    }
    NSMutableDictionary *dict = [NSMutableDictionary new];
    NSDictionary *source = value.toDictionary;
    for (NSString *key in source.allKeys) {
        JSValue *val = value[key];
        if (val.isBoolean) {
            dict[key] = [NSNumber numberWithBool:val.toBool];
        } else if (val.isNull) {
            dict[key] = [NSNull null];
        } else if (val.isNumber) {
            dict[key] = val.toNumber;
        } else if (val.isString) {
            dict[key] = val.toString;
        } else {
            // TODO decide what the right thing to do here is...
            // Maybe try and instantiate the native class?
            // But generally don't use objects to send back.
            dict[key] = val.toString;
        }
    }
    return dict;
}

-(JSValue *)toJsObject:(NSDictionary *)value {
    return [JSValue valueWithObject:value inContext:self.engine.jsEngine];
}

-(NSError *)toNativeError:(JSValue *)value {
    if (value.isNull || value.isUndefined) {
        return nil;
    }
    NSDictionary *info = [NSDictionary dictionaryWithObject:value[@"message"].toString forKey:NSLocalizedDescriptionKey];
    return [[self.errorClass alloc] initWithDomain:@"Manticore" code:-1 userInfo:info];
}
@end
