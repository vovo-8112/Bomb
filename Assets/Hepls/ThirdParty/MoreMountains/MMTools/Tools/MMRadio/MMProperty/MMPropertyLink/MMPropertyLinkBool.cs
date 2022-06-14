using UnityEngine;
using System;

namespace MoreMountains.Tools
{
    public class MMPropertyLinkBool : MMPropertyLink
    {
        public Func<bool> GetBoolDelegate;
        public Action<bool> SetBoolDelegate;

        protected bool _initialValue;
        protected bool _newValue;
        public override void Initialization(MMProperty property)
        {
            base.Initialization(property);
            _initialValue = (bool)GetPropertyValue(property);
        }
        public override void CreateGettersAndSetters(MMProperty property)
        {
            base.CreateGettersAndSetters(property);
            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                object firstArgument = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

                if (property.MemberPropertyInfo.GetGetMethod() != null)
                {
                    GetBoolDelegate = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>),
                                                                                firstArgument,
                                                                                property.MemberPropertyInfo.GetGetMethod());
                }
                if (property.MemberPropertyInfo.GetSetMethod() != null)
                {
                    SetBoolDelegate = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>),
                                                                            firstArgument,
                                                                            property.MemberPropertyInfo.GetSetMethod());
                }
                _getterSetterInitialized = true;
            }
        }
        public override object GetValue(MMPropertyEmitter emitter, MMProperty property)
        {
            return GetValueOptimized(property);
        }
        public override void SetValue(MMPropertyReceiver receiver, MMProperty property, object newValue)
        {
            SetValueOptimized(property, (bool)newValue);
        }
        public override float GetLevel(MMPropertyEmitter emitter, MMProperty property)
        {
            bool boolValue = GetValueOptimized(property);
            float returnValue = (boolValue == true) ? emitter.BoolRemapTrue : emitter.BoolRemapFalse;
            emitter.Level = returnValue;
            return returnValue;
        }
        public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
        {
            base.SetLevel(receiver, property, level);
            _newValue = (level > receiver.Threshold) ? receiver.BoolRemapOne : receiver.BoolRemapZero;            
            SetValueOptimized(property, _newValue);
        }
        protected virtual bool GetValueOptimized(MMProperty property)
        {
            return _getterSetterInitialized ? GetBoolDelegate() : (bool)GetPropertyValue(property);
        }
        protected virtual void SetValueOptimized(MMProperty property, bool newValue)
        {
            if (_getterSetterInitialized)
            {
                SetBoolDelegate(_newValue);
            }
            else
            {
                SetPropertyValue(property, _newValue);
            }
        }
    }
}
