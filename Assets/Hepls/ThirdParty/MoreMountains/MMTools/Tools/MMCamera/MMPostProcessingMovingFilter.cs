using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    public struct MMPostProcessingMovingFilterEvent
    {
        public delegate void Delegate(MMTweenType curve, bool active, bool toggle, float duration, int channel = 0, bool stop = false);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(MMTweenType curve, bool active, bool toggle, float duration, int channel = 0, bool stop = false)
        {
            OnEvent?.Invoke(curve, active, toggle, duration, channel, stop);
        }
    }
    [AddComponentMenu("More Mountains/Tools/Camera/MMPostProcessingMovingFilter")]
    public class MMPostProcessingMovingFilter : MonoBehaviour
    {
        public enum TimeScales { Unscaled, Scaled }

        [Header("Settings")]
        public int Channel = 0;
        public TimeScales TimeScale = TimeScales.Unscaled;
        public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        public bool Active = false;

        [MMVector("On","Off")]
        public Vector2 FilterOffset = new Vector2(0f, 5f);
        public bool AddToInitialPosition = true;

        [Header("Tests")]
        public float TestDuration = 0.5f;
        [MMInspectorButton("PostProcessingToggle")]
        public bool PostProcessingToggleButton;
        [MMInspectorButton("PostProcessingTriggerOff")]
        public bool PostProcessingTriggerOffButton;
        [MMInspectorButton("PostProcessingTriggerOn")]
        public bool PostProcessingTriggerOnButton;

        protected bool _lastReachedState = false;
        protected float _duration = 2f;
        protected float _lastMovementStartedAt = 0f;
        protected Vector3 _initialPosition;
        protected Vector3 _newPosition;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _lastMovementStartedAt = 0f;

            if (AddToInitialPosition)
            {
                _initialPosition = this.transform.localPosition;
            }
            else
            {
                _initialPosition = Vector3.zero;
            }
            
            _newPosition = _initialPosition;
            _newPosition.y = Active ? _initialPosition.y + FilterOffset.x : _initialPosition.y + FilterOffset.y;            
            this.transform.localPosition = _newPosition;
            _lastReachedState = Active;
        }
        protected virtual void Update()
        {
            if (_lastReachedState == Active)
            {
                return;
            }
            MoveTowardsCurrentTarget();
        }
        protected virtual void MoveTowardsCurrentTarget()
        {
            if (_newPosition != this.transform.localPosition)
            {
                this.transform.localPosition = _newPosition;
            }

            float originY = Active ? _initialPosition.y + FilterOffset.y : _initialPosition.y + FilterOffset.x;
            float targetY = Active ? _initialPosition.y + FilterOffset.x : _initialPosition.y + FilterOffset.y;
            float currentTime = (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;

            _newPosition = this.transform.localPosition;
            _newPosition.y = MMTween.Tween(currentTime - _lastMovementStartedAt, 0f, _duration, originY, targetY, Curve);
          
            if (currentTime - _lastMovementStartedAt > _duration)
            {
                _newPosition.y = targetY;
                this.transform.localPosition = _newPosition;
                _lastReachedState = Active;
            }
        }
        public virtual void OnMMPostProcessingMovingFilterEvent(MMTweenType curve, bool active, bool toggle, float duration, int channel = 0, bool stop = false)
        {
            if ((channel != Channel) && (channel != -1) && (Channel != -1))
            {
                return;
            }
            
            if (stop)
            {
                _lastReachedState = Active;
                return;
            }
            
            Curve = curve;
            _duration = duration;

            if (toggle)
            {
                Active = !Active;
            }
            else
            {
                Active = active;
            }            

            float currentTime = (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;
            _lastMovementStartedAt = currentTime;
        }
        protected virtual void OnEnable()
        {
            MMPostProcessingMovingFilterEvent.Register(OnMMPostProcessingMovingFilterEvent);
        }
        protected virtual void OnDisable()
        {
            MMPostProcessingMovingFilterEvent.Unregister(OnMMPostProcessingMovingFilterEvent);
        }
        protected virtual void PostProcessingToggle()
        {
            MMPostProcessingMovingFilterEvent.Trigger(new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic), false, true, TestDuration, 0);
        }
        protected virtual void PostProcessingTriggerOff()
        {
            MMPostProcessingMovingFilterEvent.Trigger(new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic), false, false, TestDuration, 0);
        }
        protected virtual void PostProcessingTriggerOn()
        {
            MMPostProcessingMovingFilterEvent.Trigger(new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic), true, false, TestDuration, 0);
        }
    }
}
