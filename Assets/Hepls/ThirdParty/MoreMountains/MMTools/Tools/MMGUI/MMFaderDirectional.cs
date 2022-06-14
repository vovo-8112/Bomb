using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Tools/GUI/MMFaderDirectional")]
    public class MMFaderDirectional : MonoBehaviour, MMEventListener<MMFadeEvent>, MMEventListener<MMFadeInEvent>, MMEventListener<MMFadeOutEvent>, MMEventListener<MMFadeStopEvent>
    {
        public enum Directions { TopToBottom, LeftToRight, RightToLeft, BottomToTop }

        [Header("Identification")]
        [Tooltip("the ID for this fader (0 is default), set more IDs if you need more than one fader")]
        public int ID;

        [Header("Directional Fader")]
        [Tooltip("the direction this fader should move in when fading in")]
        public Directions FadeInDirection = Directions.LeftToRight;
        [Tooltip("the direction this fader should move in when fading out")]
        public Directions FadeOutDirection = Directions.LeftToRight;
        
        [Header("Timing")]
        [Tooltip("the default duration of the fade in/out")]
        public float DefaultDuration = 0.2f;
        [Tooltip("the default curve to use for this fader")]
        public MMTweenType DefaultTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
        [Tooltip("whether or not the fade should happen in unscaled time")]
        public bool IgnoreTimescale = true;
        [Tooltip("whether or not to automatically disable this fader on init")]
        public bool DisableOnInit = true;

        [Header("Delay")]
        [Tooltip("a delay (in seconds) to apply before playing this fade")]
        public float InitialDelay = 0f;

        [Header("Interaction")]
        [Tooltip("whether or not the fader should block raycasts when visible")]
        public bool ShouldBlockRaycasts = false;
        public virtual float Width { get { return _rectTransform.rect.width; } }
        public virtual float Height { get { return _rectTransform.rect.height; } }

        [Header("Debug")]
        [MMInspectorButton("FadeIn1Second")]
        public bool FadeIn1SecondButton;
        [MMInspectorButton("FadeOut1Second")]
        public bool FadeOut1SecondButton;
        [MMInspectorButton("DefaultFade")]
        public bool DefaultFadeButton;
        [MMInspectorButton("ResetFader")]
        public bool ResetFaderButton;

        protected RectTransform _rectTransform;
        protected CanvasGroup _canvasGroup;
        protected float _currentDuration;
        protected MMTweenType _currentCurve;
        protected bool _fading = false;
        protected float _fadeStartedAt;
        protected Vector2 _initialPosition;

        protected Vector2 _fromPosition;
        protected Vector2 _toPosition;
        protected Vector2 _newPosition;
        protected bool _active;
        protected bool _initialized = false;
        protected virtual void ResetFader()
        {
            _rectTransform.anchoredPosition = _initialPosition;
        }
        protected virtual void DefaultFade()
        {
            MMFadeEvent.Trigger(DefaultDuration, 1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
        }
        protected virtual void FadeIn1Second()
        {
            MMFadeInEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
        }
        protected virtual void FadeOut1Second()
        {
            MMFadeOutEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
        }
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
            _rectTransform = this.gameObject.GetComponent<RectTransform>();
            _initialPosition = _rectTransform.anchoredPosition;
            if (DisableOnInit)
            {
                DisableFader();
            }
            _initialized = true;
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
            float endTime = _fadeStartedAt + _currentDuration;

            if (currentTime - _fadeStartedAt < _currentDuration)
            {
                _newPosition = MMTween.Tween(currentTime, _fadeStartedAt, endTime, _fromPosition, _toPosition, _currentCurve);
                _rectTransform.anchoredPosition = _newPosition;
            }
            else
            {
                StopFading();
            }
        }
        protected virtual void StopFading()
        {
            _rectTransform.anchoredPosition = _toPosition;
            _fading = false;

            if (_initialPosition != _toPosition)
            {
                DisableFader();
            }
        }
        protected virtual IEnumerator StartFading(bool fadingIn, float duration, MMTweenType curve, int id,
            bool ignoreTimeScale, Vector3 worldPosition)
        {
            if (id != ID)
            {
                yield break;
            }

            if (InitialDelay > 0f)
            {
                yield return MMCoroutine.WaitFor(InitialDelay);
            }

            if (!_initialized)
            {
                Initialization();
            }

            if (curve == null)
            {
                curve = DefaultTween;
            }
            
            IgnoreTimescale = ignoreTimeScale;
            EnableFader();
            _fading = true;

            _fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
            _currentCurve = curve;
            _currentDuration = duration;

            _fromPosition = _rectTransform.anchoredPosition;
            _toPosition = fadingIn ? _initialPosition : ExitPosition();

            _newPosition = MMTween.Tween(0f, 0f, duration, _fromPosition, _toPosition, _currentCurve);
            _rectTransform.anchoredPosition = _newPosition;
        }
        protected virtual Vector2 BeforeEntryPosition()
        {
            switch (FadeInDirection)
            {
                case Directions.BottomToTop:
                    return _initialPosition + Vector2.down * Height;
                case Directions.LeftToRight:
                    return _initialPosition + Vector2.left * Width;
                case Directions.RightToLeft:
                    return _initialPosition + Vector2.right * Width;
                case Directions.TopToBottom:
                    return _initialPosition + Vector2.up * Height;
            }
            return Vector2.zero;
        }
        protected virtual Vector2 ExitPosition()
        {
            switch (FadeOutDirection)
            {
                case Directions.BottomToTop:
                    return _initialPosition + Vector2.up * Height;
                case Directions.LeftToRight:
                    return _initialPosition + Vector2.right * Width;
                case Directions.RightToLeft:
                    return _initialPosition + Vector2.left * Width;
                case Directions.TopToBottom:
                    return _initialPosition + Vector2.down * Height;
            }
            return Vector2.zero;
        }
        protected virtual void DisableFader()
        {
            if (ShouldBlockRaycasts)
            {
                _canvasGroup.blocksRaycasts = false;
            }
            _active = false;
            _canvasGroup.alpha = 0;
            _rectTransform.anchoredPosition = BeforeEntryPosition();
            this.enabled = false;
        }
        protected virtual void EnableFader()
        {
            this.enabled = true;
            if (ShouldBlockRaycasts)
            {
                _canvasGroup.blocksRaycasts = true;
            }
            _active = true;
            _canvasGroup.alpha = 1;
        }
        public virtual void OnMMEvent(MMFadeEvent fadeEvent)
        {
            bool status = _active ? false : true;
            StartCoroutine(StartFading(status, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
                fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
        }
        public virtual void OnMMEvent(MMFadeInEvent fadeEvent)
        {
            StartCoroutine(StartFading(true, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
                fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
        }
        public virtual void OnMMEvent(MMFadeOutEvent fadeEvent)
        {
            StartCoroutine(StartFading(false, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
                fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
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
        protected virtual void OnDestroy()
        {
            this.MMEventStopListening<MMFadeEvent>();
            this.MMEventStopListening<MMFadeStopEvent>();
            this.MMEventStopListening<MMFadeInEvent>();
            this.MMEventStopListening<MMFadeOutEvent>();
        }
    }
}

