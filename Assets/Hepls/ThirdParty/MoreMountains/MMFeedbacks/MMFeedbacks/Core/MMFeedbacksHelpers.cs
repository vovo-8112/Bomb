using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using System.Reflection;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    public class MMFeedbacksHelpers : MonoBehaviour
    {
		public static float Remap(float x, float A, float B, float C, float D)
        {
            float remappedValue = C + (x - A) / (B - A) * (D - C);
            return remappedValue;
        }
    }

    public class MMFReadOnlyAttribute : PropertyAttribute { }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class MMFInspectorButtonAttribute : PropertyAttribute
    {
        public readonly string MethodName;

        public MMFInspectorButtonAttribute(string MethodName)
        {
            this.MethodName = MethodName;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class MMFEnumConditionAttribute : PropertyAttribute
    {
        public string ConditionEnum = "";
        public bool Hidden = false;

        BitArray bitArray = new BitArray(32);
        public bool ContainsBitFlag(int enumValue)
        {
            return bitArray.Get(enumValue);
        }

        public MMFEnumConditionAttribute(string conditionBoolean, params int[] enumValues)
        {
            this.ConditionEnum = conditionBoolean;
            this.Hidden = true;

            for (int i = 0; i < enumValues.Length; i++)
            {
                bitArray.Set(enumValues[i], true);
            }
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MMFInspectorButtonAttribute))]
    public class MMFInspectorButtonPropertyDrawer : PropertyDrawer
    {
        private MethodInfo _eventMethodInfo = null;

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            MMFInspectorButtonAttribute inspectorButtonAttribute = (MMFInspectorButtonAttribute)attribute;

            float buttonLength = position.width;
            Rect buttonRect = new Rect(position.x + (position.width - buttonLength) * 0.5f, position.y, buttonLength, position.height);

            if (GUI.Button(buttonRect, inspectorButtonAttribute.MethodName))
            {
                System.Type eventOwnerType = prop.serializedObject.targetObject.GetType();
                string eventName = inspectorButtonAttribute.MethodName;

                if (_eventMethodInfo == null)
                {
                    _eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }

                if (_eventMethodInfo != null)
                {
                    _eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
                }
                else
                {
                    Debug.LogWarning(string.Format("InspectorButton: Unable to find method {0} in {1}", eventName, eventOwnerType));
                }
            }
        }
    }
#endif

    public class MMFInformationAttribute : PropertyAttribute
    {
        public enum InformationType { Error, Info, None, Warning }

#if UNITY_EDITOR
        public string Message;
        public MessageType Type;
        public bool MessageAfterProperty;

        public MMFInformationAttribute(string message, InformationType type, bool messageAfterProperty)
        {
            this.Message = message;
            if (type == InformationType.Error) { this.Type = UnityEditor.MessageType.Error; }
            if (type == InformationType.Info) { this.Type = UnityEditor.MessageType.Info; }
            if (type == InformationType.Warning) { this.Type = UnityEditor.MessageType.Warning; }
            if (type == InformationType.None) { this.Type = UnityEditor.MessageType.None; }
            this.MessageAfterProperty = messageAfterProperty;
        }
#else
		public MMFInformationAttribute(string message, InformationType type, bool messageAfterProperty)
		{

		}
#endif
    }

    public class MMFHiddenAttribute : PropertyAttribute { }

    [AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class MMFConditionAttribute : PropertyAttribute
    {
        public string ConditionBoolean = "";
        public bool Hidden = false;

        public MMFConditionAttribute(string conditionBoolean)
        {
            this.ConditionBoolean = conditionBoolean;
            this.Hidden = false;
        }

        public MMFConditionAttribute(string conditionBoolean, bool hideInInspector)
        {
            this.ConditionBoolean = conditionBoolean;
            this.Hidden = hideInInspector;
        }
    }

    public static class MMFeedbackStaticMethods
    {
        static List<Component> m_ComponentCache = new List<Component>();
		public static Component GetComponentNoAlloc(this GameObject @this, System.Type componentType)
        {
            @this.GetComponents(componentType, m_ComponentCache);
            var component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component;
        }
        public static T MMFGetComponentNoAlloc<T>(this GameObject @this) where T : Component
        {
            @this.GetComponents(typeof(T), m_ComponentCache);
            Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component as T;
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FeedbackPathAttribute : System.Attribute
    {
        public string Path;
        public string Name;

        public FeedbackPathAttribute(string path)
        {
            Path = path;
            Name = path.Split('/').Last();
        }

        static public string GetFeedbackDefaultName(System.Type type)
        {
            FeedbackPathAttribute attribute = type.GetCustomAttributes(false).OfType<FeedbackPathAttribute>().FirstOrDefault();
            return attribute != null ? attribute.Name : type.Name;
        }

        static public string GetFeedbackDefaultPath(System.Type type)
        {
            FeedbackPathAttribute attribute = type.GetCustomAttributes(false).OfType<FeedbackPathAttribute>().FirstOrDefault();
            return attribute != null ? attribute.Path : type.Name;
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FeedbackHelpAttribute : System.Attribute
    {
        public string HelpText;

        public FeedbackHelpAttribute(string helpText)
        {
            HelpText = helpText;
        }

        static public string GetFeedbackHelpText(System.Type type)
        {
            FeedbackHelpAttribute attribute = type.GetCustomAttributes(false).OfType<FeedbackHelpAttribute>().FirstOrDefault();
            return attribute != null ? attribute.HelpText : "";
        }
    }
}