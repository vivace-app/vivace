using System;
using UnityEngine;

namespace Tools.Config
{
    public class ConfigComponent : SingletonMonoBehaviour<ConfigComponent>
    {
        private const string BasePath = "Config/";

        [SerializeField] private ConfigEnvironment targetEnv = ConfigEnvironment.Development;
        private ApplicationConfigs _config;

        private new void Awake() => DontDestroyOnLoad(gameObject);

        public ApplicationConfigs Config => _config ??= LoadConfig();

        private ApplicationConfigs LoadConfig()
        {
            return targetEnv switch
            {
                ConfigEnvironment.Development => Resources.Load<ApplicationConfigs>(BasePath + "Development"),
                ConfigEnvironment.Production => Resources.Load<ApplicationConfigs>(BasePath + "Production"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}