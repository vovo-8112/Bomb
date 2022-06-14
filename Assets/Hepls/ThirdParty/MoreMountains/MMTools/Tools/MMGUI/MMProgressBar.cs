using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
    [MMRequiresConstantRepaint]
    [AddComponentMenu("More Mountains/Tools/GUI/MMProgressBar")]
    public class MMProgressBar : MMMonoBehaviour
	{
		public enum MMProgressBarStates {Idle, Decreasing, Increasing, InDecreasingDelay, InIncreasingDelay }
        public enum FillModes { LocalScale, FillAmount, Width, Height, Anchor }
        public enum BarDirections { LeftToRight, RightToLeft, UpToDown, DownToUp }
        public enum TimeScales { UnscaledTime, Time }
        public enum BarFillModes { SpeedBased, FixedDuration }
        
        [MMInspectorGroup("Bindings", true, 10)]
        public string PlayerID;
        public Transform ForegroundBar;
        [FormerlySerializedAs("DelayedBar")] 
        public Transform DelayedBarDecreasing;
        public Transform DelayedBarIncreasing;

        
        [MMInspectorGroup("Fill Settings", true, 11)]
        [FormerlySerializedAs("StartValue")] 
        [Range(0f,1f)]
        public float MinimumBarFillValue = 0f;
        [FormerlySerializedAs("EndValue")] 
        [Range(0f,1f)]
        public float MaximumBarFillValue = 1f;
        public bool SetInitialFillValueOnStart = false;
        [MMCondition("SetInitialFillValueOnStart", true)]
        [Range(0f,1f)]
        public float InitialFillValue = 0f;
        public BarDirections BarDirection = BarDirections.LeftToRight;
        public FillModes FillMode = FillModes.LocalScale;
        public TimeScales TimeScale = TimeScales.UnscaledTime;
        public BarFillModes BarFillMode = BarFillModes.SpeedBased;

        [MMInspectorGroup("Foreground Bar Settings", true, 12)]
		public bool LerpForegroundBar = true;
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarSpeedDecreasing = 15f;
		[FormerlySerializedAs("LerpForegroundBarSpeed")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarSpeedIncreasing = 15f;
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarDurationDecreasing = 0.2f;
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarDurationIncreasing = 0.2f;
		[MMCondition("LerpForegroundBar", true)]
		public AnimationCurve LerpForegroundBarCurveDecreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		[MMCondition("LerpForegroundBar", true)]
		public AnimationCurve LerpForegroundBarCurveIncreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Delayed Bar Decreasing", true, 13)]
		[FormerlySerializedAs("Delay")] 
		public float DecreasingDelay = 1f;
		[FormerlySerializedAs("LerpDelayedBar")] 
		public bool LerpDecreasingDelayedBar = true;
		[FormerlySerializedAs("LerpDelayedBarSpeed")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public float LerpDecreasingDelayedBarSpeed = 15f;
		[FormerlySerializedAs("LerpDelayedBarDuration")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public float LerpDecreasingDelayedBarDuration = 0.2f;
		[FormerlySerializedAs("LerpDelayedBarCurve")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public AnimationCurve LerpDecreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Delayed Bar Increasing", true, 18)]
		public float IncreasingDelay = 1f;
		public bool LerpIncreasingDelayedBar = true;
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public float LerpIncreasingDelayedBarSpeed = 15f;
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public float LerpIncreasingDelayedBarDuration = 0.2f;
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public AnimationCurve LerpIncreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Bump", true, 14)]
		public bool BumpScaleOnChange = true;
		public bool BumpOnIncrease = false;
		public bool BumpOnDecrease = false;
        public float BumpDuration = 0.2f;
        public bool ChangeColorWhenBumping = true;
        [MMCondition("ChangeColorWhenBumping", true)]
        public Color BumpColor = Color.white;
        [FormerlySerializedAs("BumpAnimationCurve")]
        public AnimationCurve BumpScaleAnimationCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
        public AnimationCurve BumpColorAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        public bool Bumping { get; protected set; }

        [MMInspectorGroup("Events", true, 16)]
        public UnityEvent OnBump;
        public UnityEvent OnBarMovementDecreasingStart;
        public UnityEvent OnBarMovementDecreasingStop;
        public UnityEvent OnBarMovementIncreasingStart;
        public UnityEvent OnBarMovementIncreasingStop;

        [MMInspectorGroup("Text", true, 20)] 
        public Text PercentageText;

        public string TextPrefix;
        public string TextSuffix;
        public float TextValueMultiplier = 1f;
        public string TextFormat = "{000}";

        [MMInspectorGroup("Debug", true, 15)]
        [Range(0f, 1f)] 
        public float DebugNewTargetValue;

        [MMInspectorButton("DebugUpdateBar")]
        public bool DebugUpdateBarButton;
        [MMInspectorButton("DebugSetBar")]
        public bool DebugSetBarButton;
        [MMInspectorButton("Bump")]
        public bool TestBumpButton;
        [MMInspectorButton("Plus10Percent")]
        public bool Plus10PercentButton;
        [MMInspectorButton("Minus10Percent")]
        public bool Minus10PercentButton;
        
        [MMInspectorGroup("Debug Read Only", true, 19)]
        [Range(0f,1f)]
        public float BarProgress;/// the current progress of the bar, ideally read only
        [Range(0f,1f)]
        public float BarTarget;
        [Range(0f,1f)]
        public float DelayedBarIncreasingProgress;
        [Range(0f,1f)]
        public float DelayedBarDecreasingProgress;

        protected bool _initialized;
        protected Vector2 _initialBarSize;
        protected Color _initialColor;
        protected Vector3 _initialScale;
        
        protected Image _foregroundImage;
        protected Image _delayedDecreasingImage;
        protected Image _delayedIncreasingImage;
        
        protected Vector3 _targetLocalScale = Vector3.one;
		protected float _newPercent;
        protected float _percentLastTimeBarWasUpdated;
		protected float _lastUpdateTimestamp;
        
        protected float _time;
        protected float _deltaTime;
        protected int _direction;
        protected Coroutine _coroutine;
        protected bool _coroutineShouldRun = false;
        protected bool _isDelayedBarIncreasingNotNull;
        protected bool _isDelayedBarDecreasingNotNull;
        protected bool _actualUpdate;
        protected Vector2 _anchorVector;

        protected float _delayedBarDecreasingProgress;
        protected float _delayedBarIncreasingProgress;
        protected MMProgressBarStates CurrentState = MMProgressBarStates.Idle;

        #region PUBLIC_API
        public virtual void UpdateBar01(float normalizedValue) 
        {
	        UpdateBar(Mathf.Clamp01(normalizedValue), 0f, 1f);
        }
        public virtual void UpdateBar(float currentValue,float minValue,float maxValue) 
        {
            if (!_initialized)
            {
                Initialization();
            }
            
	        _newPercent = MMMaths.Remap(currentValue, minValue, maxValue, MinimumBarFillValue, MaximumBarFillValue);
	        
	        _actualUpdate = (BarTarget != _newPercent);
	        
	        if (!_actualUpdate)
	        {
		        return;
	        }
	        
	        if (CurrentState != MMProgressBarStates.Idle)
	        {
		        if ((CurrentState == MMProgressBarStates.Decreasing) ||
		            (CurrentState == MMProgressBarStates.InDecreasingDelay))
		        {
			        if (_newPercent >= BarTarget)
			        {
				        StopCoroutine(_coroutine);
				        SetBar01(BarTarget);
			        }
		        }
		        if ((CurrentState == MMProgressBarStates.Increasing) ||
		            (CurrentState == MMProgressBarStates.InIncreasingDelay))
		        {
			        if (_newPercent <= BarTarget)
			        {
				        StopCoroutine(_coroutine);
				        SetBar01(BarTarget);
			        }
		        }
	        }
	        
	        _percentLastTimeBarWasUpdated = BarProgress;
	        _delayedBarDecreasingProgress = DelayedBarDecreasingProgress;
	        _delayedBarIncreasingProgress = DelayedBarIncreasingProgress;
	        
	        BarTarget = _newPercent;
			
	        if ((_newPercent != _percentLastTimeBarWasUpdated) && !Bumping)
	        {
		        Bump();
	        }

	        DetermineDeltaTime();
	        _lastUpdateTimestamp = _time;
	        
		    DetermineDirection();
		    if (_direction < 0)
		    {
			    OnBarMovementDecreasingStart?.Invoke();
		    }
		    else
		    {
			    OnBarMovementIncreasingStart?.Invoke();
		    }
		        
		    if (_coroutine != null)
		    {
			    StopCoroutine(_coroutine);
		    }
		    _coroutineShouldRun = true;
		    
		    if (!this.gameObject.activeInHierarchy)
            {
                this.gameObject.SetActive(true);    
		    }

            if (this.gameObject.activeInHierarchy)
            {
                _coroutine = StartCoroutine(UpdateBarsCo());
            }                

            UpdateText();
        }
        public virtual void SetBar(float currentValue, float minValue, float maxValue)
        {
	        float newPercent = MMMaths.Remap(currentValue, minValue, maxValue, 0f, 1f);
	        SetBar01(newPercent);
        }
        public virtual void SetBar01(float newPercent)
        {
            if (!_initialized)
            {
                Initialization();
            }

            newPercent = MMMaths.Remap(newPercent, 0f, 1f, MinimumBarFillValue, MaximumBarFillValue);
	        BarProgress = newPercent;
	        DelayedBarDecreasingProgress = newPercent;
	        DelayedBarIncreasingProgress = newPercent;
	        BarTarget = newPercent;
	        _percentLastTimeBarWasUpdated = newPercent;
	        _delayedBarDecreasingProgress = DelayedBarDecreasingProgress;
	        _delayedBarIncreasingProgress = DelayedBarIncreasingProgress;
	        SetBarInternal(newPercent, ForegroundBar, _foregroundImage, _initialBarSize);
	        SetBarInternal(newPercent, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
	        SetBarInternal(newPercent, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);
	        UpdateText();
	        _coroutineShouldRun = false;
	        CurrentState = MMProgressBarStates.Idle;
        }
        
        #endregion PUBLIC_API

        #region START
        protected virtual void Start()
		{
            Initialization();
        }

        protected virtual void OnEnable()
        {
            if (!_initialized)
            {
                return;
            }

            if (_foregroundImage != null)
            {
	            _foregroundImage.color = _initialColor;    
            }
        }

        public virtual void Initialization()
        {
            _isDelayedBarDecreasingNotNull = DelayedBarDecreasing != null;
            _isDelayedBarIncreasingNotNull = DelayedBarIncreasing != null;
            _initialScale = this.transform.localScale;

            if (ForegroundBar != null)
            {
                _foregroundImage = ForegroundBar.GetComponent<Image>();
                _initialBarSize = _foregroundImage.rectTransform.sizeDelta;
            }
            if (DelayedBarDecreasing != null)
            {
                _delayedDecreasingImage = DelayedBarDecreasing.GetComponent<Image>();
            }
            if (DelayedBarIncreasing != null)
            {
                _delayedIncreasingImage = DelayedBarIncreasing.GetComponent<Image>();
            }
            _initialized = true;

            if (_foregroundImage != null)
            {
                _initialColor = _foregroundImage.color;
            }

            _percentLastTimeBarWasUpdated = BarProgress;

            if (SetInitialFillValueOnStart)
            {
                SetBar01(InitialFillValue);
            }
        }
        
        #endregion START

        #region TESTS
        protected virtual void DebugUpdateBar()
        {
	        this.UpdateBar01(DebugNewTargetValue);
        }
        protected virtual void DebugSetBar()
        {
	        this.SetBar01(DebugNewTargetValue);
        }
        public virtual void Plus10Percent()
        {
	        float newProgress = BarTarget + 0.1f;
	        newProgress = Mathf.Clamp(newProgress, 0f, 1f);
	        UpdateBar01(newProgress);
        }
        public virtual void Minus10Percent()
        {
	        float newProgress = BarTarget - 0.1f;
	        newProgress = Mathf.Clamp(newProgress, 0f, 1f);
	        UpdateBar01(newProgress);
        }


        #endregion TESTS

        
        
        protected virtual void UpdateText()
        {
	        if (PercentageText == null)
	        {
		        return;
	        }

	        PercentageText.text = TextPrefix + (BarTarget * TextValueMultiplier).ToString(TextFormat) + TextSuffix;
        }
		protected virtual IEnumerator UpdateBarsCo()
		{
			while (_coroutineShouldRun)
			{
				DetermineDeltaTime();
				DetermineDirection();
				UpdateBars();
				yield return null;
			}

			CurrentState = MMProgressBarStates.Idle;
			yield break;
		}
		
        protected virtual void DetermineDeltaTime()
        {
	        _deltaTime = (TimeScale == TimeScales.Time) ? Time.deltaTime : Time.unscaledDeltaTime;
	        _time = (TimeScale == TimeScales.Time) ? Time.time : Time.unscaledTime;
        }

        protected virtual void DetermineDirection()
        {
		    _direction = (_newPercent > _percentLastTimeBarWasUpdated) ? 1 : -1;
        }
		protected virtual void UpdateBars()
		{
			float newFill;
			float newFillDelayed;
			float t1, t2 = 0f;
			if (_direction < 0)
			{
				newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedDecreasing, LerpForegroundBarDurationDecreasing, LerpForegroundBarCurveDecreasing, 0f, _percentLastTimeBarWasUpdated, out t1);
				SetBarInternal(newFill, ForegroundBar, _foregroundImage, _initialBarSize);
				SetBarInternal(newFill, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);

				BarProgress = newFill;
				DelayedBarIncreasingProgress = newFill;

				CurrentState = MMProgressBarStates.Decreasing;
				
				if (_time - _lastUpdateTimestamp > DecreasingDelay)
				{
					newFillDelayed = ComputeNewFill(LerpDecreasingDelayedBar, LerpDecreasingDelayedBarSpeed, LerpDecreasingDelayedBarDuration, LerpDecreasingDelayedBarCurve, DecreasingDelay,_delayedBarDecreasingProgress, out t2);
					SetBarInternal(newFillDelayed, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);

					DelayedBarDecreasingProgress = newFillDelayed;
					CurrentState = MMProgressBarStates.InDecreasingDelay;
				}
			}
			else
			{
				newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, 0f, _delayedBarIncreasingProgress, out t1);
				SetBarInternal(newFill, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);
				
				DelayedBarIncreasingProgress = newFill;
				CurrentState = MMProgressBarStates.Increasing;

				if (DelayedBarIncreasing == null)
				{
					newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, 0f, _percentLastTimeBarWasUpdated, out t2);
					SetBarInternal(newFill, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
					SetBarInternal(newFill, ForegroundBar, _foregroundImage, _initialBarSize);
					
					BarProgress = newFill;	
					DelayedBarDecreasingProgress = newFill;
					CurrentState = MMProgressBarStates.InDecreasingDelay;
				}
				else
				{
					if (_time - _lastUpdateTimestamp > IncreasingDelay)
					{
						newFillDelayed = ComputeNewFill(LerpIncreasingDelayedBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, IncreasingDelay, _delayedBarDecreasingProgress, out t2);
					
						SetBarInternal(newFillDelayed, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
						SetBarInternal(newFillDelayed, ForegroundBar, _foregroundImage, _initialBarSize);
					
						BarProgress = newFillDelayed;	
						DelayedBarDecreasingProgress = newFillDelayed;
						CurrentState = MMProgressBarStates.InDecreasingDelay;
					}
				}
			}
			
			if ((t1 >= 1f) && (t2 >= 1f))
			{
				_coroutineShouldRun = false;
				if (_direction > 0)
				{
					OnBarMovementIncreasingStop?.Invoke();
				}
				else
				{
					OnBarMovementDecreasingStop?.Invoke();
				}
			}
		}

		protected virtual float ComputeNewFill(bool lerpBar, float barSpeed, float barDuration, AnimationCurve barCurve, float delay, float lastPercent, out float t)
		{
			float newFill = 0f;
			t = 0f;
			if (lerpBar)
			{
				float delta = 0f;
				float timeSpent = _time - _lastUpdateTimestamp - delay;
				float speed = barSpeed;
				if (speed == 0f) { speed = 1f; }
				
				float duration = (BarFillMode == BarFillModes.FixedDuration) ? barDuration : (Mathf.Abs(_newPercent - lastPercent)) / speed;
				
				delta = MMMaths.Remap(timeSpent, 0f, duration, 0f, 1f);
				delta = Mathf.Clamp(delta, 0f, 1f);
				t = delta;
				if (t < 1f)
				{
					delta = barCurve.Evaluate(delta);
					newFill = Mathf.LerpUnclamped(lastPercent, _newPercent, delta);	
				}
				else
				{
					newFill = _newPercent;
				}
			}
			else
			{
				newFill = _newPercent;
			}

			newFill = Mathf.Clamp( newFill, 0f, 1f);

			return newFill;
		}

		protected virtual void SetBarInternal(float newAmount, Transform bar, Image image, Vector2 initialSize)
		{
			if (bar == null)
			{
                return;
			}
			
			switch (FillMode)
            {
                case FillModes.LocalScale:
                    _targetLocalScale = Vector3.one;
                    switch (BarDirection)
                    {
                        case BarDirections.LeftToRight:
                            _targetLocalScale.x = newAmount;
                            break;
                        case BarDirections.RightToLeft:
                            _targetLocalScale.x = 1f - newAmount;
                            break;
                        case BarDirections.DownToUp:
                            _targetLocalScale.y = newAmount;
                            break;
                        case BarDirections.UpToDown:
                            _targetLocalScale.y = 1f - newAmount;
                            break;
                    }

                    bar.localScale = _targetLocalScale;
                    break;

                case FillModes.Width:
                    if (image == null)
                    {
                        return;
                    }
                    float newSizeX = MMMaths.Remap(newAmount, 0f, 1f, 0, initialSize.x);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
                    break;

                case FillModes.Height:
                    if (image == null)
                    {
                        return;
                    }
                    float newSizeY = MMMaths.Remap(newAmount, 0f, 1f, 0, initialSize.y);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
                    break;

                case FillModes.FillAmount:
                    if (image == null)
                    {
                        return;
                    }
                    image.fillAmount = newAmount;
                    break;
                case FillModes.Anchor:
	                if (image == null)
	                {
		                return;
	                }
	                switch (BarDirection)
	                {
		                case BarDirections.LeftToRight:
			                _anchorVector.x = 0f;
			                _anchorVector.y = 0f;
			                image.rectTransform.anchorMin = _anchorVector;
			                _anchorVector.x = newAmount;
			                _anchorVector.y = 1f;
			                image.rectTransform.anchorMax = _anchorVector;
			                break;
		                case BarDirections.RightToLeft:
			                _anchorVector.x = newAmount;
			                _anchorVector.y = 0f;
			                image.rectTransform.anchorMin = _anchorVector;
			                _anchorVector.x = 1f;
			                _anchorVector.y = 1f;
			                image.rectTransform.anchorMax = _anchorVector;
			                break;
		                case BarDirections.DownToUp:
			                _anchorVector.x = 0f;
			                _anchorVector.y = 0f;
			                image.rectTransform.anchorMin = _anchorVector;
			                _anchorVector.x = 1f;
			                _anchorVector.y = newAmount;
			                image.rectTransform.anchorMax = _anchorVector;
			                break;
		                case BarDirections.UpToDown:
			                _anchorVector.x = 0f;
			                _anchorVector.y = newAmount;
			                image.rectTransform.anchorMin = _anchorVector;
			                _anchorVector.x = 1f;
			                _anchorVector.y = 1f;
			                image.rectTransform.anchorMax = _anchorVector;
			                break;
	                }
	                break;
            }
		}

		#region  Bump
		public virtual void Bump()
		{
			bool shouldBump = false;

			if (!_initialized)
			{
				return;
			}
			
			DetermineDirection();
			
			if (BumpOnIncrease && (_direction > 0))
			{
				shouldBump = true;
			}
			
			if (BumpOnDecrease && (_direction < 0))
			{
				shouldBump = true;
			}
			
			if (BumpScaleOnChange)
			{
				shouldBump = true;
			}

			if (!shouldBump)
			{
				return;
			}
			
			if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(BumpCoroutine());
			}

			OnBump?.Invoke();
		}
		protected virtual IEnumerator BumpCoroutine()
		{
			float journey = 0f;

			Bumping = true;

			while (journey <= BumpDuration)
			{
				journey = journey + _deltaTime;
				float percent = Mathf.Clamp01(journey / BumpDuration);
				float curvePercent = BumpScaleAnimationCurve.Evaluate(percent);
				float colorCurvePercent = BumpColorAnimationCurve.Evaluate(percent);
				this.transform.localScale = curvePercent * _initialScale;

				if (ChangeColorWhenBumping && (_foregroundImage != null))
				{
					_foregroundImage.color = Color.Lerp(_initialColor, BumpColor, colorCurvePercent);
				}

				yield return null;
			}
			if (ChangeColorWhenBumping && (_foregroundImage != null))
			{
				_foregroundImage.color = _initialColor;
			}
			Bumping = false;
			yield return null;
		}

		#endregion Bump

		#region ShowHide
		public virtual void ShowBar()
		{
			this.gameObject.SetActive(true);
		}
		public virtual void HideBar(float delay)
		{
			if (delay <= 0)
			{
				this.gameObject.SetActive(false);
			}
			else if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(HideBarCo(delay));
			}
		}
		protected virtual IEnumerator HideBarCo(float delay)
		{
			yield return MMCoroutine.WaitFor(delay);
			this.gameObject.SetActive(false);
		}

		#endregion ShowHide
		
	}
}