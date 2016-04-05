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

@class PPManticoreEngine;

/**
 * Exposing these elements of the loading cycle allows you to inject your own code
 * at appropriate points. Typically this is used to expose new native functionality
 * to the javascript. Note that if you load script, your own will/didLoadScript event
 * will get called, so make sure to not infinitely do so.
 */
@protocol PPManticorePluginDelegate <NSObject>
@optional
-(void)willLoadPolyfill:(PPManticoreEngine* _Nonnull) engine;
-(void)didLoadPolyfill:(PPManticoreEngine* _Nonnull) engine;

-(void)engine:(PPManticoreEngine* _Nonnull) engine willLoadScript:(NSString* _Nonnull) script withName:(NSString* _Nullable)name;
-(void)engine:(PPManticoreEngine* _Nonnull) engine didLoadScript:(NSString* _Nonnull) script withName:(NSString* _Nullable)name;
@end

/**
 * This class basically exists to give you a pleasant delegate interface to a 
 * multicast event model (from NSNotificationCenter)
 */
@interface PPManticorePlugin : NSObject
-(instancetype _Nonnull)initWithDelegate:(id<PPManticorePluginDelegate> _Nonnull)delegate forEngine:(PPManticoreEngine* _Nullable)engine;
@end
