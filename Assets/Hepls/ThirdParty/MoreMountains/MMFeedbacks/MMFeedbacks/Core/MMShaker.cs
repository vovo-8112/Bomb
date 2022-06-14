using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.Feedbacks
{
    public class MMShaker : MonoBehaviour
    {
        [Header("Shake Settings")]
        [Tooltip("the channel to listen to - has to match the one on the feedback")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float ShakeDuration = 0.2f;
        [Tooltip("if this is true this shaker will play on awake")]
        public bool PlayOnAwake = false;
        [Tooltip("if this is true, a new shake can happen while shaking")]
        public bool Interruptible = true;
        [Tooltip("if this is true, this shaker will always reset target values, regardless of how it was called")]
        public bool AlwaysResetTargetValuesAfterShake = false;
        [Tooltip("whether or not this shaker is shaking right now")]
        [MMFReadOnly]
        public bool Shaking = false;
        
        [HideInInspector] 
        public bool ForwardDirection = true;

        [HideInInspector] 
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
        public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
        public bool ListeningToEvents => _listeningToEvents;

        [HideInInspector]
        internal bool _listeningToEvents = false;
        protected float _shakeStartedTimestamp;
        protected float _remappedTimeSinceStart;
        protected bool _resetShakerValuesAfterShake;
        protected bool _resetTargetValuesAfterShake;
        protected float _journey;
        protected virtual void Awake()
        {
            Shaking = false;
            Initialization();
            if (!_listeningToEvents)
            {
                StartListening();
            }
            this.enabled = PlayOnAwake;
        }
        protected virtual void Initialization()
        {
        }
        public virtual void StartShaking()
        {
            _journey = ForwardDirection ? 0f : ShakeDuration;
            
            if (Shaking)
            {
                return;
            }
            else
            {
                this.enabled = true;
                _shakeStartedTimestamp = GetTime();
                Shaking = true;
                GrabInitialValues();
                ShakeStarts();
            }
        }
        protected virtual void ShakeStarts()
        {

        }
        protected virtual void GrabInitialValues()
        {

        }
        protected virtual void Update()
        {
            if (Shaking)
            {
                Shake();
                _journey += ForwardDirection ? GetDeltaTime() : -GetDeltaTime();
            }

            if (Shaking && ((_journey < 0) || (_journey > ShakeDuration)))
            {
                Shaking = false;
                ShakeComplete();
            }
        }
        protected virtual void Shake()
        {

        }
        protected virtual float ShakeFloat(AnimationCurve curve, float remapMin, float remapMax, bool relativeIntensity, float initialValue)
        {
            float newValue = 0f;
            
            float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
            
            float curveValue = curve.Evaluate(remappedTime);
            newValue = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, remapMin, remapMax);
            if (relativeIntensity)
            {
                newValue += initialValue;
            }
            return newValue;
        }
        protected virtual void ResetTargetValues()
        {

        }
        protected virtual void ResetShakerValues()
        {

        }
        protected virtual void ShakeComplete()
        {
            if (_resetTargetValuesAfterShake || AlwaysResetTargetValuesAfterShake)
            {
                ResetTargetValues();
            }   
            if (_resetShakerValuesAfterShake)
            {
                ResetShakerValues();
            }            
            this.enabled = false;
        }
        protected virtual void OnEnable()
        {
            StartShaking();
        }
        protected virtual void OnDestroy()
        {
            StopListening();
        }
        protected virtual void OnDisable()
        {
            if (Shaking)
            {
                ShakeComplete();
            }
        }
        public virtual void Play()
        {
            this.enabled = true;
        }
        public virtual void Stop()
        {
            Shaking = false;
            ShakeComplete();
        }
        public virtual void StartListening()
        {
            _listeningToEvents = true;
        }
        public virtual void StopListening()
        {
            _listeningToEvents = false;
        }
        protected virtual bool CheckEventAllowed(int channel, bool useRange = false, float range = 0f, Vector3 eventOriginPosition = default(Vector3))
        {
            if ((channel != Channel) && (channel != -1) && (Channel != -1))
            {
                return false;
            }
            if (!this.gameObject.activeInHierarchy)
            {
                return false;
            }
            else
            {
                if (useRange)
                {
                    if (Vector3.Distance(this.transform.position, eventOriginPosition) > range)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
