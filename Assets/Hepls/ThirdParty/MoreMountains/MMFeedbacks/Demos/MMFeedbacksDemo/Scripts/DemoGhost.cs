using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public class DemoGhost : MonoBehaviour
    {
        public virtual void OnAnimationEnd()
        {
            this.gameObject.SetActive(false);
        }
    }
}