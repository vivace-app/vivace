using UnityEditor;
using UnityEngine;

public class LicensePackerEditor : EditorWindow
{
    #region Field

    private static GUIStyle MarginStyle;

    [SerializeField]
    private string[] ignoreDirectoriesStartsWith;

    #endregion Field

    #region Method

    [MenuItem("Custom/LicensePacker")]
    static void Init()
    {
        GetWindow<LicensePackerEditor>("LicensePacker");
    }

    protected void OnGUI()
    {
        if (MarginStyle == null)
        {
            MarginStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                margin   = new RectOffset(5, 5, 5, 5)
            };
        }

        EditorGUILayout.BeginVertical(MarginStyle);

        EditorGUILayout.LabelField("License File Name");

        LicensePacker.LicenseFileName = EditorGUILayout.TextField(LicensePacker.LicenseFileName);

        EditorGUILayout.LabelField("Split");

        LicensePacker.Split = EditorGUILayout.TextArea(LicensePacker.Split);

        if (this.ignoreDirectoriesStartsWith == null)
        {
            this.ignoreDirectoriesStartsWith = LicensePacker.IgnoreDirectoriesStartsWith;
        }

        ScriptableObject   scriptableObject = this;
        SerializedObject   serializedObject = new SerializedObject(scriptableObject);
        SerializedProperty arrayProperty    = serializedObject.FindProperty("ignoreDirectoriesStartsWith");

        EditorGUILayout.PropertyField(arrayProperty);

        serializedObject.ApplyModifiedProperties();

        LicensePacker.IgnoreDirectoriesStartsWith = this.ignoreDirectoriesStartsWith;

        EditorGUILayout.EndVertical();
    }

    #endregion Method
}