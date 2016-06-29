//
//  ViewController.m
//  Hello Manticore
//
//  Created by Schneider, Griffin on 12/4/15.
//  Copyright © 2015 Schneider, Griffin. All rights reserved.
//

#import "ViewController.h"
#import <ManticoreGenerated/PPManticoreHello.h>


@interface ViewController()
@property (nonatomic, strong) PPManticoreHello *hello;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation ViewController


- (void)loadView {
    [super loadView];
    
    self.hello = [[PPManticoreHello alloc] initWithStringProperty:@"Hello World!" intProperty:42];
    [self updateUI];
    
}

- (void)updateUI {
    
    self.textLabel.text = self.hello.stringProperty;
    [self.button setTitle:[NSString stringWithFormat:@"%d", self.hello.intProperty] forState:UIControlStateNormal];
}


- (IBAction)buttonPressed:(id)sender {
    self.hello.intProperty++;
    [self updateUI];
}

@end
