using System;
using UnityEngine;

namespace Tools.Config
{
    [Serializable]
    public class ApplicationConfigs : ScriptableObject
    {
        public VersionConfig versionConfig;
        public FirebaseConfig firebaseConfig;
    }

    [Serializable]
    public class VersionConfig
    {
        public string version = "2.0.0";
    }

    [Serializable]
    public class FirebaseConfig
    {
        public string clientId = "sample.apps.googleusercontent.com";
    }
}