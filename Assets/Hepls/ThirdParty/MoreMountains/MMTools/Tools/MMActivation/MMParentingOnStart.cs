using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMParentingOnStart : MonoBehaviour
    {
        public enum Modes { Awake, Start, Script }
        public Modes Mode = Modes.Awake;
        public Transform TargetParent;
        protected virtual void Awake()
        {
            if (Mode == Modes.Awake)
            {
                Parent();
            }
        }
        protected virtual void Start()
        {
            if (Mode == Modes.Start)
            {
                Parent();
            }
        }
        public virtual void Parent()
        {
            this.transform.SetParent(TargetParent);
        }
    }
}
