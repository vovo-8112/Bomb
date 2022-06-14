using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Events;

namespace MoreMountains.Tools 
{
    [Serializable]
    public class MMPropertyReceiver : MMPropertyPicker
    {
        public bool ShouldModifyValue = true;
        public bool RelativeValue = true;
        public bool ModifyX = true;
        public bool ModifyY = true;
        public bool ModifyZ = true;
        public bool ModifyW = true;
        public float Threshold = 0.5f;
        public bool BoolRemapZero = false;
        public bool BoolRemapOne = true;
        public string StringRemapZero = "Zero";
        public string StringRemapOne = "One";
        public int IntRemapZero = 0;
        public int IntRemapOne = 1;
        public float FloatRemapZero = 0f;
        public float FloatRemapOne = 1f;
        public Vector2 Vector2RemapZero = Vector2.zero;
        public Vector2 Vector2RemapOne = Vector2.one;
        public Vector3 Vector3RemapZero = Vector3.zero;
        public Vector3 Vector3RemapOne = Vector3.one;
        public Vector4 Vector4RemapZero = Vector4.zero;
        public Vector4 Vector4RemapOne = Vector4.one;
        public Vector3 QuaternionRemapZero = Vector3.zero;
        public Vector3 QuaternionRemapOne = new Vector3(180f, 180f, 180f);
        public Color ColorRemapZero = Color.white;
        public Color ColorRemapOne = Color.black;
        public float Level = 0f;
        public virtual void SetLevel(float newLevel)
        {
            if (!PropertyFound)
            {
                return;
            }

            if (!ShouldModifyValue)
            {
                return;
            }
            
            _propertySetter.SetLevel(this, _targetMMProperty, newLevel);
        }       
    }
}
