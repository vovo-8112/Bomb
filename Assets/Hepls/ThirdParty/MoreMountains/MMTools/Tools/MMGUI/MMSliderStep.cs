using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/GUI/MMSliderStep")]
    [RequireComponent(typeof(Slider))]
    public class MMSliderStep : MonoBehaviour
    {
        [Header("Slider Step")]
        public float StepThreshold = 0.1f;
        public UnityEvent OnStep;

        protected Slider _slider;
        protected float _lastStep = 0f;
        protected virtual void OnEnable()
        {
            _slider = this.gameObject.GetComponent<Slider>();
            _slider.onValueChanged.AddListener(ValueChangeCheck);
        }
        protected virtual void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(ValueChangeCheck);
        }
        public virtual void ValueChangeCheck(float value)
        {
            if (Mathf.Abs(_slider.value - _lastStep) > StepThreshold)
            {
                _lastStep = _slider.value;
                OnStep?.Invoke();
            }
        }
    }
}
