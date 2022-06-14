using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMObservableDemoObserverAutoSleep : MonoBehaviour
    {
        public MMObservableDemoSubject TargetSubject;

        protected virtual void OnSpeedChange()
        {
            this.transform.position = this.transform.position.MMSetY(TargetSubject.PositionX.Value);
        }
        protected virtual void Awake()
        {
            TargetSubject.PositionX.OnValueChanged += OnSpeedChange;
            this.enabled = false;
        }
        protected virtual void OnDestroy()
        {
            TargetSubject.PositionX.OnValueChanged -= OnSpeedChange;
        }
        protected virtual void OnEnable()
        {

        }
        protected virtual void OnDisable()
        {

        }
    }
}
