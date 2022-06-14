using MoreMountains.Tools;
using UnityEngine;
using TMPro;
using System.Collections;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you tweak the softness of a TMP text over time.")]
    [FeedbackPath("TextMesh Pro/TMP Softness")]
    public class MMFeedbackTMPSoftness : MMFeedback
    {
        public override float FeedbackDuration { get { return (Mode == MMFeedbackBase.Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
        #endif

        [Header("Target")]
        [Tooltip("the TMP_Text component to control")]
        public TMP_Text TargetTMPText;

        [Header("Softness")]
        [Tooltip("whether or not values should be relative")]
        public bool RelativeValues = true;
        [Tooltip("the selected mode")]
        public MMFeedbackBase.Modes Mode = MMFeedbackBase.Modes.OverTime;
        [Tooltip("the duration of the feedback, in seconds")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float Duration = 0.5f;
        [Tooltip("the curve to tween on")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public MMTweenType SoftnessCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.3f, 1f), new Keyframe(1, 0f)));
        [Tooltip("the value to remap the curve's 0 to")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float RemapZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
        public float RemapOne = 1f;
        [Tooltip("the value to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
        public float InstantSoftness;
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;

        protected float _initialSoftness;
        protected Coroutine _coroutine;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);

            if (!Active)
            {
                return;
            }

            _initialSoftness = TargetTMPText.fontMaterial.GetFloat(ShaderUtilities.ID_FaceDilate);
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (TargetTMPText == null)
            {
                return;
            }

            if (Active)
            {
                switch (Mode)
                {
                    case MMFeedbackBase.Modes.Instant:
                        TargetTMPText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, InstantSoftness);
                        TargetTMPText.UpdateMeshPadding();
                        break;
                    case MMFeedbackBase.Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(ApplyValueOverTime());
                        break;
                }
            }
        }

        protected virtual IEnumerator ApplyValueOverTime()
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetValue(remappedTime);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetValue(FinalNormalizedTime);
            _coroutine = null;
            yield return null;
        }

        protected virtual void SetValue(float time)
        {
            float intensity = MMTween.Tween(time, 0f, 1f, RemapZero, RemapOne, SoftnessCurve);
            float newValue = intensity;
            if (RelativeValues)
            {
                newValue += _initialSoftness;
            }
            TargetTMPText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, newValue);
            TargetTMPText.UpdateMeshPadding();
        }
    }
}
