using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class MMDebugLogCommandArgumentCountAttribute : System.Attribute
    {
        public readonly int ArgumentCount;

        public MMDebugLogCommandArgumentCountAttribute(int argumentCount)
        {
            this.ArgumentCount = argumentCount;
        }
    }
}
