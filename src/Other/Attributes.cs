using System;

namespace TestingLib {
    /// <summary>
    /// Attributes for TestingLib.
    /// </summary>
    public class Attributes {
        /// <summary>
        /// Visibility to DevTools.
        /// </summary>
        public enum Visibility {
            /// <summary>
            /// Will be fetched by DevTools.
            /// </summary>
            Whitelist,
            /// <summary>
            /// Will not be fetched by DevTools.
            /// </summary>
            Blacklist,
            /// <summary>
            /// Shows only on the DevTools menu.
            /// </summary>
            MenuOnly,
            /// <summary>
            /// Appears in the config, but not the DevTools menu.
            /// </summary>
            ConfigOnly,
        }
        /// <summary>
        /// Time until can be executed.
        /// </summary>
        public enum Available {
            /// <summary>
            /// Can be executed right away.
            /// </summary>
            Always,
            /// <summary>
            /// Can be executed after player has spawned.
            /// </summary>
            PlayerSpawn,
        }

        /// <summary>
        /// Who can execute this method?
        /// </summary>
        public enum Permission {
            /// <summary>
            /// Can be executed right away.
            /// </summary>
            HostOnly,
            /// <summary>
            /// Can be executed after player has spawned.
            /// </summary>
            AllClients,
        }
        /// <summary>
        /// Visiblity to DevTools.
        /// </summary>
        public class DevTools : Attribute {
            private Visibility _Visiblity;
            private Available _Time;
            private Permission _Permission;
            private bool _DefaultValue;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public DevTools(Visibility initVisibility, Available initTime, Permission initPermission, bool initDefaultValue) {
                Init(initVisibility, initTime, initPermission, initDefaultValue);
            }
            public DevTools(Visibility initVisibility, Available initTime, Permission initPermission) {
                Init(initVisibility, initTime, initPermission, defaultValue: true);
            }
            public DevTools(Visibility initVisibility, Available initTime) {
                Init(initVisibility, initTime, Permission.AllClients, defaultValue: true);
            }
            public DevTools(Visibility initVisibility) {
                Init(initVisibility, Available.Always, Permission.AllClients, defaultValue: true);
            }
            private void Init(Visibility initVisiblity, Available initTime, Permission initPermission, bool defaultValue){
                _Visiblity = initVisiblity;
                _Time = initTime;
                _Permission = initPermission;
                _DefaultValue = defaultValue;
            }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            /// <summary>
            /// Value of Visiblity.
            /// </summary>
            public Visibility Visibility { get { return _Visiblity; }}
            /// <summary>
            /// The specified time a method can be executed.
            /// </summary>
            public Available Time { get { return _Time; }}
            /// <summary>
            /// Who can execute this method?
            /// </summary>
            public Permission Permission { get { return _Permission; }}
            /// <summary>
            /// Default value of the Config entry for this method.
            /// </summary>
            public bool DefaultValue { get { return _DefaultValue; }}
        };
    }
}