#if UNITY_IOS
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Library.Editor
{
    /// <Summary>
    /// XCodeでBitCodeをOFFにします
    /// </Summary>
    public static class PostBuildProcess
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS) ProcessForXCode(path);
        }

        private static void ProcessForXCode(string path)
        {
            var pjPath = PBXProject.GetPBXProjectPath(path);
            var pj = new PBXProject();
            pj.ReadFromString(File.ReadAllText(pjPath));
            var target = pj.GetUnityFrameworkTargetGuid();

            pj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            File.WriteAllText(pjPath, pj.WriteToString());
            
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
#endif