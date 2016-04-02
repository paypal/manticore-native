@class PPManticoreSDKTest;
@class PPManticoreSDKTestDefault;
@class PPManticoreSDKTestDefaultSubclass;
@class PPManticoreSDKTestDefault;
@class PPManticoreError;


/**
 * Valid SDK statuses
 */
typedef NS_ENUM(NSInteger, PPManticoreSDKTestStatuses) {
  PPManticoreSDKTestStatusesON_FIRE = 0,
  PPManticoreSDKTestStatusesWET = 1,
  PPManticoreSDKTestStatusesUNINMAGINABLE = 2
};


/**
 * Callback for fetch method
 */
typedef void (^PPManticoreSDKTestFetchedHandler)(PPManticoreError* error, NSDictionary* response);

/**
 * Callback for echo method
 */
typedef void (^PPManticoreSDKTestEchoHandler)(PPManticoreError* error, NSString* arg);

/**
 * Callback for echo method with return
 */
typedef NSString* (^PPManticoreSDKTestEchoReturnHandler)();

/**
 * Callback for PPManticore goFetchP method
 */
typedef void (^PPManticoreGoFetchPCallback)(PPManticoreError* error, NSDictionary* result);
 
/**
 * Simple event
 */
typedef void (^PPManticoreFakeEventEvent)(PPManticoreSDKTestDefault* item);

/**
 * Returned from addFakeEventListener and used to unsubscribe from the event.
 */
typedef id PPManticoreFakeEventSignal;

     
