using System;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(SwipeInput))]
    public class SwipeInputEditor : UnityEditor.Editor
    {
        SerializedProperty useInputSystemIfBothHandling;
        GUIContent useInputSystemIfBothHandlingLabel = new GUIContent("Use InputSystem If Both Handling");

        SerializedProperty widthReference;
        GUIContent widthReferenceLabel = new GUIContent("Width Reference");

        SerializedProperty validWidth;
        GUIContent validWidthLabel = new GUIContent("Valid Width");

        SerializedProperty timeout;
        GUIContent timeoutLabel = new GUIContent("Timeout");

        SerializedProperty validArea;
        GUIContent validAreaLabel = new GUIContent("Valid Area");

        SerializedProperty OnSwipe;
        SerializedProperty OnSwipeRaw;


        bool _callbacksFoldOut = true;
        string CallbacksFoldoutSaveKey {
            get { 
                return "__EditorValue__" + target.GetType().Name + "_callbacksFoldOut"; 
            }
        }


        //Debug tools
        SerializedProperty gizmo_viewValidArea;
        GUIContent gizmo_viewValidAreaLabel = new GUIContent("View Valid Area");

        SerializedProperty gizmo_validAreaColor;
        GUIContent gizmo_validAreaColorLabel = new GUIContent("Valid Area Color");

        bool _debugToolsFoldOut = true;
        string DebugToolsFoldoutSaveKey {
            get { 
                return "__EditorValue__" + target.GetType().Name + "_debugToolsFoldOut"; 
            }
        }


        private void OnEnable()
        {
            useInputSystemIfBothHandling = serializedObject.FindProperty("useInputSystemIfBothHandling");

            widthReference = serializedObject.FindProperty("widthReference");
            validWidth = serializedObject.FindProperty("validWidth");
            timeout = serializedObject.FindProperty("timeout");
            validArea = serializedObject.FindProperty("validArea");

            OnSwipe = serializedObject.FindProperty("OnSwipe");
            OnSwipeRaw = serializedObject.FindProperty("OnSwipeRaw");

            _callbacksFoldOut = PlayerPrefs.GetInt(CallbacksFoldoutSaveKey, 1) == 1;

            //Debug Tools
            gizmo_viewValidArea = serializedObject.FindProperty("gizmo_viewValidArea");
            gizmo_validAreaColor = serializedObject.FindProperty("gizmo_validAreaColor");
            _debugToolsFoldOut = PlayerPrefs.GetInt(DebugToolsFoldoutSaveKey, 1) == 1;
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt(CallbacksFoldoutSaveKey, _callbacksFoldOut ? 1 : 0);
            PlayerPrefs.SetInt(DebugToolsFoldoutSaveKey, _debugToolsFoldOut ? 1 : 0);
        }

        public override void OnInspectorGUI()
        {
            //var obj = target as SwipeInput;

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();


            bool useInputSystem = InputCompatible.EnableInputSystem;
            if (useInputSystem && InputCompatible.EnableLegacyInputManager)
            {
                EditorGUILayout.PropertyField(useInputSystemIfBothHandling, useInputSystemIfBothHandlingLabel);
                useInputSystem = useInputSystemIfBothHandling.boolValue;
            }

            EditorGUILayout.PropertyField(widthReference, widthReferenceLabel);
            EditorGUILayout.PropertyField(validWidth, validWidthLabel);
            EditorGUILayout.PropertyField(timeout, timeoutLabel);
            EditorGUILayout.PropertyField(validArea, validAreaLabel);


            _callbacksFoldOut = EditorGUILayout.Foldout(_callbacksFoldOut, "Callbacks" );
            if (_callbacksFoldOut)
            {
                EditorGUILayout.PropertyField(OnSwipe);
                EditorGUILayout.PropertyField(OnSwipeRaw);
            }


            //Debug Tools
            _debugToolsFoldOut = EditorGUILayout.Foldout(_debugToolsFoldOut, "Debug Tools" );
            if (_debugToolsFoldOut)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(gizmo_viewValidArea, gizmo_viewValidAreaLabel);
                EditorGUILayout.PropertyField(gizmo_validAreaColor, gizmo_validAreaColorLabel);
                EditorGUI.indentLevel--;
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
