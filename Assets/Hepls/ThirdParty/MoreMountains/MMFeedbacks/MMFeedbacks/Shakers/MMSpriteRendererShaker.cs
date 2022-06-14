using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Renderer/MMSpriteRendererShaker")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MMSpriteRendererShaker : MMShaker
    {
        [Header("SpriteRenderer")]
        [Tooltip("the SpriteRenderer to affect when playing the feedback")]
        public SpriteRenderer BoundSpriteRenderer;
        [Tooltip("whether or not that SpriteRenderer should be turned off on start")]
        public bool StartsOff = true;

        [Header("Color")]
        [Tooltip("whether or not this shaker should modify color")]
        public bool ModifyColor = true;
        [Tooltip("the colors to apply to the SpriteRenderer over time")]
        public Gradient ColorOverTime;

        [Header("Flip")]
        [Tooltip("whether or not to flip the sprite on X")]
        public bool FlipX = false;
        [Tooltip("whether or not to flip the sprite on Y")]
        public bool FlipY = false;

        protected Color _initialColor;
        protected bool _originalModifyColor;
        protected float _originalShakeDuration;
        protected Gradient _originalColorOverTime;
        protected bool _originalFlipX;
        protected bool _originalFlipY;
        protected override void Initialization()
        {
            base.Initialization();
            if (BoundSpriteRenderer == null)
            {
                BoundSpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            }
        }
        protected virtual void Reset()
        {
            ShakeDuration = 1f;
        }
        protected override void Shake()
        {
            if (ModifyColor)
            {
                _remappedTimeSinceStart = MMFeedbacksHelpers.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
                BoundSpriteRenderer.color = ColorOverTime.Evaluate(_remappedTimeSinceStart);
            }            
        }
        protected override void GrabInitialValues()
        {
            _initialColor = BoundSpriteRenderer.color;
            _originalFlipX = BoundSpriteRenderer.flipX;
            _originalFlipY = BoundSpriteRenderer.flipY;
        }
        protected override void ResetTargetValues()
        {
            base.ResetTargetValues();
            BoundSpriteRenderer.color = _initialColor;
            BoundSpriteRenderer.flipX = _originalFlipX;
            BoundSpriteRenderer.flipY = _originalFlipY;
        }
        protected override void ResetShakerValues()
        {
            base.ResetShakerValues();
            ModifyColor = _originalModifyColor;
            ShakeDuration = _originalShakeDuration;
            ColorOverTime = _originalColorOverTime;
        }
        public override void StartListening()
        {
            base.StartListening();
            MMSpriteRendererShakeEvent.Register(OnMMSpriteRendererShakeEvent);
        }
        public override void StopListening()
        {
            base.StopListening();
            MMSpriteRendererShakeEvent.Unregister(OnMMSpriteRendererShakeEvent);
        }

        public virtual void OnMMSpriteRendererShakeEvent(float shakeDuration, bool modifyColor, Gradient colorOverTime,
            bool flipX, bool flipY,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
            bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
        {
            if (!CheckEventAllowed(channel, useRange, eventRange, eventOriginPosition) ||  (!Interruptible && Shaking))
            {
                return;
            }

            _resetShakerValuesAfterShake = resetShakerValuesAfterShake;
            _resetTargetValuesAfterShake = resetTargetValuesAfterShake;

            if (resetShakerValuesAfterShake)
            {
                _originalModifyColor = ModifyColor;
                _originalShakeDuration = ShakeDuration;
                _originalColorOverTime = ColorOverTime;
            }

            ModifyColor = modifyColor;
            ShakeDuration = shakeDuration;
            ColorOverTime = colorOverTime;
            FlipX = flipX;
            FlipY = flipY;

            if (FlipX)
            {
                BoundSpriteRenderer.flipX = !BoundSpriteRenderer.flipX;
            }
            if (FlipY)
            {
                BoundSpriteRenderer.flipY = !BoundSpriteRenderer.flipY;
            }            

            Play();
        }
    }
    public struct MMSpriteRendererShakeEvent
    {
        public delegate void Delegate(float shakeDuration, bool modifyColor, Gradient colorOverTime,
            bool flipX, bool flipY,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
            bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(float shakeDuration, bool modifyColor, Gradient colorOverTime,
            bool flipX, bool flipY,
            float feedbacksIntensity = 1.0f, int channel = 0, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
            bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
        {
            OnEvent?.Invoke(shakeDuration, modifyColor, colorOverTime,
                flipX, flipY,
                feedbacksIntensity, channel, resetShakerValuesAfterShake, resetTargetValuesAfterShake, 
                useRange, eventRange, eventOriginPosition);
        }
    }
}
