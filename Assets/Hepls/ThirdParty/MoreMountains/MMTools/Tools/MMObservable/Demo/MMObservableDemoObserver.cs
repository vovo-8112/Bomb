using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMObservableDemoObserver : MonoBehaviour
    {
        public MMObservableDemoSubject TargetSubject;
        protected virtual void OnPositionChange()
        {
            this.transform.position = this.transform.position.MMSetY(TargetSubject.PositionX.Value);
        }
        protected virtual void OnEnable()
        {
            TargetSubject.PositionX.OnValueChanged += OnPositionChange;
        }
        protected virtual void OnDisable()
        {
            TargetSubject.PositionX.OnValueChanged -= OnPositionChange;
        }
    }
}
