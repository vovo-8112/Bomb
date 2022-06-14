using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    [Serializable]
    public class MMPropertyEmitter : MMPropertyPicker 
    {
        public bool ClampMin = true;
        public bool ClampMax = true;
        public enum Vector2Options { X, Y }
        public enum Vector3Options { X, Y, Z }
        public enum Vector4Options { X, Y, Z, W }
        public Vector2Options Vector2Option;
        public Vector3Options Vector3Option;
        public Vector4Options Vector4Option;
        public float BoolRemapFalse = 0f;
        public float BoolRemapTrue = 1f;
        public int IntRemapMinToZero = 0;
        public int IntRemapMaxToOne = 1;
        public float FloatRemapMinToZero = 0f;
        public float FloatRemapMaxToOne = 1f;
        public float QuaternionRemapMinToZero = 0f;
        public float QuaternionRemapMaxToOne = 360f;
        public float Level = 0f;
        public virtual float GetLevel()
        {
            return _propertySetter.GetLevel(this, _targetMMProperty);
        }
    }
}
