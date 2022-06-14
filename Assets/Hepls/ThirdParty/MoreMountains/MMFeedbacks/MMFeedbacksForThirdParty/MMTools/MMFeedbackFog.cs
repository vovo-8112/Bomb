using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you animate the density, color, end and start distance of your scene's fog")]
    [FeedbackPath("Renderer/Fog")]
    public class MMFeedbackFog : MMFeedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
#endif
        public enum Modes { OverTime, Instant }

        [Header("Fog")]
        [Tooltip("whether the feedback should affect the sprite renderer instantly or over a period of time")]
        public Modes Mode = Modes.OverTime;
        [Tooltip("how long the sprite renderer should change over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public float Duration = 0.2f;
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;

        [Header("Fog Density")]
        [Tooltip("whether or not to modify the fog's density")]
        public bool ModifyFogDensity = true;
        [Tooltip("a curve to use to animate the fog's density over time")]
        public MMTweenType DensityCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        [Tooltip("the value to remap the fog's density curve zero value to")]
        public float DensityRemapZero = 0.01f;
        [Tooltip("the value to remap the fog's density curve one value to")]
        public float DensityRemapOne = 0.05f;
        [Tooltip("the value to change the fog's density to when in instant mode")]
        public float DensityInstantChange;
        
        [Header("Fog Start Distance")]
        [Tooltip("whether or not to modify the fog's start distance")]
        public bool ModifyStartDistance = true;
        [Tooltip("a curve to use to animate the fog's start distance over time")]
        public MMTweenType StartDistanceCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        [Tooltip("the value to remap the fog's start distance curve zero value to")]
        public float StartDistanceRemapZero = 0f;
        [Tooltip("the value to remap the fog's start distance curve one value to")]
        public float StartDistanceRemapOne = 0f;
        [Tooltip("the value to change the fog's start distance to when in instant mode")]
        public float StartDistanceInstantChange;
        
        [Header("Fog End Distance")]
        [Tooltip("whether or not to modify the fog's end distance")]
        public bool ModifyEndDistance = true;
        [Tooltip("a curve to use to animate the fog's end distance over time")]
        public MMTweenType EndDistanceCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        [Tooltip("the value to remap the fog's end distance curve zero value to")]
        public float EndDistanceRemapZero = 0f;
        [Tooltip("the value to remap the fog's end distance curve one value to")]
        public float EndDistanceRemapOne = 300f;
        [Tooltip("the value to change the fog's end distance to when in instant mode")]
        public float EndDistanceInstantChange;
        
        [Header("Fog Color")]
        [Tooltip("whether or not to modify the fog's color")]
        public bool ModifyColor = true;
        [Tooltip("the colors to apply to the sprite renderer over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public Gradient ColorOverTime;
        [Tooltip("the color to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public Color InstantColor;
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }
        
        protected Coroutine _coroutine;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                switch (Mode)
                {
                    case Modes.Instant:
                        if (ModifyColor)
                        {
                            RenderSettings.fogColor = InstantColor;
                        }

                        if (ModifyStartDistance)
                        {
                            RenderSettings.fogStartDistance = StartDistanceInstantChange;
                        }

                        if (ModifyEndDistance)
                        {
                            RenderSettings.fogEndDistance = EndDistanceInstantChange;
                        }

                        if (ModifyFogDensity)
                        {
                            RenderSettings.fogDensity = DensityInstantChange * intensityMultiplier;
                        }
                        break;
                    case Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }

                        _coroutine = StartCoroutine(FogSequence(intensityMultiplier));
                        break;
                }
            }
        }
        protected virtual IEnumerator FogSequence(float intensityMultiplier)
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetFogValues(remappedTime, intensityMultiplier);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetFogValues(FinalNormalizedTime, intensityMultiplier);    
            _coroutine = null;      
            yield return null;
        }
        protected virtual void SetFogValues(float time, float intensityMultiplier)
        {
            if (ModifyColor)
            {
                RenderSettings.fogColor = ColorOverTime.Evaluate(time); 
            }

            if (ModifyFogDensity)
            {
                RenderSettings.fogDensity = MMTween.Tween(time, 0f, 1f, DensityRemapZero, DensityRemapOne, DensityCurve) * intensityMultiplier;
            }

            if (ModifyStartDistance)
            {
                RenderSettings.fogStartDistance = MMTween.Tween(time, 0f, 1f, StartDistanceRemapZero, StartDistanceRemapOne, StartDistanceCurve);
            }

            if (ModifyEndDistance)
            {
                RenderSettings.fogEndDistance = MMTween.Tween(time, 0f, 1f, EndDistanceRemapZero, EndDistanceRemapOne, EndDistanceCurve);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active && (_coroutine != null))
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
}
