using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    [RequireComponent(typeof(Rect))]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchButton")]
    public class MMTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, ISubmitHandler
	{
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp, Disabled }
		[Header("Binding")]
		public UnityEvent ButtonPressedFirstTime;
		public UnityEvent ButtonReleased;
		public UnityEvent ButtonPressed;

		[Header("Sprite Swap")]
		[MMInformation("Here you can define, for disabled and pressed states, if you want a different sprite, and a different color.", MMInformationAttribute.InformationType.Info,false)]
		public Sprite DisabledSprite;
		public bool DisabledChangeColor = false;
		public Color DisabledColor = Color.white;
		public Sprite PressedSprite;
		public bool PressedChangeColor = false;
		public Color PressedColor= Color.white;
		public Sprite HighlightedSprite;
		public bool HighlightedChangeColor = false;
		public Color HighlightedColor = Color.white;

		[Header("Opacity")]
		[MMInformation("Here you can set different opacities for the button when it's pressed, idle, or disabled. Useful for visual feedback.",MMInformationAttribute.InformationType.Info,false)]
		public float PressedOpacity = 1f;
		public float IdleOpacity = 1f;
		public float DisabledOpacity = 1f;

		[Header("Delays")]
		[MMInformation("Specify here the delays to apply when the button is pressed initially, and when it gets released. Usually you'll keep them at 0.",MMInformationAttribute.InformationType.Info,false)]
		public float PressedFirstTimeDelay = 0f;
		public float ReleasedDelay = 0f;

		[Header("Buffer")]
		public float BufferDuration = 0f;

		[Header("Animation")]
		[MMInformation("Here you can bind an animator, and specify animation parameter names for the various states.",MMInformationAttribute.InformationType.Info,false)]
		public Animator Animator;
		public string IdleAnimationParameterName = "Idle";
		public string DisabledAnimationParameterName = "Disabled";
		public string PressedAnimationParameterName = "Pressed";

		[Header("Mouse Mode")]
		[MMInformation("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better to leave it unchecked if you're going for touch input).", MMInformationAttribute.InformationType.Info,false)]
		public bool MouseMode = false;

		public bool ReturnToInitialSpriteAutomatically { get; set; }
		public ButtonStates CurrentState { get; protected set; }

		protected bool _zonePressed = false;
		protected CanvasGroup _canvasGroup;
		protected float _initialOpacity;
		protected Animator _animator;
		protected Image _image;
		protected Sprite _initialSprite;
		protected Color _initialColor;
		protected float _lastClickTimestamp = 0f;
		protected Selectable _selectable;
		protected virtual void Awake()
		{
			Initialization ();
		}

		protected virtual void Initialization()
		{
			ReturnToInitialSpriteAutomatically = true;

			_selectable = GetComponent<Selectable> ();

			_image = GetComponent<Image> ();
			if (_image != null)
			{
				_initialColor = _image.color;
				_initialSprite = _image.sprite;
			}

			_animator = GetComponent<Animator> ();
			if (Animator != null)
			{
				_animator = Animator;
			}

			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup!=null)
			{
				_initialOpacity = IdleOpacity;
				_canvasGroup.alpha = _initialOpacity;
				_initialOpacity = _canvasGroup.alpha;
			}
			ResetButton();
		}
		protected virtual void Update()
		{
			switch (CurrentState)
			{
				case ButtonStates.Off:
					SetOpacity (IdleOpacity);
					if ((_image != null) && (ReturnToInitialSpriteAutomatically))
					{
						_image.color = _initialColor;
						_image.sprite = _initialSprite;
					}
					if (_selectable != null)
					{
						_selectable.interactable = true;
						if (EventSystem.current.currentSelectedGameObject == this.gameObject)
						{
							if ((_image != null) && HighlightedChangeColor)
							{
								_image.color = HighlightedColor;
							}
							if (HighlightedSprite != null)
							{
								_image.sprite = HighlightedSprite;
							}
						}
					}
					break;

				case ButtonStates.Disabled:
					SetOpacity (DisabledOpacity);
					if (_image != null)
					{
						if (DisabledSprite != null)
						{
							_image.sprite = DisabledSprite;	
						}
						if (DisabledChangeColor)
						{
							_image.color = DisabledColor;	
						}
					}
					if (_selectable != null)
					{
						_selectable.interactable = false;
					}
					break;

				case ButtonStates.ButtonDown:

					break;

				case ButtonStates.ButtonPressed:
					SetOpacity (PressedOpacity);
					OnPointerPressed();
					if (_image != null)
					{
						if (PressedSprite != null)
						{
							_image.sprite = PressedSprite;
						}
						if (PressedChangeColor)
						{
							_image.color = PressedColor;	
						}
					}
					break;

				case ButtonStates.ButtonUp:

					break;
			}
			UpdateAnimatorStates ();
		}
		protected virtual void LateUpdate()
		{
			if (CurrentState == ButtonStates.ButtonUp)
			{
				CurrentState = ButtonStates.Off;
			}
			if (CurrentState == ButtonStates.ButtonDown)
			{
				CurrentState = ButtonStates.ButtonPressed;
			}
		}

		public event System.Action<PointerEventData.FramePressState, PointerEventData> ButtonStateChange;
		public virtual void OnPointerDown(PointerEventData data)
		{
			if (Time.time - _lastClickTimestamp < BufferDuration)
			{
				return;
			}

			if (CurrentState != ButtonStates.Off)
			{
				return;
			}
			CurrentState = ButtonStates.ButtonDown;
			_lastClickTimestamp = Time.time;
			ButtonStateChange?.Invoke(PointerEventData.FramePressState.Pressed, data);
			if ((Time.timeScale != 0) && (PressedFirstTimeDelay > 0))
			{
				Invoke ("InvokePressedFirstTime", PressedFirstTimeDelay);	
			}
			else
			{
				ButtonPressedFirstTime.Invoke();
			}
		}

		protected virtual void InvokePressedFirstTime()
		{
			if (ButtonPressedFirstTime!=null)
			{
				ButtonPressedFirstTime.Invoke();
			}
		}
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (CurrentState != ButtonStates.ButtonPressed && CurrentState != ButtonStates.ButtonDown)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonUp;
			ButtonStateChange?.Invoke(PointerEventData.FramePressState.Released, data);
			if ((Time.timeScale != 0) && (ReleasedDelay > 0))
			{
				Invoke ("InvokeReleased", ReleasedDelay);
			}
			else
			{
				ButtonReleased.Invoke();
			}
		}

		protected virtual void InvokeReleased()
		{
			if (ButtonReleased != null)
			{
				ButtonReleased.Invoke();
			}			
		}
		public virtual void OnPointerPressed()
		{
			CurrentState = ButtonStates.ButtonPressed;
			if (ButtonPressed != null)
			{
				ButtonPressed.Invoke();
			}
		}
		protected virtual void ResetButton()
		{
			SetOpacity(_initialOpacity);
			CurrentState = ButtonStates.Off;
		}
		public virtual void OnPointerEnter(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerDown (data);
			}
		}
		public virtual void OnPointerExit(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerUp(data);	
			}
		}
		protected virtual void OnEnable()
		{
			ResetButton();
		}

		private void OnDisable()
		{
			bool wasActive = CurrentState != ButtonStates.Off && CurrentState != ButtonStates.Disabled;
			DisableButton();
			CurrentState = ButtonStates.Off;
			if (wasActive)
			{
				ButtonStateChange?.Invoke(PointerEventData.FramePressState.Released, null);
				ButtonReleased?.Invoke();
			}
		}

		public virtual void DisableButton()
		{
			CurrentState = ButtonStates.Disabled;
		}

		public virtual void EnableButton()
		{
			if (CurrentState == ButtonStates.Disabled)
			{
				CurrentState = ButtonStates.Off;	
			}
		}

		protected virtual void SetOpacity(float newOpacity)
		{
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha = newOpacity;
			}
		}

		protected virtual void UpdateAnimatorStates ()
		{
			if (_animator == null)
			{
				return;
			}
			if (DisabledAnimationParameterName != null)
			{
				_animator.SetBool (DisabledAnimationParameterName, (CurrentState == ButtonStates.Disabled));
			}
			if (PressedAnimationParameterName != null)
			{
				_animator.SetBool (PressedAnimationParameterName, (CurrentState == ButtonStates.ButtonPressed));
			}
			if (IdleAnimationParameterName != null)
			{
				_animator.SetBool (IdleAnimationParameterName, (CurrentState == ButtonStates.Off));
			}
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (ButtonPressedFirstTime!=null)
			{
				ButtonPressedFirstTime.Invoke();
			}
			if (ButtonReleased!=null)
			{
				ButtonReleased.Invoke ();
			}
		}
	}
}