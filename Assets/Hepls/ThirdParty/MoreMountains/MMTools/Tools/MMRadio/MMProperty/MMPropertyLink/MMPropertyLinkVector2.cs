using UnityEngine;
using System;

namespace MoreMountains.Tools
{
    public class MMPropertyLinkVector2 : MMPropertyLink
    {
        public Func<Vector2> GetVector2Delegate;
        public Action<Vector2> SetVector2Delegate;

        protected Vector2 _initialValue;
        protected Vector2 _newValue;
        protected Vector2 _vector2;
        public override void Initialization(MMProperty property)
        {
            base.Initialization(property);
            _initialValue = (Vector2)GetPropertyValue(property);
        }
        public override void CreateGettersAndSetters(MMProperty property)
        {
            base.CreateGettersAndSetters(property);
            if (property.MemberType == MMProperty.MemberTypes.Property)
            {
                object firstArgument = (property.TargetScriptableObject == null) ? (object)property.TargetComponent : (object)property.TargetScriptableObject;

                if (property.MemberPropertyInfo.GetGetMethod() != null)
                {
                    GetVector2Delegate = (Func<Vector2>)Delegate.CreateDelegate(typeof(Func<Vector2>),
                                                                                firstArgument,
                                                                                property.MemberPropertyInfo.GetGetMethod());
                }
                if (property.MemberPropertyInfo.GetSetMethod() != null)
                {
                    SetVector2Delegate = (Action<Vector2>)Delegate.CreateDelegate(typeof(Action<Vector2>),
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
            SetValueOptimized(property, (Vector2)newValue);
        }
        public override float GetLevel(MMPropertyEmitter emitter, MMProperty property)
        {
            _vector2 = _getterSetterInitialized ? GetVector2Delegate() : (Vector2)GetPropertyValue(property);

            float newValue = 0f;

            switch (emitter.Vector2Option)
            {
                case MMPropertyEmitter.Vector2Options.X:
                    newValue = _vector2.x;
                    break;
                case MMPropertyEmitter.Vector2Options.Y:
                    newValue = _vector2.y;
                    break;
            }

            float returnValue = newValue;
            returnValue = MMMaths.Clamp(returnValue, emitter.FloatRemapMinToZero, emitter.FloatRemapMaxToOne, emitter.ClampMin, emitter.ClampMax);
            returnValue = MMMaths.Remap(returnValue, emitter.FloatRemapMinToZero, emitter.FloatRemapMaxToOne, 0f, 1f);

            emitter.Level = returnValue;
            return returnValue;
        }
        public override void SetLevel(MMPropertyReceiver receiver, MMProperty property, float level)
        {
            base.SetLevel(receiver, property, level);

            _newValue.x = receiver.ModifyX ? MMMaths.Remap(level, 0f, 1f, receiver.Vector2RemapZero.x, receiver.Vector2RemapOne.x) : 0f;
            _newValue.y = receiver.ModifyY ? MMMaths.Remap(level, 0f, 1f, receiver.Vector2RemapZero.y, receiver.Vector2RemapOne.y) : 0f;

            if (receiver.RelativeValue)
            {
                _newValue = _initialValue + _newValue;
            }
            
            if (_getterSetterInitialized)
            {
                SetVector2Delegate(_newValue);
            }
            else
            {
                SetPropertyValue(property, _newValue);
            }
        }
        protected virtual Vector2 GetValueOptimized(MMProperty property)
        {
            return _getterSetterInitialized ? GetVector2Delegate() : (Vector2)GetPropertyValue(property);
        }
        protected virtual void SetValueOptimized(MMProperty property, Vector2 newValue)
        {
            if (_getterSetterInitialized)
            {
                SetVector2Delegate(_newValue);
            }
            else
            {
                SetPropertyValue(property, _newValue);
            }
        }
    }
}
