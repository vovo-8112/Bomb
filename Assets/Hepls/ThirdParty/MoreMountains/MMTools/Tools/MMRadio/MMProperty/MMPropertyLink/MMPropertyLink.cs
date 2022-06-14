using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    public abstract class MMPropertyLink
    {
        protected bool _getterSetterInitialized = false;
        public virtual void Initialization(MMProperty property) 
        {
            CreateGettersAndSetters(property);
        }
        public virtual void CreateGettersAndSetters(MMProperty property)
        {

        }
        public virtual float GetLevel(MMPropertyEmitter emitter, MMProperty property)
        {
            return 0f;
        }
        public virtual void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
        {
            receiver.Level = level;
        }
        public virtual object GetValue(MMPropertyEmitter emitter, MMProperty property)
        {
            return 0f;
        }
        public virtual void SetValue(MMPropertyReceiver receiver, MMProperty property, object newValue)
        {

        }
        protected virtual object GetPropertyValue(MMProperty property)
        {
            object target = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                return property.MemberPropertyInfo.GetValue(target);
            }
            else if (property.MemberType == MMProperty.MemberTypes.Field)
            {
                return property.MemberFieldInfo.GetValue(target);
            }
            return 0f;
        }
        protected virtual void SetPropertyValue(MMProperty property, object newValue)
        {
            object target = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                property.MemberPropertyInfo.SetValue(target, newValue);
            }
            else if (property.MemberType == MMProperty.MemberTypes.Field)
            {
                property.MemberFieldInfo.SetValue(target, newValue);
            }
        }
    }
}
