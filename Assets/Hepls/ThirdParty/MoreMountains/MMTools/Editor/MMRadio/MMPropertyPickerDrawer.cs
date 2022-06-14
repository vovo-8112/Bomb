using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(MMPropertyPicker), true)]
    [CanEditMultipleObjects]
    public class MMPropertyPickerDrawer : PropertyDrawer
    {
        protected UnityEngine.Object _TargetObject;
        protected GameObject _TargetGameObject;

        protected const int _lineHeight = 20;
        protected const int _lineMargin = 2;

        protected int _selectedComponentIndex = 0;
        protected int _selectedPropertyIndex = 0;

        public const string _undefinedComponentString = "<Undefined Component>";
        public const string _undefinedPropertyString = "<Undefined Property>";

        protected bool _initialized = false;

        protected string[] _componentNames;
        protected List<Component> _componentList;

        protected string[] _propertiesNames;
        protected List<string> _propertiesList;
        protected Type _propertyType = null;

        protected int _numberOfLines = 0;
        protected Color _progressBarBackground = new Color(0, 0, 0, 0.5f);

        protected Type[] _authorizedTypes;
        protected bool _targetIsScriptableObject;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialization(property);

            _numberOfLines = 2;

            if (_TargetObject != null)
            {
                _numberOfLines = 3;
                if (_selectedComponentIndex != 0)
                {
                    _numberOfLines = 4;
                }
            }

            if (_targetIsScriptableObject)
            {
                _numberOfLines = 4;
            }

            return _lineHeight * _numberOfLines + _lineMargin * _numberOfLines - 1 + AdditionalHeight();
        }

        public virtual float AdditionalHeight()
        {
            return 0f;
        }
        protected virtual void Initialization(SerializedProperty property)
        {
            if (_initialized)
            {
                return;
            }

            FillAuthorizedTypes();

            FillComponentsList(property);
            FillPropertyList(property);

            GetComponentIndex(property);
            GetPropertyIndex(property);

            _propertyType = GetPropertyType(property);

            _initialized = true;
        }

        protected static bool AuthorizedType(Type[] typeArray, Type checkedType)
        {
            foreach (Type t in typeArray)
            {
                if (t == checkedType)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void FillAuthorizedTypes()
        {
            _authorizedTypes = new Type[]
            {
                    typeof(String),
                    typeof(float),
                    typeof(Vector2),
                    typeof(Vector3),
                    typeof(Vector4),
                    typeof(Quaternion),
                    typeof(int),
                    typeof(bool),
                    typeof(Color)
            };
        }

        #if  UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialization(property);
            Rect targetLabelRect = new Rect(position.x, position.y, position.width, _lineHeight);
            Rect targetObjectRect = new Rect(position.x, position.y + (_lineHeight + _lineMargin), position.width, _lineHeight);
            Rect targetComponentRect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 2, position.width, _lineHeight);
            Rect targetPropertyRect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * 3, position.width, _lineHeight);

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(targetLabelRect, new GUIContent(property.name));

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(targetObjectRect, property.FindPropertyRelative("TargetObject"), new GUIContent("Target Object"), true);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                _TargetObject = property.FindPropertyRelative("TargetObject").objectReferenceValue as UnityEngine.Object;
                FillComponentsList(property);
                _selectedComponentIndex = 0;
                _selectedPropertyIndex = 0;
                SetTargetComponent(property);
                if (_targetIsScriptableObject)
                {
                    FillPropertyList(property);
                }
            }
            if (_targetIsScriptableObject)
            {
                EditorGUI.LabelField(targetComponentRect, "Type", "Scriptable Object");
            }
            if ((_componentNames != null) && (_componentNames.Length > 0))
            {
                EditorGUI.BeginChangeCheck();
                _selectedComponentIndex = EditorGUI.Popup(targetComponentRect, "Component", _selectedComponentIndex, _componentNames);
                if (EditorGUI.EndChangeCheck())
                {
                    SetTargetComponent(property);
                    _selectedPropertyIndex = 0;
                    FillPropertyList(property);
                }
            }
            if (((_selectedComponentIndex != 0) || _targetIsScriptableObject) && (_propertiesNames != null) && (_propertiesNames.Length > 0))
            {
                EditorGUI.BeginChangeCheck();
                _selectedPropertyIndex = EditorGUI.Popup(targetPropertyRect, "Property", _selectedPropertyIndex, _propertiesNames);
                if (EditorGUI.EndChangeCheck())
                {
                    SetTargetProperty(property);
                }
            }

            DisplayAdditionalProperties(position, property, label);

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
        #endif

        protected virtual void DisplayAdditionalProperties(Rect position, SerializedProperty property, GUIContent label)
        {

        }

        protected virtual void DrawLevelProgressBar(Rect position, float level, Color frontColor, Color negativeColor)
        {
            Rect levelLabelRect = new Rect(position.x, position.y + (_lineHeight + _lineMargin) * (_numberOfLines - 1), position.width, _lineHeight);
            Rect levelValueRect = new Rect(position.x - 15 + EditorGUIUtility.labelWidth + 4, position.y + (_lineHeight + _lineMargin) * (_numberOfLines - 1), position.width, _lineHeight);

            float progressX = position.x - 5 + EditorGUIUtility.labelWidth + 60;
            float progressY = position.y + (_lineHeight + _lineMargin) * (_numberOfLines - 1) + 6;
            float progressHeight = 10f;
            float fullProgressWidth = position.width - EditorGUIUtility.labelWidth - 60 + 5;

            bool negative = false;
            float displayLevel = level;
            if (level < 0f)
            {
                negative = true;
                level = -level;
            }

            float progressLevel = Mathf.Clamp01(level);
            Rect levelProgressBg = new Rect(progressX, progressY, fullProgressWidth, progressHeight);
            float progressWidth = MMMaths.Remap(progressLevel, 0f, 1f, 0f, fullProgressWidth);
            Rect levelProgressFront = new Rect(progressX, progressY, progressWidth, progressHeight);

            EditorGUI.LabelField(levelLabelRect, new GUIContent("Level"));
            EditorGUI.LabelField(levelValueRect, new GUIContent(displayLevel.ToString("F4")));
            EditorGUI.DrawRect(levelProgressBg, _progressBarBackground);
            if (negative)
            {
                EditorGUI.DrawRect(levelProgressFront, negativeColor);
            }
            else
            {
                EditorGUI.DrawRect(levelProgressFront, frontColor);
            }            
        }
        protected virtual void FillComponentsList(SerializedProperty property)
        {
            _TargetObject = property.FindPropertyRelative("TargetObject").objectReferenceValue as UnityEngine.Object;
            _TargetGameObject = property.FindPropertyRelative("TargetObject").objectReferenceValue as GameObject;

            _targetIsScriptableObject = false;
            if (property.FindPropertyRelative("TargetObject").objectReferenceValue is ScriptableObject)
            {
                _targetIsScriptableObject = true;
            }

            if (_TargetGameObject == null)
            {
                _componentNames = null;
                return;
            }
            _componentList = new List<Component>();
            _componentNames = new string[0];
            List<string> tempComponentsNameList = new List<string>();
            tempComponentsNameList.Add(_undefinedComponentString);
            _componentList.Add(null);
            Component[] components = _TargetGameObject.GetComponents(typeof(Component));
            foreach (Component component in components)
            {
                _componentList.Add(component);
                tempComponentsNameList.Add(component.GetType().Name);
            }
            _componentNames = tempComponentsNameList.ToArray();
        }
        protected virtual void FillPropertyList(SerializedProperty property)
        {
            if (_TargetObject == null)
            {
                return;
            }

            if ((property.FindPropertyRelative("TargetComponent").objectReferenceValue == null)
                && !_targetIsScriptableObject)
            {
                return;
            }
            _propertiesNames = new string[0];
            _propertiesList = new List<string>();
            List<string> tempPropertiesList = new List<string>();
            tempPropertiesList.Add(_undefinedPropertyString);
            _propertiesList.Add("");

            if (!_targetIsScriptableObject)
            {
                var fieldsList = property.FindPropertyRelative("TargetComponent").objectReferenceValue.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(field =>
                        (AuthorizedType(_authorizedTypes, field.FieldType))
                    )
                    .OrderBy(prop => prop.FieldType.Name).ToList();

                foreach (FieldInfo fieldInfo in fieldsList)
                {
                    string newEntry = fieldInfo.Name + " [Field - " + fieldInfo.FieldType.Name + "]";
                    tempPropertiesList.Add(newEntry);
                    _propertiesList.Add(fieldInfo.Name);
                }
                var propertiesList = property.FindPropertyRelative("TargetComponent").objectReferenceValue.GetType()
                  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(prop =>
                        (AuthorizedType(_authorizedTypes, prop.PropertyType))
                    )
                    .OrderBy(prop => prop.PropertyType.Name).ToList();

                foreach (PropertyInfo foundProperty in propertiesList)
                {
                    string newEntry = foundProperty.Name + " [Property - " + foundProperty.PropertyType.Name + "]";
                    tempPropertiesList.Add(newEntry);
                    _propertiesList.Add(foundProperty.Name);
                }
            }
            else
            {
                var fieldsList = property.FindPropertyRelative("TargetObject").objectReferenceValue.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(field =>
                        (AuthorizedType(_authorizedTypes, field.FieldType))
                    )
                    .OrderBy(prop => prop.FieldType.Name).ToList();

                foreach (FieldInfo fieldInfo in fieldsList)
                {
                    string newEntry = fieldInfo.Name + " [Field - " + fieldInfo.FieldType.Name + "]";
                    tempPropertiesList.Add(newEntry);
                    _propertiesList.Add(fieldInfo.Name);
                }
                var propertiesList = property.FindPropertyRelative("TargetObject").objectReferenceValue.GetType()
                  .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(prop =>
                        (AuthorizedType(_authorizedTypes, prop.PropertyType))
                    )
                    .OrderBy(prop => prop.PropertyType.Name).ToList();

                foreach (PropertyInfo foundProperty in propertiesList)
                {
                    string newEntry = foundProperty.Name + " [Property - " + foundProperty.PropertyType.Name + "]";
                    tempPropertiesList.Add(newEntry);
                    _propertiesList.Add(foundProperty.Name);
                }
            }

            _propertiesNames = tempPropertiesList.ToArray();
        }
        protected virtual void SetTargetProperty(SerializedProperty property)
        {
            if (_selectedPropertyIndex > 0)
            {
                property.serializedObject.Update();
                property.FindPropertyRelative("TargetPropertyName").stringValue = _propertiesList[_selectedPropertyIndex];
                property.serializedObject.ApplyModifiedProperties();
                _propertyType = GetPropertyType(property);
            }
            else
            {
                property.serializedObject.Update();
                property.FindPropertyRelative("TargetPropertyName").stringValue = "";
                property.serializedObject.ApplyModifiedProperties();
                _selectedPropertyIndex = 0;
                _propertyType = null;
            }
        }
        protected virtual void SetTargetComponent(SerializedProperty property)
        {
            if (_targetIsScriptableObject)
            {
                property.serializedObject.Update();
                property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue = property.FindPropertyRelative("TargetObject").objectReferenceValue as ScriptableObject;
                property.FindPropertyRelative("TargetComponent").objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            if (_selectedComponentIndex > 0)
            {
                property.serializedObject.Update();
                property.FindPropertyRelative("TargetComponent").objectReferenceValue = _componentList[_selectedComponentIndex];
                property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                property.serializedObject.Update();
                property.FindPropertyRelative("TargetComponent").objectReferenceValue = null;
                property.FindPropertyRelative("TargetPropertyName").stringValue = "";
                property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue = null;
                _selectedComponentIndex = 0;
                _selectedPropertyIndex = 0;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        protected virtual void GetComponentIndex(SerializedProperty property)
        {
            int index = 0;
            bool found = false;

            Component targetComponent = property.FindPropertyRelative("TargetComponent").objectReferenceValue as Component;

            if ((_componentList == null) || (_componentList.Count == 0))
            {
                _selectedComponentIndex = 0;
                return;
            }

            foreach (Component component in _componentList)
            {
                if (component == targetComponent)
                {
                    _selectedComponentIndex = index;
                    found = true;
                }
                index++;
            }
            if (!found)
            {
                _selectedComponentIndex = 0;
            }
        }
        protected virtual void GetPropertyIndex(SerializedProperty property)
        {
            int index = 0;
            bool found = false;

            Component targetComponent = property.FindPropertyRelative("TargetComponent").objectReferenceValue as Component;
            ScriptableObject targetScriptable = property.FindPropertyRelative("TargetScriptableObject").objectReferenceValue as ScriptableObject;

            if ((targetComponent == null) && (targetScriptable == null))
            {
                return;
            }

            string targetProperty = property.FindPropertyRelative("TargetPropertyName").stringValue;

            if ((_propertiesList == null) || (_propertiesList.Count == 0))
            {
                _selectedPropertyIndex = 0;
                return;
            }

            foreach (string prop in _propertiesList)
            {
                if (prop == targetProperty)
                {
                    _selectedPropertyIndex = index;
                    found = true;
                }
                index++;
            }
            if (!found)
            {
                _selectedPropertyIndex = 0;
            }

        }

        protected virtual Type GetPropertyType(SerializedProperty property)
        {
            if (_selectedPropertyIndex == 0)
            {
                return null;
            }

            MMProperty tempProperty;

            tempProperty = MMProperty.FindProperty(_propertiesList[_selectedPropertyIndex], property.FindPropertyRelative("TargetComponent").objectReferenceValue as Component, null, property.FindPropertyRelative("TargetObject").objectReferenceValue as ScriptableObject);
                        
            if (tempProperty != null)
            {
                return tempProperty.PropertyType;
            }
            else
            {
                return null;
            }
        }

    }
}
