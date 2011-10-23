﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CUETools.Codecs.FLACCL.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CUETools.Codecs.FLACCL.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use additional CPU threads.
        /// </summary>
        internal static string DescriptionCPUThreads {
            get {
                return ResourceManager.GetString("DescriptionCPUThreads", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Additional preprocessor definitions for OpenCL kernel.
        /// </summary>
        internal static string DescriptionDefines {
            get {
                return ResourceManager.GetString("DescriptionDefines", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use CPU or GPU device.
        /// </summary>
        internal static string DescriptionDeviceType {
            get {
                return ResourceManager.GetString("DescriptionDeviceType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do final encoding stages on GPU (experimental).
        /// </summary>
        internal static string DescriptionDoRice {
            get {
                return ResourceManager.GetString("DescriptionDoRice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use GPU on all stages.
        /// </summary>
        internal static string DescriptionGPUOnly {
            get {
                return ResourceManager.GetString("DescriptionGPUOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GPU thread block size (64, 128, 256).
        /// </summary>
        internal static string DescriptionGroupSize {
            get {
                return ResourceManager.GetString("DescriptionGroupSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Device uses host memory (Don&apos;t use).
        /// </summary>
        internal static string DescriptionMappedMemory {
            get {
                return ResourceManager.GetString("DescriptionMappedMemory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OpenCL platform to use.
        /// </summary>
        internal static string DescriptionPlatform {
            get {
                return ResourceManager.GetString("DescriptionPlatform", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Number of frames processed per one multiprocessor.
        /// </summary>
        internal static string DescriptionTaskSize {
            get {
                return ResourceManager.GetString("DescriptionTaskSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calculate MD5 hash for audio stream.
        /// </summary>
        internal static string DoMD5Description {
            get {
                return ResourceManager.GetString("DoMD5Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Decode each frame and compare with original.
        /// </summary>
        internal static string DoVerifyDescription {
            get {
                return ResourceManager.GetString("DoVerifyDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Samples written differs from the expected sample count.
        /// </summary>
        internal static string ExceptionSampleCount {
            get {
                return ResourceManager.GetString("ExceptionSampleCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation failed.
        /// </summary>
        internal static string ExceptionValidationFailed {
            get {
                return ResourceManager.GetString("ExceptionValidationFailed", resourceCulture);
            }
        }
    }
}
