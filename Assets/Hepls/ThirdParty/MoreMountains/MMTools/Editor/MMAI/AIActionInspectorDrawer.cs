using UnityEngine;
using UnityEditor;
 using System.Collections;

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(AIAction))]
    public class AIActionPropertyInspector : PropertyDrawer
    {
        const float LineHeight = 16f;

        #if  UNITY_EDITOR
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(prop));
            Rect position = rect;
            position.height = height;
            DrawSelectionDropdown(position, prop);
            position.y += height;
            EditorGUI.PropertyField(position, prop);
        }
        
        #endif
        protected virtual void DrawSelectionDropdown(Rect position, SerializedProperty prop)
        {
            AIAction thisAction = prop.objectReferenceValue as AIAction;
            AIAction[] actions = (prop.serializedObject.targetObject as AIBrain).GetAttachedActions();
            int selected = 0;
            int i = 1;
            string[] options = new string[actions.Length + 1];
            options[0] = "None";
            foreach (AIAction action in actions)
            {
                string name = string.IsNullOrEmpty(action.Label) ? action.GetType().Name : action.Label;
                options[i] = i.ToString() + " - " + name;
                if (action == thisAction)
                {
                    selected = i;
                }
                i++;
            }

            EditorGUI.BeginChangeCheck();
            selected = EditorGUI.Popup(position, selected, options);
            if (EditorGUI.EndChangeCheck())
            {
                prop.objectReferenceValue = (selected == 0) ? null : actions[selected - 1];
                prop.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(prop.serializedObject.targetObject);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(property));
            float height = h * 2;
            return height;
        }
    }
}