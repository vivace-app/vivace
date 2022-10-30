using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(SwipeEventTrigger))]
    public class SwipeEventTriggerEditor : UnityEditor.Editor
    {
        SerializedProperty useInputSystemIfBothHandling;
        GUIContent useInputSystemIfBothHandlingLabel = new GUIContent("Use InputSystem If Both Handling");

        SerializedProperty widthReference;
        GUIContent widthReferenceLabel = new GUIContent("Width Reference");

        SerializedProperty validWidth;
        GUIContent validWidthLabel = new GUIContent("Valid Width");

        SerializedProperty timeout;
        GUIContent timeoutLabel = new GUIContent("Timeout");

        SerializedProperty OnSwipe;
        SerializedProperty OnSwipeRaw;

        bool _callbacksFoldOut = true;
        string CallbacksFoldoutSaveKey {
            get { 
                return "__EditorValue__" + target.GetType().Name + "_callbacksFoldOut"; 
            }
        }

        private void OnEnable()
        {
            useInputSystemIfBothHandling = serializedObject.FindProperty("useInputSystemIfBothHandling");

            widthReference = serializedObject.FindProperty("widthReference");
            validWidth = serializedObject.FindProperty("validWidth");
            timeout = serializedObject.FindProperty("timeout");

            OnSwipe = serializedObject.FindProperty("OnSwipe");
            OnSwipeRaw = serializedObject.FindProperty("OnSwipeRaw");
            
            _callbacksFoldOut = PlayerPrefs.GetInt(CallbacksFoldoutSaveKey, 1) == 1;
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt(CallbacksFoldoutSaveKey, _callbacksFoldOut ? 1 : 0);
        }

        public override void OnInspectorGUI()
        {
            //var obj = target as SwipeEventTrigger;

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


            _callbacksFoldOut = EditorGUILayout.Foldout(_callbacksFoldOut, "Callbacks" );
            if (_callbacksFoldOut)
            {
                EditorGUILayout.PropertyField(OnSwipe);
                EditorGUILayout.PropertyField(OnSwipeRaw);
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
