using UnityEngine;
using System;

namespace MoreMountains.Tools
{
    public class MMPropertyLinkString : MMPropertyLink
    {
        public Func<string> GetStringDelegate;
        public Action<string> SetStringDelegate;

        protected string _initialValue;
        protected string _newValue;
        public override void Initialization(MMProperty property)
        {
            base.Initialization(property);
            _initialValue = (string)GetPropertyValue(property);
        }
        public override void CreateGettersAndSetters(MMProperty property)
        {
            base.CreateGettersAndSetters(property);
            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                object firstArgument = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

                if (property.MemberPropertyInfo.GetGetMethod() != null)
                {
                    GetStringDelegate = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>),
                                                                                firstArgument,
                                                                                property.MemberPropertyInfo.GetGetMethod());
                }
                if (property.MemberPropertyInfo.GetSetMethod() != null)
                {
                    SetStringDelegate = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>),
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
            SetValueOptimized(property, (string)newValue);
        }
        public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
        {
            base.SetLevel(receiver, property, level);
            _newValue = (level > receiver.Threshold) ? receiver.StringRemapOne : receiver.StringRemapZero;

            SetValueOptimized(property, _newValue);
        }
        protected virtual string GetValueOptimized(MMProperty property)
        {
            return _getterSetterInitialized ? GetStringDelegate() : (string)GetPropertyValue(property);
        }
        protected virtual void SetValueOptimized(MMProperty property, string newValue)
        {
            if (_getterSetterInitialized)
            {
                SetStringDelegate(_newValue);
            }
            else
            {
                SetPropertyValue(property, _newValue);
            }
        }
    }
}
