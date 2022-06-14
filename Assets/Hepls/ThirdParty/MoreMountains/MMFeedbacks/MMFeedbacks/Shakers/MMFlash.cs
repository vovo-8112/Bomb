using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System;

namespace MoreMountains.Feedbacks
{
    public struct MMFlashEvent
    {
        public delegate void Delegate(Color flashColor, float duration, float alpha, int flashID, int channel, TimescaleModes timescaleMode, bool stop = false);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(Color flashColor, float duration, float alpha, int flashID, int channel, TimescaleModes timescaleMode, bool stop = false)
        {
            OnEvent?.Invoke(flashColor, duration, alpha, flashID, channel, timescaleMode, stop);
        }
    }

    [Serializable]
    public class MMFlashDebugSettings
    {
        public int Channel = 0;
        public Color FlashColor = Color.white;
        public float FlashDuration = 0.2f;
        public float FlashAlpha = 1f;
        public int FlashID = 0;
    }
    
	[RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMFlash")]
    public class MMFlash : MonoBehaviour
    {
        [Header("Flash")]
        [Tooltip("the channel to receive flash events on")]
        public int Channel = 0;
        [Tooltip("the ID of this MMFlash object. When triggering a MMFlashEvent you can specify an ID, and only MMFlash objects with this ID will answer the call and flash, allowing you to have more than one flash object in a scene")]
        public int FlashID = 0;
        
        [Header("Debug")]
        [Tooltip("the set of test settings to use when pressing the DebugTest button")]
        public MMFlashDebugSettings DebugSettings;
        [Tooltip("a test button that calls the DebugTest method")]
        [MMFInspectorButton("DebugTest")]
        public bool DebugTestButton;

        public virtual float GetTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

        protected Image _image;
        protected CanvasGroup _canvasGroup;
		protected bool _flashing = false;
        protected float _targetAlpha;
        protected Color _initialColor;
        protected float _delta;
        protected float _flashStartedTimestamp;
        protected int _direction = 1;
        protected float _duration;
        protected TimescaleModes _timescaleMode;
		protected virtual void Start()
		{
			_image = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _initialColor = _image.color;
        }
		protected virtual void Update()
		{
			if (_flashing)
			{
				_image.enabled = true;

                if (GetTime() - _flashStartedTimestamp > _duration / 2f)
                {
                    _direction = -1;
                }

                if (_direction == 1)
                {
                    _delta += GetDeltaTime() / (_duration / 2f);
                }
                else
                {
                    _delta -= GetDeltaTime() / (_duration / 2f);
                }
                
                if (GetTime() - _flashStartedTimestamp > _duration)
                {
                    _flashing = false;
                }

                _canvasGroup.alpha = Mathf.Lerp(0f, _targetAlpha, _delta);
            }
			else
			{
				_image.enabled = false;
			}
		}

        public virtual void DebugTest()
        {
            MMFlashEvent.Trigger(DebugSettings.FlashColor, DebugSettings.FlashDuration, DebugSettings.FlashAlpha, DebugSettings.FlashID, DebugSettings.Channel, TimescaleModes.Unscaled);
        }
		public virtual void OnMMFlashEvent(Color flashColor, float duration, float alpha, int flashID, int channel, TimescaleModes timescaleMode, bool stop = false)
        {
            if (flashID != FlashID) 
            {
                return;
            }
            
            if (stop)
            {
	            _flashing = false;
	            return;
            }

            if ((channel != Channel) && (channel != -1) && (Channel != -1))
            {
                return;
            }

            if (!_flashing)
            {
                _flashing = true;
                _direction = 1;
                _canvasGroup.alpha = 0;
                _targetAlpha = alpha;
                _delta = 0f;
                _image.color = flashColor;
                _duration = duration;
                _timescaleMode = timescaleMode;
                _flashStartedTimestamp = GetTime();
            }
        }
		protected virtual void OnEnable()
		{
            MMFlashEvent.Register(OnMMFlashEvent);
		}
		protected virtual void OnDisable()
		{
            MMFlashEvent.Unregister(OnMMFlashEvent);
        }		
	}
}