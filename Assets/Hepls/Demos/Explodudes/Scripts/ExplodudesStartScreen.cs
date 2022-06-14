using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ExplodudesStartScreen : MonoBehaviour
    {
        protected virtual void Start()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        public virtual void DisableStartScreen()
        {
            this.gameObject.SetActive(false);
        }
    }
}
