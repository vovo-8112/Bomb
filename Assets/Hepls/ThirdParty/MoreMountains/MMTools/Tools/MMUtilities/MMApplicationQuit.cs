using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMApplicationQuit : MonoBehaviour
    {
        [Header("Debug")]
        [MMInspectorButton("Quit")] 
        public bool QuitButton;
        public virtual void Quit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
