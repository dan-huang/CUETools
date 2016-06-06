//
//  CUETools.m
//  Tonal
//
//  Created by hjr on 6/8/16.
//  Copyright © 2016 icmd. All rights reserved.
//

#import "CUETools.h"
#import <mono/jit/jit.h>
#import <mono/metadata/assembly.h>
#import <mono/metadata/debug-helpers.h>

@interface CUETools() {

}

@property (copy) OnProgress onProgress;
@property (copy) OnEnd onEnd;
@property (copy) OnError onError;

@end

@implementation CUETools

void on_progress(void *context, CUEToolsStep step, double percent) {
    CUETools *cuetools = (__bridge CUETools *)context;
    cuetools.onProgress(step, percent);
}

void on_end(void *context,
            bool exist,
            int confidence,
            int offset,
            char *binPath) {
    CUETools *cuetools = (__bridge CUETools *)context;
    cuetools.onEnd(exist, confidence, offset,
                   binPath ? [NSString stringWithUTF8String:binPath] : nil);
}

void on_error(void *context, char *error) {
    CUETools *cuetools = (__bridge CUETools *)context;
    cuetools.onError(error ? [NSString stringWithUTF8String:error] : nil);
}

+ (void)repair:(NSString *)cuePath
    onProgress:(OnProgress)onProgress
         onEnd:(OnEnd)onEnd
       onError:(OnError)onError {

    int paramCount = 5;
    MonoMethod *repair = mono_class_get_method_from_name([self sharedMonoCUETools], "Repair", paramCount);
    MonoObject *cuetools_object = mono_object_new(mono_domain_get(), [self sharedMonoCUETools]);
    void *params[paramCount];
    params[0] = mono_string_new(mono_domain_get(), cuePath.UTF8String);

    void (*onProgress_)(void *, CUEToolsStep, double);
    void (*onEnd_)(void *, bool, int, int, char *);
    void (*onError_)(void *, char *);

    onProgress_ = &on_progress;
    onEnd_ = &on_end;
    onError_ = &on_error;
    params[1] = &onProgress_;
    params[2] = &onEnd_;
    params[3] = &onError_;

    CUETools *cuetools = [[CUETools alloc] init];
    cuetools.onProgress = onProgress;
    cuetools.onEnd = onEnd;
    cuetools.onError = onError;
    params[4] = &cuetools;

    mono_runtime_invoke (repair, cuetools_object, params, NULL);
}

+ (MonoClass *)sharedMonoCUETools
{
    static MonoClass *sharedCUETools = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        MonoDomain *domain = mono_jit_init ("Tonal");
        mono_domain_set_config(domain, [[NSBundle bundleForClass:[self class]] bundlePath].UTF8String, "framework.config");
        NSString* dll = [[NSBundle bundleForClass:[self class]] pathForResource:@"Tonal" ofType:@"dll"];
        mono_set_assemblies_path([[NSBundle bundleForClass:[self class]] resourcePath].UTF8String);
        MonoAssembly* assembly = mono_domain_assembly_open(domain, [dll UTF8String]);
        MonoImage* image = mono_assembly_get_image(assembly);
        sharedCUETools = mono_class_from_name(image, "Tonal", "CUETools");
    });
    return sharedCUETools;
}

@end
