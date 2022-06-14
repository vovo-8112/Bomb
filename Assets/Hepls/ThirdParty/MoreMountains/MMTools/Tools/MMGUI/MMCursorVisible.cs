using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/GUI/MMCursorVisible")]
    public class MMCursorVisible : MonoBehaviour
    {
        public enum CursorVisibilities { Visible, Invisible }
        public CursorVisibilities CursorVisibility = CursorVisibilities.Visible;
        protected virtual void Update()
        {
            if (CursorVisibility == CursorVisibilities.Visible)
            {
                Cursor.visible = true;
            }
            else
            {
                Cursor.visible = false;
            }
        }
    }
}
