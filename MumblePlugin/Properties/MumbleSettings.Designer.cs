﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Plugin.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class MumbleSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static MumbleSettings defaultInstance = ((MumbleSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new MumbleSettings())));
        
        public static MumbleSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("PPS Chat (Registered users may create channels in here)")]
        public string MumbleChannel {
            get {
                return ((string)(this["MumbleChannel"]));
            }
            set {
                this["MumbleChannel"] = value;
            }
        }
    }
}
