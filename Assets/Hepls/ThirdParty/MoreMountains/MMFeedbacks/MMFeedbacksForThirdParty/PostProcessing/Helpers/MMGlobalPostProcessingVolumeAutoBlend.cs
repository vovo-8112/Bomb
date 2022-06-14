using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Rendering.PostProcessing;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMGlobalPostProcessingVolumeAutoBlend")]
    [RequireComponent(typeof(PostProcessVolume))]
    public class MMGlobalPostProcessingVolumeAutoBlend : MonoBehaviour
    {
        public enum TimeScales { Scaled, Unscaled }
        public enum BlendTriggerModes { OnEnable, Script }

        [Header("Blend")]
        [Tooltip("the trigger mode for this MMGlobalPostProcessingVolumeAutoBlend")]
        public BlendTriggerModes BlendTriggerMode = BlendTriggerModes.OnEnable;
        [Tooltip("the duration of the blend (in seconds)")]
        public float BlendDuration = 1f;
        [Tooltip("the curve to use to blend")]
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));

        [Header("Weight")]
        [Tooltip("the weight at the start of the blend")]
        [Range(0f, 1f)]
        public float InitialWeight = 0f;
        [Tooltip("the desired weight at the end of the blend")]
        [Range(0f, 1f)]
        public float FinalWeight = 1f;

        [Header("Behaviour")]
        [Tooltip("the timescale to operate on")]
        public TimeScales TimeScale = TimeScales.Unscaled;
        [Tooltip("whether or not the associated volume should be disabled at 0")]
        public bool DisableVolumeOnZeroWeight = true;
        [Tooltip("whether or not this blender should disable itself at 0")]
        public bool DisableSelfAfterEnd = true;
        [Tooltip("whether or not this blender can be interrupted")]
        public bool Interruptable = true;
        [Tooltip("whether or not this blender should pick the current value as its starting point")]
        public bool StartFromCurrentValue = true;
        [Tooltip("reset to initial value on end ")]
        public bool ResetToInitialValueOnEnd = false;

        [Header("Tests")]
        [Tooltip("test blend button")]
        [MMFInspectorButton("Blend")]
        public bool TestBlend;
        [Tooltip("test blend back button")]
        [MMFInspectorButton("BlendBack")]
        public bool TestBlendBackwards;
        protected float GetTime()
        {
            return (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;
        }

        protected float _initial;
        protected float _destination;
        protected float _startTime;
        protected bool _blending = false;
        protected PostProcessVolume _volume;
        protected virtual void Awake()
        {
            _volume = this.gameObject.GetComponent<PostProcessVolume>();
            _volume.weight = InitialWeight;
        }
        protected virtual void OnEnable()
        {
            if ((BlendTriggerMode == BlendTriggerModes.OnEnable) && !_blending)
            {
                Blend();
            }
        }
        public virtual void Blend()
        {
            if (_blending && !Interruptable)
            {
                return;
            }
            _initial = StartFromCurrentValue ? _volume.weight : InitialWeight;
            _destination = FinalWeight;
            StartBlending();
        }
        public virtual void BlendBack()
        {
            if (_blending && !Interruptable)
            {
                return;
            }
            _initial = StartFromCurrentValue ? _volume.weight : FinalWeight;
            _destination = InitialWeight;
            StartBlending();
        }
        protected virtual void StartBlending()
        {
            _startTime = GetTime();
            _blending = true;
            this.enabled = true;
            if (DisableVolumeOnZeroWeight)
            {
                _volume.enabled = true;
            }
        }
        public virtual void StopBlending()
        {
            _blending = false;
        }
        protected virtual void Update()
        {
            if (!_blending)
            {
                return;
            }

            float timeElapsed = (GetTime() - _startTime);
            if (timeElapsed < BlendDuration)
            {                
                float remapped = MMFeedbacksHelpers.Remap(timeElapsed, 0f, BlendDuration, 0f, 1f);
                _volume.weight = Mathf.LerpUnclamped(_initial, _destination, Curve.Evaluate(remapped));
            }
            else
            {
                _volume.weight = ResetToInitialValueOnEnd ? _initial : _destination;
                _blending = false;
                if (DisableVolumeOnZeroWeight && (_volume.weight == 0f))
                {
                    _volume.enabled = false;
                }
                if (DisableSelfAfterEnd)
                {
                    this.enabled = false;
                }
            }            
        }
    }
}
