using UnityEngine;
using System;

namespace MoreMountains.Tools
{
    public class MMPropertyLinkFloat : MMPropertyLink
    {
        public Func<float> GetFloatDelegate;
        public Action<float> SetFloatDelegate;

        protected float _initialValue;
        protected float _newValue;
        public override void Initialization(MMProperty property)
        {
            base.Initialization(property);
            _initialValue = (float)GetPropertyValue(property);
        }
        public override void CreateGettersAndSetters(MMProperty property)
        {
            base.CreateGettersAndSetters(property);
            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                object firstArgument = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

                if (property.MemberPropertyInfo.GetGetMethod() != null)
                {
                    GetFloatDelegate = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>),
                                                                                    firstArgument,
                                                                                    property.MemberPropertyInfo.GetGetMethod());
                }
                if (property.MemberPropertyInfo.GetSetMethod() != null)
                {
                    SetFloatDelegate = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>),
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
            SetValueOptimized(property, (float)newValue);
        }
        public override float GetLevel(MMPropertyEmitter emitter, MMProperty property)
        {
            float returnValue = GetValueOptimized(property);

            returnValue = MMMaths.Clamp(returnValue, emitter.FloatRemapMinToZero, emitter.FloatRemapMaxToOne, emitter.ClampMin, emitter.ClampMax);
            returnValue = MMMaths.Remap(returnValue, emitter.FloatRemapMinToZero, emitter.FloatRemapMaxToOne, 0f, 1f);

            emitter.Level = returnValue;
            return returnValue;
        }
        public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
        {
            base.SetLevel(receiver, property, level);

            _newValue = MMMaths.Remap(level, 0f, 1f, receiver.FloatRemapZero, receiver.FloatRemapOne);

            if (receiver.RelativeValue)
            {
                _newValue = _initialValue + _newValue;
            }

            SetValueOptimized(property, _newValue);
        }
        protected virtual float GetValueOptimized(MMProperty property)
        {
            return _getterSetterInitialized ? GetFloatDelegate() : (float)GetPropertyValue(property);
        }
        protected virtual void SetValueOptimized(MMProperty property, float newValue)
        {
            if (_getterSetterInitialized)
            {
                SetFloatDelegate(_newValue);
            }
            else
            {
                SetPropertyValue(property, _newValue);
            }
        }
    }
}
