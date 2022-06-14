using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchDynamicJoystick")]
    public class MMTouchDynamicJoystick : MMTouchJoystick, IPointerDownHandler
	{
		[Header("Dynamic Joystick")]
		[MMInformation("Here you can select an image for your joystick's knob, and decide if the joystick's detection zone should reset its position whenever the drag ends.", MMInformationAttribute.InformationType.Info,false)]
		public Sprite JoystickKnobImage;
		public bool RestorePosition = true;

		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected CanvasGroup _knobCanvasGroup;
		protected override void Start()
		{
			base.Start();
			_initialPosition = GetComponent<RectTransform>().localPosition;
			if (JoystickKnobImage!=null)
			{
				GameObject knob = new GameObject();
				knob.transform.SetParent(gameObject.transform);
				knob.name="DynamicJoystickKnob";
				knob.transform.position = transform.position;
				knob.transform.localScale = transform.localScale;

				Image knobImage = knob.AddComponent<Image>();
				knobImage.sprite = JoystickKnobImage;

				_knobCanvasGroup = knob.AddComponent<CanvasGroup>();
			}
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
			SetNeutralPosition(_newPosition);
	    }
		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
			if (RestorePosition)
			{
				GetComponent<RectTransform>().localPosition = _initialPosition;
			}
		}
	}
}
