using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks {
  [CustomPropertyDrawer(typeof(MMFEnumConditionAttribute))]
  public class MMFEnumConditionAttributeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      MMFEnumConditionAttribute enumConditionAttribute = (MMFEnumConditionAttribute) attribute;
      bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);
      bool previouslyEnabled = GUI.enabled;
      GUI.enabled = enabled;
      if (!enumConditionAttribute.Hidden || enabled) {
        EditorGUI.PropertyField(position, property, label, true);
      }

      GUI.enabled = previouslyEnabled;
    }

    private bool GetConditionAttributeResult(MMFEnumConditionAttribute enumConditionAttribute,
      SerializedProperty property) {
      bool enabled = true;
      string propertyPath = property.propertyPath;
      string conditionPath = propertyPath.Replace(property.name, enumConditionAttribute.ConditionEnum);
      SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

      if (sourcePropertyValue != null) {
        int currentEnum = sourcePropertyValue.enumValueIndex;

        enabled = enumConditionAttribute.ContainsBitFlag(currentEnum);
      } else {
        Debug.LogWarning("No matching boolean found for ConditionAttribute in object: " +
                         enumConditionAttribute.ConditionEnum);
      }

      return enabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      MMFEnumConditionAttribute enumConditionAttribute = (MMFEnumConditionAttribute) attribute;
      bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);

      if (!enumConditionAttribute.Hidden || enabled) {
        return EditorGUI.GetPropertyHeight(property, label);
      } else {
        return -EditorGUIUtility.standardVerticalSpacing;
      }
    }
  }
  [CustomPropertyDrawer(typeof(MMFConditionAttribute))]
  public class MMFConditionAttributeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      MMFConditionAttribute conditionAttribute = (MMFConditionAttribute) attribute;
      bool enabled = GetConditionAttributeResult(conditionAttribute, property);
      bool previouslyEnabled = GUI.enabled;
      GUI.enabled = enabled;
      if (!conditionAttribute.Hidden || enabled) {
        EditorGUI.PropertyField(position, property, label, true);
      }

      GUI.enabled = previouslyEnabled;
    }

    private bool GetConditionAttributeResult(MMFConditionAttribute condHAtt, SerializedProperty property) {
      bool enabled = true;
      string propertyPath = property.propertyPath;
      string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionBoolean);
      SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

      if (sourcePropertyValue != null) {
        enabled = sourcePropertyValue.boolValue;
      } else {
        Debug.LogWarning("No matching boolean found for ConditionAttribute in object: " + condHAtt.ConditionBoolean);
      }

      return enabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      MMFConditionAttribute conditionAttribute = (MMFConditionAttribute) attribute;
      bool enabled = GetConditionAttributeResult(conditionAttribute, property);

      if (!conditionAttribute.Hidden || enabled) {
        return EditorGUI.GetPropertyHeight(property, label);
      } else {
        return -EditorGUIUtility.standardVerticalSpacing;
      }
    }
  }

  [CustomPropertyDrawer(typeof(MMFHiddenAttribute))]
  public class MMFHiddenAttributeDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return 0f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
  }

  [CustomPropertyDrawer(typeof(MMFInformationAttribute))]
  public class MMFInformationAttributeDrawer : PropertyDrawer {
    const int spaceBeforeTheTextBox = 5;
    const int spaceAfterTheTextBox = 10;
    const int iconWidth = 55;

    MMFInformationAttribute informationAttribute {
      get {
        return ((MMFInformationAttribute) attribute);
      }
    }
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label) {
      if (HelpEnabled()) {
        EditorStyles.helpBox.richText = true;
        Rect helpPosition = rect;
        Rect textFieldPosition = rect;

        if (!informationAttribute.MessageAfterProperty) {
          helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);

          textFieldPosition.y += helpPosition.height + spaceBeforeTheTextBox;
          textFieldPosition.height = GetPropertyHeight(prop, label);
        } else {
          textFieldPosition.height = GetPropertyHeight(prop, label);

          helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);
          helpPosition.y += GetPropertyHeight(prop, label) - DetermineTextboxHeight(informationAttribute.Message) -
                            spaceAfterTheTextBox;
        }

        EditorGUI.HelpBox(helpPosition, informationAttribute.Message, informationAttribute.Type);
        EditorGUI.PropertyField(textFieldPosition, prop, label, true);
      } else {
        Rect textFieldPosition = rect;
        textFieldPosition.height = GetPropertyHeight(prop, label);
        EditorGUI.PropertyField(textFieldPosition, prop, label, true);
      }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      if (HelpEnabled()) {
        return EditorGUI.GetPropertyHeight(property) + DetermineTextboxHeight(informationAttribute.Message) +
               spaceAfterTheTextBox + spaceBeforeTheTextBox;
      } else {
        return EditorGUI.GetPropertyHeight(property);
      }
    }
    protected virtual bool HelpEnabled() {
      bool helpEnabled = false;
      if (EditorPrefs.HasKey("MMShowHelpInInspectors")) {
        if (EditorPrefs.GetBool("MMShowHelpInInspectors")) {
          helpEnabled = true;
        }
      }

      return helpEnabled;
    }
    protected virtual float DetermineTextboxHeight(string message) {
      GUIStyle style = new GUIStyle(EditorStyles.helpBox);
      style.richText = true;

      float newHeight = style.CalcHeight(new GUIContent(message), EditorGUIUtility.currentViewWidth - iconWidth);
      return newHeight;
    }
  }

  [CustomPropertyDrawer(typeof(MMFReadOnlyAttribute))]
  public class MMFReadOnlyAttributeDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return EditorGUI.GetPropertyHeight(property, label, true);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      GUI.enabled = false;
      EditorGUI.PropertyField(position, property, label, true);
      GUI.enabled = true;
    }
  }
}