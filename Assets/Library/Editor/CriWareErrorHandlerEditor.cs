/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CriWareErrorHandler))]
public class CriWareErrorHandlerEditor : UnityEditor.Editor {
	private SerializedProperty m_enableDebugPrintOnTerminal;
	private SerializedProperty m_enableForceCrashOnError;
	private SerializedProperty m_dontDestroyOnLoad;
	private SerializedProperty m_messageBufferCounts;

	private void OnEnable() {
		m_enableDebugPrintOnTerminal = serializedObject.FindProperty("enableDebugPrintOnTerminal");
		m_enableForceCrashOnError = serializedObject.FindProperty("enableForceCrashOnError");
		m_dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
		m_messageBufferCounts = serializedObject.FindProperty("messageBufferCounts");
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.PropertyField(m_enableDebugPrintOnTerminal, new GUIContent("Print Debug on Terminal"));
		EditorGUILayout.PropertyField(m_enableForceCrashOnError, new GUIContent("Force Crash on Error"));
		EditorGUILayout.HelpBox("Force Crash on Error:\nAny CRIWARE error will cause the editor to crash.\nDon't enable this when debugging in the editor.", m_enableForceCrashOnError.boolValue ? MessageType.Warning : MessageType.Info);
		EditorGUILayout.PropertyField(m_dontDestroyOnLoad, new GUIContent("Dont Destroy on Load"));
		m_messageBufferCounts.intValue = GenIntField("Error list length:", "", m_messageBufferCounts.intValue, 0, 32);

		serializedObject.ApplyModifiedProperties();
	}

	private int GenIntField(string label_str, string tooltip, int field_value, int min, int max)
	{
		GUIContent content = new GUIContent(label_str, tooltip);
		return Mathf.Min(max, Mathf.Max(min, EditorGUILayout.IntField(content, field_value)));
	}
}

/* end of file */