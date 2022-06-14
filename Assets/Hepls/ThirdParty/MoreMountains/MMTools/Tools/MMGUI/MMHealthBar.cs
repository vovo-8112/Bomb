﻿using UnityEngine;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/GUI/MMHealthBar")]
    public class MMHealthBar : MonoBehaviour 
	{
		public enum HealthBarTypes { Prefab, Drawn }
        public enum TimeScales { UnscaledTime, Time }

		[MMInformation("Add this component to an object and it'll add a healthbar next to it to reflect its health level in real time. You can decide here whether the health bar should be drawn automatically or use a prefab.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public HealthBarTypes HealthBarType = HealthBarTypes.Drawn;
        public TimeScales TimeScale = TimeScales.UnscaledTime;

		[Header("Select a Prefab")]
		[MMInformation("Select a prefab with a progress bar script on it. There is one example of such a prefab in Common/Prefabs/GUI.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public MMProgressBar HealthBarPrefab;

		[Header("Drawn Healthbar Settings ")]
		[MMInformation("Set the size (in world units), padding, back and front colors of the healthbar.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public Vector2 Size = new Vector2(1f,0.2f);
		public Vector2 BackgroundPadding = new Vector2(0.01f,0.01f);
		public Vector3 InitialRotationAngles;
        public Gradient ForegroundColor = new Gradient()
            {
                colorKeys = new GradientColorKey[2] {
                new GradientColorKey(MMColors.BestRed, 0),
                new GradientColorKey(MMColors.BestRed, 1f)
            },
            alphaKeys = new GradientAlphaKey[2] {new GradientAlphaKey(1, 0),new GradientAlphaKey(1, 1)}};
        public Gradient DelayedColor = new Gradient()
        {
            colorKeys = new GradientColorKey[2] {
                new GradientColorKey(MMColors.Orange, 0),
                new GradientColorKey(MMColors.Orange, 1f)
            },
            alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        };
        public Gradient BorderColor = new Gradient()
        {
            colorKeys = new GradientColorKey[2] {
                new GradientColorKey(MMColors.AntiqueWhite, 0),
                new GradientColorKey(MMColors.AntiqueWhite, 1f)
            },
            alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        };
        public Gradient BackgroundColor = new Gradient()
        {
            colorKeys = new GradientColorKey[2] {
                new GradientColorKey(MMColors.Black, 0),
                new GradientColorKey(MMColors.Black, 1f)
            },
            alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        };
        public string SortingLayerName = "UI";
		public float Delay = 0.5f;
		public bool LerpFrontBar = true;
		public float LerpFrontBarSpeed = 15f;
		public bool LerpDelayedBar = true;
		public float LerpDelayedBarSpeed = 15f;
		public bool BumpScaleOnChange = true;
		public float BumpDuration = 0.2f;
		public AnimationCurve BumpAnimationCurve = AnimationCurve.Constant(0,1,1);
        public MMFollowTarget.UpdateModes FollowTargetMode = MMFollowTarget.UpdateModes.LateUpdate;
        public bool NestDrawnHealthBar = false;

		[Header("Death")]
		public GameObject InstantiatedOnDeath;

		[Header("Offset")]
		[MMInformation("Set the offset (in world units), relative to the object's center, to which the health bar will be displayed.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public Vector3 HealthBarOffset = new Vector3(0f,1f,0f);

		[Header("Display")]
		[MMInformation("Here you can define whether or not the healthbar should always be visible. If not, you can set here how long after a hit it'll remain visible.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public bool AlwaysVisible = true;
		public float DisplayDurationOnHit = 1f;
		public bool HideBarAtZero = true;
		public float HideBarAtZeroDelay = 1f;

		protected MMProgressBar _progressBar;
		protected MMFollowTarget _followTransform;
		protected float _lastShowTimestamp = 0f;
		protected bool _showBar = false;
		protected Image _backgroundImage = null;
		protected Image _borderImage = null;
		protected Image _foregroundImage = null;
		protected Image _delayedImage = null;
		protected bool _finalHideStarted = false;
		protected virtual void Awake()
		{
            Initialization();
		}

        public virtual void Initialization()
        {
            _finalHideStarted = false;

            if (_progressBar != null)
            {
                _progressBar.gameObject.SetActive(AlwaysVisible);
                return;
            }

            if (HealthBarType == HealthBarTypes.Prefab)
            {
                if (HealthBarPrefab == null)
                {
                    Debug.LogWarning(this.name + " : the HealthBar has no prefab associated to it, nothing will be displayed.");
                    return;
                }
                _progressBar = Instantiate(HealthBarPrefab, transform.position + HealthBarOffset, transform.rotation) as MMProgressBar;
                _progressBar.transform.SetParent(this.transform);
                _progressBar.gameObject.name = "HealthBar";
            }

            if (HealthBarType == HealthBarTypes.Drawn)
            {
                DrawHealthBar();
                UpdateDrawnColors();
            }

            if (!AlwaysVisible)
            {
                _progressBar.gameObject.SetActive(false);
            }

            if (_progressBar != null)
            {
                _progressBar.SetBar(100f, 0f, 100f);
            }
        }
		protected virtual void DrawHealthBar()
		{
			GameObject newGameObject = new GameObject();
			newGameObject.name = "HealthBar|"+this.gameObject.name;

            if (NestDrawnHealthBar)
            {
                newGameObject.transform.SetParent(this.transform);
            }

			_progressBar = newGameObject.AddComponent<MMProgressBar>();

			_followTransform = newGameObject.AddComponent<MMFollowTarget>();
			_followTransform.Offset = HealthBarOffset;
			_followTransform.Target = this.transform;
            _followTransform.InterpolatePosition = false;
            _followTransform.InterpolateRotation = false;
            _followTransform.UpdateMode = FollowTargetMode;

			Canvas newCanvas = newGameObject.AddComponent<Canvas>();
			newCanvas.renderMode = RenderMode.WorldSpace;
			newCanvas.transform.localScale = Vector3.one;
			newCanvas.GetComponent<RectTransform>().sizeDelta = Size;
            if (!string.IsNullOrEmpty(SortingLayerName))
            {
                newCanvas.sortingLayerName = SortingLayerName;
            }

            GameObject container = new GameObject();
            container.transform.SetParent(newGameObject.transform);
            container.name = "MMProgressBarContainer";
            container.transform.localScale = Vector3.one;
            
			GameObject borderImageGameObject = new GameObject();
			borderImageGameObject.transform.SetParent(container.transform);
			borderImageGameObject.name = "HealthBar Border";
			_borderImage = borderImageGameObject.AddComponent<Image>();
			_borderImage.transform.position = Vector3.zero;
			_borderImage.transform.localScale = Vector3.one;
			_borderImage.GetComponent<RectTransform>().sizeDelta = Size;
			_borderImage.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

			GameObject bgImageGameObject = new GameObject();
			bgImageGameObject.transform.SetParent(container.transform);
			bgImageGameObject.name = "HealthBar Background";
			_backgroundImage = bgImageGameObject.AddComponent<Image>();
			_backgroundImage.transform.position = Vector3.zero;
			_backgroundImage.transform.localScale = Vector3.one;
			_backgroundImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding*2;
			_backgroundImage.GetComponent<RectTransform>().anchoredPosition = -_backgroundImage.GetComponent<RectTransform>().sizeDelta/2;
			_backgroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

			GameObject delayedImageGameObject = new GameObject();
			delayedImageGameObject.transform.SetParent(container.transform);
			delayedImageGameObject.name = "HealthBar Delayed Foreground";
			_delayedImage = delayedImageGameObject.AddComponent<Image>();
			_delayedImage.transform.position = Vector3.zero;
			_delayedImage.transform.localScale = Vector3.one;
			_delayedImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding*2;
			_delayedImage.GetComponent<RectTransform>().anchoredPosition = -_delayedImage.GetComponent<RectTransform>().sizeDelta/2;
			_delayedImage.GetComponent<RectTransform>().pivot = Vector2.zero;

			GameObject frontImageGameObject = new GameObject();
			frontImageGameObject.transform.SetParent(container.transform);
			frontImageGameObject.name = "HealthBar Foreground";
			_foregroundImage = frontImageGameObject.AddComponent<Image>();
			_foregroundImage.transform.position = Vector3.zero;
			_foregroundImage.transform.localScale = Vector3.one;
            _foregroundImage.color = ForegroundColor.Evaluate(1);
            _foregroundImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding*2;
			_foregroundImage.GetComponent<RectTransform>().anchoredPosition = -_foregroundImage.GetComponent<RectTransform>().sizeDelta/2;
			_foregroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

			_progressBar.LerpDecreasingDelayedBar = LerpDelayedBar;
			_progressBar.LerpForegroundBar = LerpFrontBar;
			_progressBar.LerpDecreasingDelayedBarSpeed = LerpDelayedBarSpeed;
			_progressBar.LerpForegroundBarSpeedIncreasing = LerpFrontBarSpeed;
			_progressBar.ForegroundBar = _foregroundImage.transform;
			_progressBar.DelayedBarDecreasing = _delayedImage.transform;
			_progressBar.DecreasingDelay = Delay;
			_progressBar.BumpScaleOnChange = BumpScaleOnChange;
			_progressBar.BumpDuration = BumpDuration;
			_progressBar.BumpScaleAnimationCurve = BumpAnimationCurve;
            _progressBar.TimeScale = (TimeScale == TimeScales.Time) ? MMProgressBar.TimeScales.Time : MMProgressBar.TimeScales.UnscaledTime;
            container.transform.localEulerAngles = InitialRotationAngles;
            _progressBar.Initialization();
		}
		protected virtual void Update()
		{
			if (_progressBar == null) 
			{
				return; 
			}

			if (_finalHideStarted)
			{
				return;
			}

			UpdateDrawnColors();
            
			if (AlwaysVisible)	
			{ 
				return; 
			}

			if (_showBar)
			{
				_progressBar.gameObject.SetActive(true);
                float currentTime = (TimeScale == TimeScales.UnscaledTime) ? Time.unscaledTime : Time.time;
				if (currentTime - _lastShowTimestamp > DisplayDurationOnHit)
				{
					_showBar = false;
				}
			}
			else
			{
				_progressBar.gameObject.SetActive(false);				
			}
		}
		protected virtual IEnumerator FinalHideBar()
		{
			_finalHideStarted = true;
			if (InstantiatedOnDeath != null)
			{
				Instantiate(InstantiatedOnDeath, this.transform.position + HealthBarOffset, this.transform.rotation);
			}
            if (HideBarAtZeroDelay == 0)
            {
                _showBar = false;
                _progressBar.gameObject.SetActive(false);
                yield return null;
            }
            else
            {
                _progressBar.HideBar(HideBarAtZeroDelay);
            }            
		}
		protected virtual void UpdateDrawnColors()
		{
			if (HealthBarType != HealthBarTypes.Drawn)
			{
				return;
			}

			if (_progressBar.Bumping)
			{
				return;
			}

			if (_borderImage != null)
			{
				_borderImage.color = BorderColor.Evaluate(_progressBar.BarProgress);
			}

			if (_backgroundImage != null)
			{
				_backgroundImage.color = BackgroundColor.Evaluate(_progressBar.BarProgress);
			}

			if (_delayedImage != null)
			{
				_delayedImage.color = DelayedColor.Evaluate(_progressBar.BarProgress);
			}

			if (_foregroundImage != null)
			{
				_foregroundImage.color = ForegroundColor.Evaluate(_progressBar.BarProgress);
			}
		}
		public virtual void UpdateBar(float currentHealth, float minHealth, float maxHealth, bool show)
        {
            if (!AlwaysVisible && show)
            {
                _showBar = true;
                _lastShowTimestamp = (TimeScale == TimeScales.UnscaledTime) ? Time.unscaledTime : Time.time;
            }

            if (_progressBar != null)
			{
				_progressBar.UpdateBar(currentHealth, minHealth, maxHealth)	;
                
                if (HideBarAtZero && _progressBar.BarTarget <= 0)
                {
                    StartCoroutine(FinalHideBar());
                }

                if (BumpScaleOnChange)
				{
					_progressBar.Bump();
				}
			}
		}
	}
}