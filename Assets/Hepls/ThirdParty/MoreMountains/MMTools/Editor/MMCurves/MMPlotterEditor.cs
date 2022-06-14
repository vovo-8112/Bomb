using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MMPlotter))]
    public class MMPlotterEditor : Editor
    {
        protected string[] _typeDisplays;
        protected string[] _excludedProperties = new string[] { "TweenMethod", "m_Script" }; 

        protected MMPlotter _mmPlotter;

        protected virtual void OnEnable()
        {
            _mmPlotter = target as MMPlotter;
            _typeDisplays = _mmPlotter.GetMethodsList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(target, "Modified Plotter");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tween Method", EditorStyles.boldLabel);

            _mmPlotter.TweenMethodIndex = EditorGUILayout.Popup("Tween Method", _mmPlotter.TweenMethodIndex, _typeDisplays, EditorStyles.popup);
            DrawPropertiesExcluding(serializedObject, _excludedProperties);

            if (GUILayout.Button("Draw Graph"))
            {
                _mmPlotter.DrawGraph();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
