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
        /// Visiblity to DevTools.
        /// </summary>
        public class DevTools : System.Attribute {

            private Visibility kindVisiblity;
            private Available kindTime;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public DevTools(Visibility initVisiblity, Available initTime) {
                kindVisiblity = initVisiblity;
                kindTime = initTime;
            }
            public DevTools(Visibility initVisibility) {
                kindVisiblity = initVisibility;
                kindTime = Available.Always;
            }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            /// <summary>
            /// Value of Visiblity.
            /// </summary>
            public Visibility Visibility { get { return kindVisiblity; }}
            /// <summary>
            /// The specified time a method can be executed.
            /// </summary>
            public Available Time { get { return kindTime; }}
        };
    }
}