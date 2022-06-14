using UnityEngine;
using System;

namespace MoreMountains.Tools
{
    public class MMPropertyLinkColor : MMPropertyLink
    {
        public Func<Color> GetColorDelegate;
        public Action<Color> SetColorDelegate;

        protected Color _initialValue;
        protected Color _newValue;
        protected Color _color;
        public override void Initialization(MMProperty property)
        {
            base.Initialization(property);
            _initialValue = (Color)GetPropertyValue(property);
        }
        public override void CreateGettersAndSetters(MMProperty property)
        {
            base.CreateGettersAndSetters(property);
            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                object firstArgument = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

                if (property.MemberPropertyInfo.GetGetMethod() != null)
                {
                    GetColorDelegate = (Func<Color>)Delegate.CreateDelegate(typeof(Func<Color>),
                                                                                firstArgument,
                                                                                property.MemberPropertyInfo.GetGetMethod());
                }
                if (property.MemberPropertyInfo.GetSetMethod() != null)
                {
                    SetColorDelegate = (Action<Color>)Delegate.CreateDelegate(typeof(Action<Color>),
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
            SetValueOptimized(property, (Color)newValue);
        }
        public override float GetLevel(MMPropertyEmitter emitter, MMProperty property)
        {
            _color = _getterSetterInitialized ? GetColorDelegate() : (Color)GetPropertyValue(property);

            return _color.MeanRGB();
        }
        public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
        {
            base.SetLevel(receiver, property, level);

            _newValue = Color.LerpUnclamped(receiver.ColorRemapZero, receiver.ColorRemapOne, level);

            if (receiver.RelativeValue)
            {
                _newValue = _initialValue + _newValue;
            }

            SetValueOptimized(property, _newValue);
        }
        protected virtual Color GetValueOptimized(MMProperty property)
        {
            return _getterSetterInitialized ? GetColorDelegate() : (Color)GetPropertyValue(property);
        }
        protected virtual void SetValueOptimized(MMProperty property, Color newValue)
        {
            if (_getterSetterInitialized)
            {
                SetColorDelegate(_newValue);
            }
            else
            {
                SetPropertyValue(property, _newValue);
            }
        }
    }
}
