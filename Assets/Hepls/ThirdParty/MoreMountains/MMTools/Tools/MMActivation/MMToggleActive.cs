using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMToggleActive : MonoBehaviour
    {
        [Header("Target - leave empty for self")]
        public GameObject TargetGameObject;
        [MMInspectorButton("ToggleActive")]        
        public bool ToggleActiveButton;
        protected virtual void Awake()
        {
            if (TargetGameObject == null)
            {
                TargetGameObject = this.gameObject;
            }
        }
        public virtual void ToggleActive()
        {
            TargetGameObject.SetActive(!TargetGameObject.activeInHierarchy);
        }
    }
}
