using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you change the color of a target Image over time. You can also use it to command one or many MMImageShakers.")]
    [FeedbackPath("UI/Image")]
    public class MMFeedbackImage : MMFeedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
#endif
        public enum Modes { OverTime, Instant, ShakerEvent }

        [Header("Sprite Renderer")]
        [Tooltip("the Image to affect when playing the feedback")]
        public Image BoundImage;
        [Tooltip("whether the feedback should affect the Image instantly or over a period of time")]
        public Modes Mode = Modes.OverTime;
        [Tooltip("how long the Image should change over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float Duration = 0.2f;
        [Tooltip("whether or not that Image should be turned off on start")]
        public bool StartsOff = false;
        [Tooltip("the channel to broadcast on")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public int Channel = 0;
        [Tooltip("whether or not to reset shaker values after shake")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool ResetTargetValuesAfterShake = true;
        [Tooltip("whether or not to broadcast a range to only affect certain shakers")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool UseRange = false;
        [Tooltip("the range of the event, in units")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public float EventRange = 100f;
        [Tooltip("the transform to use to broadcast the event as origin point")]
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public Transform EventOriginTransform;
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;
        
        [Header("Color")]
        [Tooltip("whether or not to modify the color of the image")]
        public bool ModifyColor = true;
        [Tooltip("the colors to apply to the Image over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public Gradient ColorOverTime;
        [Tooltip("the color to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
        public Color InstantColor;
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        protected Coroutine _coroutine;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);

            if (EventOriginTransform == null)
            {
                EventOriginTransform = this.transform;
            }

            if (Active)
            {
                if (StartsOff)
                {
                    Turn(false);
                }
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                Turn(true);
                switch (Mode)
                {
                    case Modes.Instant:
                        if (ModifyColor)
                        {
                            BoundImage.color = InstantColor;
                        }
                        break;
                    case Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(ImageSequence());
                        break;
                    case Modes.ShakerEvent:
                        break;
                }
            }
        }
        protected virtual IEnumerator ImageSequence()
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;

            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetImageValues(remappedTime);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetImageValues(FinalNormalizedTime);
            if (StartsOff)
            {
                Turn(false);
            }
            _coroutine = null;
            yield return null;
        }
        protected virtual void SetImageValues(float time)
        {
            if (ModifyColor)
            {
                BoundImage.color = ColorOverTime.Evaluate(time);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                Turn(false);
            }
        }
        protected virtual void Turn(bool status)
        {
            BoundImage.gameObject.SetActive(status);
            BoundImage.enabled = status;
        }
    }
}
