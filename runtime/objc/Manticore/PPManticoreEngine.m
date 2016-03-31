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

#import "PPManticoreEngine+Private.h"

@interface PPManticoreEngine ()

@property (nonatomic,strong) JSContext *jsEngine;
@property (nonatomic,strong) JSValue *exports;
@property (nonatomic,strong) PPManticoreNativeInterface *native;
@property (nonatomic,assign) BOOL loadedPolyfill;
@end

@implementation PPManticoreEngine

- (instancetype)init {
    if ((self = [super init])) {
        self.jsEngine = [JSContext new];
        // Re-export the global context as a named variable so that modules can get at it
        // We use the name "global" to mirror node.js
        self.jsEngine[@"global"] = [self.jsEngine globalObject];
        self.jsEngine[@"exports"] = self.exports = [JSValue valueWithNewObjectInContext:self.jsEngine];
        self.native = [[PPManticoreNativeInterface alloc] initWithEngine:self];
        self.converter = [[PPManticoreTypeConverter alloc] initWithEngine:self];
    }
    return self;
}

- (void)loadScript:(NSString *)script withName:(NSString *)name {
    if (!self.loadedPolyfill) {
        self.loadedPolyfill = YES;
        NSURL *bundleUrl = [[NSBundle mainBundle] URLForResource:@"PayPalManticoreResources" withExtension:@"bundle"];
        NSBundle *bundle = [NSBundle bundleWithURL:bundleUrl];
        NSString *jsPath = [bundle pathForResource:@"polyfill.pack" ofType:@"js"];
        NSString *js = [NSString stringWithContentsOfFile:jsPath encoding:NSUTF8StringEncoding error:nil];
        [self loadScript:js withName:@"manticore://polyfill.pack.js"];
    }
    if ([self.jsEngine respondsToSelector:@selector(evaluateScript:withSourceURL:)]) {
        [self.jsEngine evaluateScript:script withSourceURL:[NSURL URLWithString:name]];
    } else {
        [self.jsEngine evaluateScript:script];
    }
}

-(JSValue*)createJSObject:(NSString*)jsClassName withArguments:(NSArray*)args {
    return [[self resolveJSClass:jsClassName] constructWithArguments:args];
}

-(id)attachNativeObject:(JSValue *)value ofType:(Class)nativeType {
    if (value.isNull || value.isUndefined) {
        return nil;
    }

    if ([nativeType conformsToProtocol:@protocol(PPManticoreNativeObjectProtocol)]) {
        Class<PPManticoreNativeObjectProtocol> ppClass = nativeType;
        nativeType = [ppClass nativeClassForObject:value];
    }

    id newInstance = [nativeType alloc];
    if ([newInstance conformsToProtocol:@protocol(PPManticoreNativeObjectProtocol)]) {
        newInstance = [newInstance initFromJavascript:value];
    } else {
        if (nativeType == [NSString class]) {
            return value.toString;
        } else if (nativeType == [NSDate class]) {
            return value.toDate;
        }
        NSAssert(NO, @"Type conversion not yet implemented!");
    }
    return newInstance;
}

-(id)resolveJSObject:(id)object ofType:(Class)nativeType {
    if ([object conformsToProtocol:@protocol(PPManticoreNativeObjectProtocol)]) {
        return [object impl];
    } else {
        return object;
    }
}

-(JSValue*)resolveJSClass:(NSString*)jsClassName {
    JSValue *jsvalue = self.exports[jsClassName];
    NSAssert(!jsvalue.isUndefined, @"You need to add %@ to global.exports", jsClassName);
    return jsvalue;
}

-(void)resolvePromise:(JSValue *)promise toCallback:(void (^)(JSValue *error, JSValue *result))callback {
    [self.jsEngine[@"manticore"] invokeMethod:@"asCallback" withArguments:@[promise, callback]];
}

-(JSGlobalContextRef)globalContext {
    return self.jsEngine.JSGlobalContextRef;
}

-(JSContext *)context {
    return self.jsEngine;
}
@end
