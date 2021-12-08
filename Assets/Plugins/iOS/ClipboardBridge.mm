#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "ClipboardBridge.h"
#import "UnityAppController.h"

@implementation ClipboardBridge

/**
 *  单例
 *  @return 实例对象
 */
+(instancetype)sharedBridge{
    
    static dispatch_once_t onceToken;
    static ClipboardBridge *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc] init];
    });
    return instance;
}

-(id)init{
    if (self = [super init]){
        return self;
    }
    return nil;
}

-(void)copyTextToClipBoard:(NSString*)txt{
    
    NSLog(@"ClipBoardBridge Copy Text:%@",txt);
    UIPasteboard *past=[UIPasteboard generalPasteboard];
    past.string=txt;
    
}

-(void)dealloc{
    
}

@end
 
extern "C" {
    
    void UnityCopyTextToClipboard(const char *textList)
    {
        NSString *text = [NSString stringWithUTF8String: textList] ;
        [[ClipboardBridge sharedBridge]copyTextToClipBoard:text];
    }
    
}
