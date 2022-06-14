using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{	
	[System.Serializable]
	public class AxisEvent : UnityEvent<float> {}
	[RequireComponent(typeof(Rect))]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchAxis")]
    public class MMTouchAxis : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }
		[Header("Binding")]
		public UnityEvent AxisPressedFirstTime;
		public UnityEvent AxisReleased;
		public AxisEvent AxisPressed;

		[Header("Pressed Behaviour")]
		[MMInformation("Here you can set the opacity of the button when it's pressed. Useful for visual feedback.",MMInformationAttribute.InformationType.Info,false)]
		public float PressedOpacity = 0.5f;
		public float AxisValue;

		[Header("Mouse Mode")]
		[MMInformation("If you set this to true, you'll need to actually press the axis for it to be triggered, otherwise a simple hover will trigger it (better for touch input).", MMInformationAttribute.InformationType.Info,false)]
		public bool MouseMode = false;

		public ButtonStates CurrentState { get; protected set; }

	    protected CanvasGroup _canvasGroup;
	    protected float _initialOpacity;
	    protected virtual void Awake()
	    {
			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup!=null)
			{
				_initialOpacity = _canvasGroup.alpha;
			}
			ResetButton();
	    }
		protected virtual void Update()
	    {
			if (AxisPressed != null)
			{
				if (CurrentState == ButtonStates.ButtonPressed)
				{
					AxisPressed.Invoke(AxisValue);
				}
	        }
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
		public virtual void OnPointerDown(PointerEventData data)
	    {
			if (CurrentState != ButtonStates.Off)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonDown;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=PressedOpacity;
			}
			if (AxisPressedFirstTime!=null)
	        {
				AxisPressedFirstTime.Invoke();
	        }
	    }
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (CurrentState != ButtonStates.ButtonPressed && CurrentState != ButtonStates.ButtonDown)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonUp;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=_initialOpacity;
			}
			if (AxisReleased != null)
			{
				AxisReleased.Invoke();
			}
			AxisPressed.Invoke(0);
	    }
		protected virtual void OnEnable()
	    {
			ResetButton();
	    }
	    protected virtual void ResetButton()
	    {
			CurrentState = ButtonStates.Off;
			_canvasGroup.alpha = _initialOpacity;
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
	}
}