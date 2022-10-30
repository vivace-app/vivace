using System;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(LongClickEventTrigger))]
    public class LongClickEventTriggerEditor : UnityEditor.Editor
    {
        SerializedProperty validTime;
        GUIContent validTimeLabel = new GUIContent("Valid Time");

        SerializedProperty OnLongClick;
        SerializedProperty OnStart;
        SerializedProperty OnProgress;
        SerializedProperty OnCancel;


        bool _callbacksFoldOut = true;
        string CallbacksFoldoutSaveKey {
            get { 
                return "__EditorValue__" + target.GetType().Name + "_callbacksFoldOut"; 
            }
        }

        private void OnEnable()
        {
            validTime = serializedObject.FindProperty("validTime");

            OnLongClick = serializedObject.FindProperty("OnLongClick");
            OnStart = serializedObject.FindProperty("OnStart");
            OnProgress = serializedObject.FindProperty("OnProgress");
            OnCancel = serializedObject.FindProperty("OnCancel");
            
            _callbacksFoldOut = PlayerPrefs.GetInt(CallbacksFoldoutSaveKey, 1) == 1;
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt(CallbacksFoldoutSaveKey, _callbacksFoldOut ? 1 : 0);
        }

        public override void OnInspectorGUI()
        {
            //var obj = target as LongClickEventTrigger;

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();


            EditorGUILayout.PropertyField(validTime, validTimeLabel);


            _callbacksFoldOut = EditorGUILayout.Foldout(_callbacksFoldOut, "Callbacks" );
            if (_callbacksFoldOut)
            {
                EditorGUILayout.PropertyField(OnLongClick);
                EditorGUILayout.PropertyField(OnStart);
                EditorGUILayout.PropertyField(OnProgress);
                EditorGUILayout.PropertyField(OnCancel);
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
