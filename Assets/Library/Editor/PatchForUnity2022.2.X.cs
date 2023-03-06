using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Library.Editor
{
    public static class PatchForUnity20222X
    {
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS)
                return;
            var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            //UNITY 2022.2.X FIX https://forum.unity.com/threads/xcode-build-error-after-upgrading-to-2022-2-0.1371966/
            var projRaw = File.ReadAllText(projectPath);
            projRaw = projRaw.Replace("chmod +x \\\"$IL2CPP\\\"",
                "chmod -R +x *\\nchmod +x \\\"$IL2CPP\\\"");
            projRaw = Regex.Replace(projRaw, "--data-folder=\\\\\"([^\"]*)\\\\\"",
                "--data-folder=\\\"$PROJECT_DIR/Data/Managed\\\"");
            File.WriteAllText(projectPath, projRaw);
        }
    }
}