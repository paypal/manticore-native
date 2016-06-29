//
//  AppDelegate.m
//  Hello Manticore
//
//  Created by Schneider, Griffin on 12/4/15.
//  Copyright © 2015 Schneider, Griffin. All rights reserved.
//

#import "AppDelegate.h"
#import <ManticoreNative/PPManticoreEngine.h>
#import "PPManticoreJSBackedObject.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation AppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [PPManticoreJSBackedObject setManticoreEngine:[PPManticoreEngine new]];
    NSString *path = [[NSBundle mainBundle] pathForResource:@"manticore_modules" ofType:@"js"];
    [[PPManticoreJSBackedObject engine] loadScript:[NSString stringWithContentsOfFile:path encoding:NSUTF8StringEncoding error:NULL] withName:nil];
    return YES;
}

@end
