using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

namespace MoreMountains.Feedbacks
{
	public enum MMTimeScaleMethods
	{
		For,
		Reset,
        Unfreeze
	}
	public struct TimeScaleProperties
	{
		public float TimeScale;
		public float Duration;
		public bool Lerp;
		public float LerpSpeed;
        public bool Infinite;
	}

    public struct MMTimeScaleEvent
    {
        public delegate void Delegate(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite)
        {
            OnEvent?.Invoke(timeScaleMethod, timeScale, duration, lerp, lerpSpeed, infinite);
        }
    }
    
    public struct MMFreezeFrameEvent
    {
        public delegate void Delegate(float duration);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(float duration)
        {
            OnEvent?.Invoke(duration);
        }
    }
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMTimeManager")]
    public class MMTimeManager : MonoBehaviour
	{		
		[MMFInformationAttribute("Put this component in your scene and it'll catch MMFreezeFrameEvents and MMTimeScaleEvents, allowing you to control the flow of time.", MMFInformationAttribute.InformationType.Info, false)]
		[Tooltip("The reference timescale, to which the system will go back to after all time is changed")]
		public float NormalTimescale = 1f;
		[Tooltip("the current, real time, time scale")]
		[MMFReadOnly]
		public float CurrentTimeScale = 1f;
		[Tooltip("the time scale the system is lerping towards")]
		[MMFReadOnly]
		public float TargetTimeScale = 1f;
		[Tooltip("whether or not the timescale should be lerping")]
		[MMFReadOnly]
		public bool LerpTimescale = true;
		[Tooltip("the speed at which the timescale should lerp towards its target")]
		[MMFReadOnly]
		public float LerpSpeed;

		[MMFInspectorButtonAttribute("TestButtonToSlowDownTime")]
		public bool TestButton;

		protected Stack<TimeScaleProperties> _timeScaleProperties;
		protected float _frozenTimeLeft = -1f;
		protected TimeScaleProperties _currentProperty;
        protected float _initialFixedDeltaTime = 0f;
        protected float _initialMaximumDeltaTime = 0f;
        protected virtual void TestButtonToSlowDownTime()
		{
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0.5f, 3f, true, 1f, false);
		}
		protected virtual void Start()
		{
            Initialization();
		}

        public virtual void Initialization()
        {
            TargetTimeScale = NormalTimescale;
            _timeScaleProperties = new Stack<TimeScaleProperties>();
            _initialFixedDeltaTime = Time.fixedDeltaTime;
            _initialMaximumDeltaTime = Time.maximumDeltaTime;
            ApplyTimeScale(NormalTimescale);
        }
		protected virtual void Update()
		{
            if (_timeScaleProperties.Count > 0)
			{
				_currentProperty = _timeScaleProperties.Peek();
				TargetTimeScale = _currentProperty.TimeScale;
                LerpSpeed = _currentProperty.LerpSpeed;
				LerpTimescale = _currentProperty.Lerp;
				_currentProperty.Duration -= Time.unscaledDeltaTime;

				_timeScaleProperties.Pop();
				_timeScaleProperties.Push(_currentProperty);

				if (_currentProperty.Duration <= 0f && !_currentProperty.Infinite)
				{
					Unfreeze();
				}
			}
			else
			{
				TargetTimeScale = NormalTimescale;
            }
            if (LerpTimescale)
			{
                ApplyTimeScale(Mathf.Lerp(Time.timeScale, TargetTimeScale, Time.unscaledDeltaTime * LerpSpeed));
			}
			else
			{
                ApplyTimeScale(TargetTimeScale);
			}

		}
        protected virtual void ApplyTimeScale(float newValue)
        {
            Time.timeScale = newValue;

            if (newValue != 0)
            {
	            Time.fixedDeltaTime = _initialFixedDeltaTime * newValue;            
            }
            Time.maximumDeltaTime = _initialMaximumDeltaTime * newValue;

            CurrentTimeScale = Time.timeScale;
        }
		protected virtual void SetTimeScale(float newTimeScale)
		{
			_timeScaleProperties.Clear();
            ApplyTimeScale(newTimeScale);
		}
		protected virtual void SetTimeScale(TimeScaleProperties timeScaleProperties)
        {
            _timeScaleProperties.Push(timeScaleProperties);
		}
		protected virtual void ResetTimeScale()
		{
            SetTimeScale(NormalTimescale);
		}
		protected virtual void Unfreeze()
        {
            if (_timeScaleProperties.Count > 0)
			{
                _timeScaleProperties.Pop();
			}
            else
            {
                ResetTimeScale();
            }
		}
		public virtual void SetTimescaleTo(float newNormalTimeScale)
		{
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, newNormalTimeScale, 0f, false, 0f, true);
		}
		public virtual void OnTimeScaleEvent(MMTimeScaleMethods timeScaleMethod, float timeScale, float duration, bool lerp, float lerpSpeed, bool infinite)
        {
            TimeScaleProperties timeScaleProperty = new TimeScaleProperties();
            timeScaleProperty.TimeScale = timeScale;
            timeScaleProperty.Duration = duration;
            timeScaleProperty.Lerp = lerp;
            timeScaleProperty.LerpSpeed = lerpSpeed;
            timeScaleProperty.Infinite = infinite;

            switch (timeScaleMethod)
            {
				case MMTimeScaleMethods.Reset:
					ResetTimeScale ();
					break;

				case MMTimeScaleMethods.For:
					SetTimeScale (timeScaleProperty);
					break;

                case MMTimeScaleMethods.Unfreeze:
                    Unfreeze();
                    break;
			}
		}
		public virtual void OnMMFreezeFrameEvent(float duration)
		{
			_frozenTimeLeft = duration;

			TimeScaleProperties properties = new TimeScaleProperties();
			properties.Duration = duration;
			properties.Lerp = false;
			properties.LerpSpeed = 0f;
			properties.TimeScale = 0f;

			SetTimeScale(properties);
		}
		void OnEnable()
		{
			MMFreezeFrameEvent.Register(OnMMFreezeFrameEvent);
            MMTimeScaleEvent.Register(OnTimeScaleEvent);
		}
		void OnDisable()
        {
            MMFreezeFrameEvent.Unregister(OnMMFreezeFrameEvent);
            MMTimeScaleEvent.Unregister(OnTimeScaleEvent);
        }		
	}
}
