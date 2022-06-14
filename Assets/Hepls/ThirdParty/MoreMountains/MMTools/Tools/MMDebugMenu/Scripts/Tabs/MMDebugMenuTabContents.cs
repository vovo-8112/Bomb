using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuTabContents : MonoBehaviour
    {
        public int Index = 0;
        public Transform Parent;
        public bool ForceScaleOne = true;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (ForceScaleOne)
            {
                this.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            }            
        }
    }
}
