using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Animation/MMAnimationParameter")]
    public class MMAnimationParameter : MonoBehaviour
    {
        public string ParameterName;
        public Animator TargetAnimator;

        protected int _parameter;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _parameter = Animator.StringToHash(ParameterName);
        }
        public virtual void SetTrigger()
        {
            TargetAnimator.SetTrigger(_parameter);
        }
        public virtual void SetInt(int value)
        {
            TargetAnimator.SetInteger(_parameter, value);
        }
        public virtual void SetFloat(float value)
        {
            TargetAnimator.SetFloat(_parameter, value);
        }
        public virtual void SetBool(bool value)
        {
            TargetAnimator.SetBool(_parameter, value);
        }
    }
}
