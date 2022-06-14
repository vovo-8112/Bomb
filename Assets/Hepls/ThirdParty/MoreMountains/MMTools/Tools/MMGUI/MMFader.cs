using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    public struct MMFadeStopEvent
    {
        public int ID;
        
        public MMFadeStopEvent(int id = 0)
        {
            ID = id;
        }
        static MMFadeStopEvent e;
        public static void Trigger(int id = 0)
        {
            e.ID = id;
            MMEventManager.TriggerEvent(e);
        }
    }
    public struct MMFadeEvent
    {
        public int ID;
        public float Duration;
        public float TargetAlpha;
        public MMTweenType Curve;
        public bool IgnoreTimeScale;
        public Vector3 WorldPosition;
        public MMFadeEvent(float duration, float targetAlpha, MMTweenType tween, int id=0, 
            bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
        {
            ID = id;
            Duration = duration;
            TargetAlpha = targetAlpha;
            Curve = tween;
            IgnoreTimeScale = ignoreTimeScale;
            WorldPosition = worldPosition;
        }
        static MMFadeEvent e;
        public static void Trigger(float duration, float targetAlpha)
        {
            Trigger(duration, targetAlpha, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic));
        }
        public static void Trigger(float duration, float targetAlpha, MMTweenType tween, int id = 0, 
            bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
        {
            e.ID = id;
            e.Duration = duration;
            e.TargetAlpha = targetAlpha;
            e.Curve = tween;
            e.IgnoreTimeScale = ignoreTimeScale;
            e.WorldPosition = worldPosition;
            MMEventManager.TriggerEvent(e);
        }
    }
     
    public struct MMFadeInEvent
    {
        public int ID;
        public float Duration;
        public MMTweenType Curve;
        public bool IgnoreTimeScale;
        public Vector3 WorldPosition;
        public MMFadeInEvent(float duration, MMTweenType tween, int id = 0, 
            bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
        {
            ID = id;
            Duration = duration;
            Curve = tween;
            IgnoreTimeScale = ignoreTimeScale;
            WorldPosition = worldPosition;
        }
        static MMFadeInEvent e;
        public static void Trigger(float duration, MMTweenType tween, int id = 0, 
            bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
        {
            e.ID = id;
            e.Duration = duration;
            e.Curve = tween;
            e.IgnoreTimeScale = ignoreTimeScale;
            e.WorldPosition = worldPosition;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct MMFadeOutEvent
    {
        public int ID;
        public float Duration;
        public MMTweenType Curve;
        public bool IgnoreTimeScale;
        public Vector3 WorldPosition;
        public MMFadeOutEvent(float duration, MMTweenType tween, int id = 0, 
            bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
        {
            ID = id;
            Duration = duration;
            Curve = tween;
            IgnoreTimeScale = ignoreTimeScale;
            WorldPosition = worldPosition;
        }

        static MMFadeOutEvent e;
        public static void Trigger(float duration, MMTweenType tween, int id = 0, 
            bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
        {
            e.ID = id;
            e.Duration = duration;
            e.Curve = tween;
            e.IgnoreTimeScale = ignoreTimeScale;
            e.WorldPosition = worldPosition;
            MMEventManager.TriggerEvent(e);
        }
    }
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("More Mountains/Tools/GUI/MMFader")]
    public class MMFader : MonoBehaviour, MMEventListener<MMFadeEvent>, MMEventListener<MMFadeInEvent>, MMEventListener<MMFadeOutEvent>, MMEventListener<MMFadeStopEvent>
    {
        [Header("Identification")]
        public int ID;
        [Header("Opacity")]
        public float InactiveAlpha = 0f;
        public float ActiveAlpha = 1f;
        [Header("Timing")]
        public float DefaultDuration = 0.2f;
        public MMTweenType DefaultTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
        public bool IgnoreTimescale = true;
        [Header("Interaction")]
        public bool ShouldBlockRaycasts = false;

        [Header("Debug")]
        [MMInspectorButton("FadeIn1Second")]
        public bool FadeIn1SecondButton;
        [MMInspectorButton("FadeOut1Second")]
        public bool FadeOut1SecondButton;
        [MMInspectorButton("DefaultFade")]
        public bool DefaultFadeButton;
        [MMInspectorButton("ResetFader")]
        public bool ResetFaderButton;

        protected CanvasGroup _canvasGroup;
        protected Image _image;

        protected float _initialAlpha;
        protected float _currentTargetAlpha;
        protected float _currentDuration;
        protected MMTweenType _currentCurve;

        protected bool _fading = false;
        protected float _fadeStartedAt;
        protected bool _frameCountOne;
        protected virtual void ResetFader()
        {
            _canvasGroup.alpha = InactiveAlpha;
        }
        protected virtual void DefaultFade()
        {
            MMFadeEvent.Trigger(DefaultDuration, ActiveAlpha, DefaultTween, ID);
        }
        protected virtual void FadeIn1Second()
        {
            MMFadeInEvent.Trigger(1f, new MMTweenType(MMTween.MMTweenCurve.LinearTween));
        }
        protected virtual void FadeOut1Second()
        {
            MMFadeOutEvent.Trigger(1f, new MMTweenType(MMTween.MMTweenCurve.LinearTween));
        }
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = InactiveAlpha;

            _image = GetComponent<Image>();
            _image.enabled = false;
        }
        protected virtual void Update()
        {
            if (_canvasGroup == null) { return; }

            if (_fading)
            {
                Fade();
            }
        }
        protected virtual void Fade()
        {
            float currentTime = IgnoreTimescale ? Time.unscaledTime : Time.time;

            if (_frameCountOne)
            {
                if (Time.frameCount <= 2)
                {
                    _canvasGroup.alpha = _initialAlpha;
                    return;
                }
                _fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
                currentTime = _fadeStartedAt;
                _frameCountOne = false;
            }
                        
            float endTime = _fadeStartedAt + _currentDuration;
            if (currentTime - _fadeStartedAt < _currentDuration)
            {
                float result = MMTween.Tween(currentTime, _fadeStartedAt, endTime, _initialAlpha, _currentTargetAlpha, _currentCurve);
                _canvasGroup.alpha = result;
            }
            else
            {
                StopFading();
            }
        }
        protected virtual void StopFading()
        {
            _canvasGroup.alpha = _currentTargetAlpha;
            _fading = false;
            if (_canvasGroup.alpha == InactiveAlpha)
            {
                DisableFader();
            }
        }
        protected virtual void DisableFader()
        {
            _image.enabled = false;
            if (ShouldBlockRaycasts)
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }
        protected virtual void EnableFader()
        {
            _image.enabled = true;
            if (ShouldBlockRaycasts)
            {
                _canvasGroup.blocksRaycasts = true;
            }
        }
        protected virtual void StartFading(float initialAlpha, float endAlpha, float duration, MMTweenType curve, int id, bool ignoreTimeScale)
        {
            if (id != ID)
            {
                return;
            }
            IgnoreTimescale = ignoreTimeScale;
            EnableFader();
            _fading = true;
            _initialAlpha = initialAlpha;
            _currentTargetAlpha = endAlpha;
            _fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
            _currentCurve = curve;
            _currentDuration = duration;
            if (Time.frameCount == 1)
            {
                _frameCountOne = true;
            }
        }
        public virtual void OnMMEvent(MMFadeEvent fadeEvent)
        {
            _currentTargetAlpha = (fadeEvent.TargetAlpha == -1) ? ActiveAlpha : fadeEvent.TargetAlpha;
            StartFading(_canvasGroup.alpha, _currentTargetAlpha, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, fadeEvent.IgnoreTimeScale);
        }
        public virtual void OnMMEvent(MMFadeInEvent fadeEvent)
        {
            StartFading(InactiveAlpha, ActiveAlpha, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, fadeEvent.IgnoreTimeScale);
        }
        public virtual void OnMMEvent(MMFadeOutEvent fadeEvent)
        {
            StartFading(ActiveAlpha, InactiveAlpha, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, fadeEvent.IgnoreTimeScale);
        }
        public virtual void OnMMEvent(MMFadeStopEvent fadeStopEvent)
        {
            if (fadeStopEvent.ID == ID)
            {
                _fading = false;
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMFadeEvent>();
            this.MMEventStartListening<MMFadeStopEvent>();
            this.MMEventStartListening<MMFadeInEvent>();
            this.MMEventStartListening<MMFadeOutEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMFadeEvent>();
            this.MMEventStopListening<MMFadeStopEvent>();
            this.MMEventStopListening<MMFadeInEvent>();
            this.MMEventStopListening<MMFadeOutEvent>();
        }
    }
}
