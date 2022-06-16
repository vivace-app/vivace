using UnityEditor;
using UnityEditor.Callbacks;

namespace Libraries.Editor
{
    public static class SaveKeystore
    {
        private static string keystorePrefsName => PlayerSettings.Android.keystoreName;

        private static string keyaliasPrefsName =>
            $"{PlayerSettings.Android.keystoreName}/{PlayerSettings.Android.keyaliasName}";

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) &&
                !string.IsNullOrEmpty(PlayerSettings.Android.keystoreName))
                PlayerSettings.Android.keystorePass = EditorPrefs.GetString(keystorePrefsName);

            if (string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
                PlayerSettings.Android.keyaliasPass = EditorPrefs.GetString(keyaliasPrefsName);
        }

        [PostProcessBuild]
        private static void OnBuilt(BuildTarget target, string path)
        {
            if (target != BuildTarget.Android) return;
            if (!string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) &&
                !string.IsNullOrEmpty(PlayerSettings.Android.keystorePass))
                EditorPrefs.SetString(keystorePrefsName, PlayerSettings.Android.keystorePass);

            if (!string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName) &&
                !string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
                EditorPrefs.SetString(keyaliasPrefsName, PlayerSettings.Android.keyaliasPass);
        }
    }
}