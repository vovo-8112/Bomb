using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchRepositionableJoystick")]
    public class MMTouchRepositionableJoystick : MMTouchJoystick, IPointerDownHandler
    {
	    [Header("Dynamic Joystick")] 
	    public CanvasGroup KnobCanvasGroup;

	    public CanvasGroup BackgroundCanvasGroup;

		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected CanvasGroup _knobCanvasGroup;
		protected override void Start()
		{
			base.Start();
			_initialPosition = GetComponent<RectTransform>().localPosition;
		}

		public override void Initialize()
		{
			base.Initialize();
			SetKnobTransform(KnobCanvasGroup.transform);
			_canvasGroup = KnobCanvasGroup;
			_initialOpacity = _canvasGroup.alpha;
		}
		public virtual void OnPointerDown(PointerEventData data)
	    {
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				_newPosition = TargetCamera.ScreenToWorldPoint(data.position);
			}
			else
			{
				_newPosition = data.position;
			}
			_newPosition.z = this.transform.position.z;
			BackgroundCanvasGroup.transform.position = _newPosition;
			SetNeutralPosition(_newPosition);
			_knobTransform.position = _newPosition;
	    }
		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
		}
	}
}
