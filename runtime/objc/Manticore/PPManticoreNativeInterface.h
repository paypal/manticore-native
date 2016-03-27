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


/**
 * Generated classes implement this protocol which tells us to pass
 * the JSValue across the boundary to JS instead of the object instance.
 */
@protocol PPManticoreNativeObjectProtocol <NSObject>

@required
/**
 * Return the Javascript implementation
 */
-(JSValue*)impl;
/**
 * The Manticore engine in which this object type resides
 */
+(PPManticoreEngine*)engine;
/**
 * Create a new instance of the current class given a Javascript value of the corresponding JS type
 */
-(id)initFromJavascript:(JSValue*)value;

/**
 * This class method will eventually call initFromJavascript, but it will look at the _native
 * property of the value to determine WHICH native class to create. Take a situation where we
 * have a base class ClassA, from which ClassB derives. If a method or property is defined as
 * returning ClassA, but returns a ClassB object, the native side can't safely and easily know
 * that by just poking at the JSValue. So, the JS class must return a value for the _native
 * property indicating which native class (as specified in the code comments on the JS side)
 * should be made for a given JS instance.
 */
+(Class)nativeClassForObject:(JSValue*)value;

@end


/**
 * The native interface represents the main object exposed to the Javascript
 * layer in order to communicate with the native layer for hardware, ui, etc.
 */
@interface PPManticoreNativeInterface : NSObject <JSExport>

/**
 * The Objective-C manticore JS layer translates native calls such as console.log to
 * "defined" calls on the manticore object.
 */
@property (nonatomic,strong,readonly) JSValue *manticoreObject;

/**
 * Setup relevant native method interfaces on a JS engine
 */
- (instancetype)initWithEngine:(PPManticoreEngine *)engine;

@end
