using UnityEngine;

namespace Tools.Config
{
    public static class ConfigProvider
    {
        private const string Path = "ConfigProvider";

        private static ConfigComponent _configComponent;

        private static ConfigComponent ConfigComponent
        {
            get
            {
                if (_configComponent != null) return _configComponent;
                if (ConfigComponent.Instance == null)
                {
                    var resource = Resources.Load(Path);
                    Object.Instantiate(resource);
                }
                _configComponent = ConfigComponent.Instance;
                return _configComponent;
            }
        }

        public static VersionConfig Version => ConfigComponent.Config.versionConfig;
        
        public static FirebaseConfig Firebase => ConfigComponent.Config.firebaseConfig;
    }
}