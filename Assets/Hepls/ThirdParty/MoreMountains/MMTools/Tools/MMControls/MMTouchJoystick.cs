using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace MoreMountains.Tools 
{
	[System.Serializable]
	public class JoystickEvent : UnityEvent<Vector2> {}
	[RequireComponent(typeof(Rect))]
	[RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchJoystick")]
    public class MMTouchJoystick : MonoBehaviour, IDragHandler, IEndDragHandler
	{
		[Header("Camera")]
		public Camera TargetCamera;

		[Header("Pressed Behaviour")]
		[MMInformation("Here you can set the opacity of the joystick when it's pressed. Useful for visual feedback.", MMInformationAttribute.InformationType.Info,false)]
		public float PressedOpacity = 0.5f;

		[Header("Axis")]
		[MMInformation("Choose if you want a joystick limited to one axis or not, and define the MaxRange. The MaxRange is the maximum distance from its initial center position you can drag the joystick to.",MMInformationAttribute.InformationType.Info,false)]
		public bool HorizontalAxisEnabled = true;
		public bool VerticalAxisEnabled = true;
		[MMInformation("And finally you can bind a function to get your joystick's values. Your method has to have a Vector2 as a parameter. Drag your object here and select the method.", MMInformationAttribute.InformationType.Info,false)]
		public float MaxRange = 1.5f;

		[Header("Binding")]
		public JoystickEvent JoystickValue;

		[Header("Rotating Direction Indicator")]
		public Transform RotatingIndicator;
		public float RotatingIndicatorThreshold = 0.1f;

		public RenderMode ParentCanvasRenderMode { get; protected set; }
		protected Vector2 _neutralPosition;
		public Vector2 _joystickValue;
		protected RectTransform _canvasRectTransform;
		protected Vector2 _newTargetPosition;
		protected Vector3 _newJoystickPosition;
		protected float _initialZPosition;

	    protected CanvasGroup _canvasGroup;
		protected float _initialOpacity;
		protected Transform _knobTransform;
		protected bool _rotatingIndicatorIsNotNull = false;
        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            Initialize();
        }

        public virtual void Initialize()
		{
			_canvasRectTransform = GetComponentInParent<Canvas>().transform as RectTransform;
			_canvasGroup = GetComponent<CanvasGroup>();
			_rotatingIndicatorIsNotNull = (RotatingIndicator != null);

			SetKnobTransform(this.transform);

            SetNeutralPosition();
			if (TargetCamera == null)
			{
				throw new Exception("MMTouchJoystick : you have to set a target camera");
			}
			ParentCanvasRenderMode = GetComponentInParent<Canvas>().renderMode;
			_initialZPosition = _knobTransform.position.z;
			_initialOpacity = _canvasGroup.alpha;			
		}
		public virtual void SetKnobTransform(Transform newTransform)
		{
			_knobTransform = newTransform;
		}
		protected virtual void Update()
		{
            if (JoystickValue != null)
			{
				if (HorizontalAxisEnabled || VerticalAxisEnabled)
				{
					JoystickValue.Invoke(_joystickValue);
				}
			}

            RotateIndicator();
		}

		protected virtual void RotateIndicator()
		{
			if (!_rotatingIndicatorIsNotNull)
			{
				return;
			}

			RotatingIndicator.gameObject.SetActive(_joystickValue.magnitude > RotatingIndicatorThreshold);
			float angle = Mathf.Atan2(_joystickValue.y, _joystickValue.x) * Mathf.Rad2Deg;
			RotatingIndicator.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
		public virtual void SetNeutralPosition()
		{
            _neutralPosition = _knobTransform.position;
        }

		public virtual void SetNeutralPosition(Vector3 newPosition)
		{
			_neutralPosition = newPosition;
		}
		public virtual void OnDrag(PointerEventData eventData)
		{
			_canvasGroup.alpha = PressedOpacity;
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				_newTargetPosition = TargetCamera.ScreenToWorldPoint(eventData.position);
			}
			else
			{
				_newTargetPosition = eventData.position;
			}
			_newTargetPosition = Vector2.ClampMagnitude(_newTargetPosition - _neutralPosition, MaxRange);
			if (!HorizontalAxisEnabled)
			{
				_newTargetPosition.x = 0;
			}
			if (!VerticalAxisEnabled)
			{
				_newTargetPosition.y = 0;
			}
			_joystickValue.x = EvaluateInputValue(_newTargetPosition.x);
			_joystickValue.y = EvaluateInputValue(_newTargetPosition.y);

			_newJoystickPosition = _neutralPosition + _newTargetPosition;
			_newJoystickPosition.z = _initialZPosition;
			_knobTransform.position = _newJoystickPosition;
		}
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			_newJoystickPosition = _neutralPosition;
			_newJoystickPosition.z = _initialZPosition;
			_knobTransform.position = _newJoystickPosition;
			_joystickValue.x = 0f;
			_joystickValue.y = 0f;
			_canvasGroup.alpha=_initialOpacity;
		}
		protected virtual float EvaluateInputValue(float vectorPosition)
		{
			return Mathf.InverseLerp(0, MaxRange, Mathf.Abs(vectorPosition)) * Mathf.Sign(vectorPosition);
		}

		protected virtual void OnEnable()
		{
			Initialize();
			_canvasGroup.alpha = _initialOpacity;
		}
	}
}
