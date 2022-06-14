using MoreMountains.Tools;
using UnityEngine;
using TMPro;
using System.Collections;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the color of a target TMP over time.")]
    [FeedbackPath("TextMesh Pro/TMP Color")]
    public class MMFeedbackTMPColor : MMFeedback
    {
        public enum ColorModes { Instant, Gradient, Interpolate }
        public override float FeedbackDuration { get { return (ColorMode == ColorModes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
        #endif

        [Header("Target")]
        [Tooltip(" TMP_Text component to control")]
        public TMP_Text TargetTMPText;

        [Header("Color")]
        [Tooltip("the selected color mode :" +
            "None : nothing will happen," +
            "gradient : evaluates the color over time on that gradient, from left to right," +
            "interpolate : lerps from the current color to the destination one ")]
        public ColorModes ColorMode = ColorModes.Interpolate;
        [Tooltip("how long the color of the text should change over time")]
        [MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate, (int)ColorModes.Gradient)]
        public float Duration = 0.2f;
        [Tooltip("the color to apply")]
        [MMFEnumCondition("ColorMode", (int)ColorModes.Instant)]
        public Color InstantColor = Color.yellow;
        [Tooltip("the gradient to use to animate the color over time")]
        [MMFEnumCondition("ColorMode", (int)ColorModes.Gradient)]
        [GradientUsage(true)]
        public Gradient ColorGradient;
        [Tooltip("the destination color when in interpolate mode")]
        [MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
        public Color DestinationColor = Color.yellow;
        [Tooltip("the curve to use when interpolating towards the destination color")]
        [MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
        public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;

        protected Color _initialColor;
        protected Coroutine _coroutine;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);

            if (TargetTMPText == null)
            {
                return;
            }

            _initialColor = TargetTMPText.color;
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (TargetTMPText == null)
            {
                return;
            }

            if (Active)
            {
                switch (ColorMode)
                {
                    case ColorModes.Instant:
                        TargetTMPText.color = InstantColor;
                        break;
                    case ColorModes.Gradient:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(ChangeColor());
                        break;
                    case ColorModes.Interpolate:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(ChangeColor());
                        break;
                }
            }
        }
        protected virtual IEnumerator ChangeColor()
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetColor(remappedTime);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetColor(FinalNormalizedTime);
            _coroutine = null;
            yield break;
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
        protected virtual void SetColor(float time)
        {
            if (ColorMode == ColorModes.Gradient)
            {
                TargetTMPText.color = ColorGradient.Evaluate(time);
            }
            else if (ColorMode == ColorModes.Interpolate)
            {
                float factor = ColorCurve.Evaluate(time);
                TargetTMPText.color = Color.LerpUnclamped(_initialColor, DestinationColor, factor);
            }
        }
    }
}
