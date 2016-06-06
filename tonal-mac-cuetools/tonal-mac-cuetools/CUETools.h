//
//  CUETools.h
//  Tonal
//
//  Created by hjr on 6/8/16.
//  Copyright © 2016 icmd. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef enum {
    CUEToolsStepContactDB = 0,
    CUEToolsStepVerify,
    CUEToolsStepRepairAnalyze,
    CUEToolsStepWrite
} CUEToolsStep;

typedef void (^OnProgress)(CUEToolsStep step, double percent);
typedef void (^OnEnd)(bool exist, int confidence, int offset, NSString *binPath);
typedef void (^OnError)(NSString *error);

@interface CUETools : NSObject

+ (void)repair:(NSString *)cuePath
    onProgress:(OnProgress)onProgress
         onEnd:(OnEnd)onEnd
       onError:(OnError)onError;

@end
