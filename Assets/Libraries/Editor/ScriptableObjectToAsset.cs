using System.IO;
using UnityEditor;
using UnityEngine;

// <summary>
// ScriptableObjectをプレハブとして出力する汎用スクリプト  
// </summary>
// <remarks>
// 指定したScriptableObjectをプレハブに変換する。
// 1.Editorフォルダ下にCreateScriptableObjectPrefab.csを配置  
// 2.ScriptableObjectのファイルを選択して右クリック→Create ScriptableObjectを選択  
// </remarks>
namespace Tools
{
    public static class ScriptableObjectToAsset
    {
        private static readonly string[] labels = { "Data", "ScriptableObject", string.Empty };

        [MenuItem("Assets/Create ScriptableObject")]
        private static void Crate()
        {
            foreach (var selectedObject in Selection.objects)
            {
                // get path
                var path = GetSavePath(selectedObject);

                // create instance
                var obj = ScriptableObject.CreateInstance(selectedObject.name);
                AssetDatabase.CreateAsset(obj, path);
                labels[2] = selectedObject.name;
                // add label
                var sobj = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as ScriptableObject;
                AssetDatabase.SetLabels(sobj, labels);
                EditorUtility.SetDirty(sobj);
            }
        }

        private static string GetSavePath(Object selectedObject)
        {
            var objectName = selectedObject.name;
            var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObject));
            var path = $"{dirPath}/{objectName}.asset";

            if (!File.Exists(path)) return path;
            for (var i = 1;; i++)
            {
                path = $"{dirPath}/{objectName}({i}).asset";
                if (!File.Exists(path))
                    break;
            }

            return path;
        }
    }
}