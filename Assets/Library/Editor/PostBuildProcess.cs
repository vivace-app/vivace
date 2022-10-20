using System.IO;
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
        }
    }
}