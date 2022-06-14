using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you change the color of a target sprite renderer over time, and flip it on X or Y. You can also use it to command one or many MMSpriteRendererShakers.")]
    [FeedbackPath("Renderer/SpriteRenderer")]
    public class MMFeedbackSpriteRenderer : MMFeedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
#endif
        public enum Modes { OverTime, Instant, ShakerEvent, ToDestinationColor, ToDestinationColorAndBack }
        public enum InitialColorModes { InitialColorOnInit, InitialColorOnPlay }

        [Header("Sprite Renderer")]
        [Tooltip("the SpriteRenderer to affect when playing the feedback")]
        public SpriteRenderer BoundSpriteRenderer;
        [Tooltip("whether the feedback should affect the sprite renderer instantly or over a period of time")]
        public Modes Mode = Modes.OverTime;
        [Tooltip("how long the sprite renderer should change over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
        public float Duration = 0.2f;
        [Tooltip("whether or not that sprite renderer should be turned off on start")]
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
        [Tooltip("whether to grab the initial color to (potentially) go back to at init or when the feedback plays")] 
        public InitialColorModes InitialColorMode = InitialColorModes.InitialColorOnPlay;
        
        [Header("Color")]
        [Tooltip("whether or not to modify the color of the sprite renderer")]
        public bool ModifyColor = true;
        [Tooltip("the colors to apply to the sprite renderer over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public Gradient ColorOverTime;
        [Tooltip("the color to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
        public Color InstantColor;
        [Tooltip("the color to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
        public Color ToDestinationColor = Color.red;
        [Tooltip("the color to move to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ToDestinationColor, (int)Modes.ToDestinationColorAndBack)]
        public AnimationCurve ToDestinationColorCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));
        
        [Header("Flip")]
        [Tooltip("whether or not to flip the sprite on X")]
        public bool FlipX = false;
        [Tooltip("whether or not to flip the sprite on Y")]
        public bool FlipY = false;
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        protected Coroutine _coroutine;
        protected Color _initialColor;
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

            if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnInit))
            {
                _initialColor = BoundSpriteRenderer.color;
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                if ((BoundSpriteRenderer != null) && (InitialColorMode == InitialColorModes.InitialColorOnPlay))
                {
                    _initialColor = BoundSpriteRenderer.color;
                }
                
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                Turn(true);
                switch (Mode)
                {
                    case Modes.Instant:
                        if (ModifyColor)
                        {
                            BoundSpriteRenderer.color = InstantColor;
                        }
                        Flip();
                        break;
                    case Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(SpriteRendererSequence());
                        break;
                    case Modes.ShakerEvent:
                        MMSpriteRendererShakeEvent.Trigger(FeedbackDuration, ModifyColor, ColorOverTime, 
                            FlipX, FlipY,   
                            intensityMultiplier,
                            Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
                            UseRange, EventRange, EventOriginTransform.position);
                        break;
                    case Modes.ToDestinationColor:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(SpriteRendererToDestinationSequence(false));
                        break;
                    case Modes.ToDestinationColorAndBack:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(SpriteRendererToDestinationSequence(true));
                        break;
                }
            }
        }
        protected virtual IEnumerator SpriteRendererSequence()
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            Flip();
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetSpriteRendererValues(remappedTime);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetSpriteRendererValues(FinalNormalizedTime);
            if (StartsOff)
            {
                Turn(false);
            }            
            _coroutine = null;    
            yield return null;
        }
        protected virtual IEnumerator SpriteRendererToDestinationSequence(bool andBack)
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            Flip();
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                if (andBack)
                {
                    remappedTime = (remappedTime < 0.5f)
                        ? MMFeedbacksHelpers.Remap(remappedTime, 0f, 0.5f, 0f, 1f)
                        : MMFeedbacksHelpers.Remap(remappedTime, 0.5f, 1f, 1f, 0f);
                }
                
                float evalTime = ToDestinationColorCurve.Evaluate(remappedTime);
                
                if (ModifyColor)
                {
                    BoundSpriteRenderer.color = Color.LerpUnclamped(_initialColor, ToDestinationColor, evalTime);
                }

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            if (ModifyColor)
            {
                BoundSpriteRenderer.color = andBack ? _initialColor : ToDestinationColor;
            }
            if (StartsOff)
            {
                Turn(false);
            }            
            _coroutine = null;    
            yield return null;
        }
        protected virtual void Flip()
        {
            if (FlipX)
            {
                BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
            }
            if (FlipY)
            {
                BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
            }
        }
        protected virtual void SetSpriteRendererValues(float time)
        {
            if (ModifyColor)
            {
                BoundSpriteRenderer.color = ColorOverTime.Evaluate(time);
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
        protected virtual void Turn(bool status)
        {
            BoundSpriteRenderer.gameObject.SetActive(status);
            BoundSpriteRenderer.enabled = status;
        }
    }
}
