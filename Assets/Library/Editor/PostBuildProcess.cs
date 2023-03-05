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
            
            // ================================================================
            
            var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            //UNITY 2022.2.X FIX https://forum.unity.com/threads/xcode-build-error-after-upgrading-to-2022-2-0.1371966/
            var projRaw = File.ReadAllText(projectPath);
            projRaw = projRaw.Replace("chmod +x \\\"$IL2CPP\\\"",
                "chmod -R +x *\\nchmod +x \\\"$IL2CPP\\\"");
            projRaw = Regex.Replace(projRaw, "--data-folder=\\\\\"([^\"]*)\\\\\"",
                "--data-folder=\\\"$PROJECT_DIR/Data/Managed\\\"");
            File.WriteAllText(projectPath, projRaw);
            
            // ================================================================
            
            var projPath = PBXProject.GetPBXProjectPath(path);
            var proj = new PBXProject();
            
            proj.ReadFromString(File.ReadAllText(projPath));
            var target2 = proj.TargetGuidByName("Unity-iPhone");

            // システムのフレームワークを追加
            proj.AddFrameworkToProject(target2, "AssetsLibrary.framework", false);
            
            // // 自前のフレームワークを追加
            // CopyAndReplaceDirectory("Assets/Lib/mylib.framework", Path.Combine(path, "Frameworks/mylib.framework"));
            // proj.AddFileToBuild(target2, proj.AddFile("Frameworks/mylib.framework", "Frameworks/mylib.framework", PBXSourceTree.Source));
            //
            // // ファイルを追加
            // var fileName = "my_file.xml";
            // var filePath = Path.Combine("Assets/Lib", fileName);
            // File.Copy(filePath, Path.Combine(path, fileName));
            // proj.AddFileToBuild(target2, proj.AddFile(fileName, fileName, PBXSourceTree.Source));
            //
            // // Yosemiteでipaが書き出せないエラーに対応するための設定
            // proj.SetBuildProperty(target2, "CODE_SIGN_RESOURCE_RULES_PATH", "$(SDKROOT)/ResourceRules.plist");

            // フレームワークの検索パスを設定・追加
            proj.SetBuildProperty(target2, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            proj.AddBuildProperty(target2, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/GoogleSDK");
            
            // 書き出し
            File.WriteAllText(projPath, proj.WriteToString());
        }
        
        // ちょっとしたユーティリティ関数（http://goo.gl/fzYig8を参考）
        private static void CopyAndReplaceDirectory(string srcPath, string dstPath)
        {
            if (Directory.Exists(dstPath))
                Directory.Delete(dstPath);
            if (File.Exists(dstPath))
                File.Delete(dstPath);

            Directory.CreateDirectory(dstPath);

            foreach (var file in Directory.GetFiles(srcPath))
                File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

            foreach (var dir in Directory.GetDirectories(srcPath))
                CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
        }
    }
}
#endif