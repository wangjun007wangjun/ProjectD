
#ifndef ClipboardBridge_h
#define ClipboardBridge_h

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>

@interface ClipboardBridge:NSObject

/**
 *  单例
 *  @return 实例对象
 */
+(instancetype)sharedBridge;


-(void)copyTextToClipBoard:(NSString*)txt;

@end


#endif /* BatteryBridge_h */

