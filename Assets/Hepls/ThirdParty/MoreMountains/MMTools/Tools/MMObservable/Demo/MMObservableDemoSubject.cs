using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMObservableDemoSubject : MonoBehaviour
    {
        public MMObservable<float> PositionX = new MMObservable<float>();
        protected virtual void Update()
        {
            PositionX.Value = this.transform.position.x;
        }
    }
}
