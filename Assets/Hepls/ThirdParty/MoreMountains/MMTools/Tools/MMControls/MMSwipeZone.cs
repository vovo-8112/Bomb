using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
	public enum MMPossibleSwipeDirections { Up, Down, Left, Right }


	[System.Serializable]
	public class SwipeEvent : UnityEvent<MMSwipeEvent> {}
	public struct MMSwipeEvent
	{
		public MMPossibleSwipeDirections SwipeDirection;
		public float SwipeAngle;
		public float SwipeLength;
		public Vector2 SwipeOrigin;
		public Vector2 SwipeDestination;
		public float SwipeDuration;
		public MMSwipeEvent(MMPossibleSwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination, float swipeDuration)
		{
			SwipeDirection = direction;
			SwipeAngle = angle;
			SwipeLength = length;
			SwipeOrigin = origin;
			SwipeDestination = destination;
			SwipeDuration = swipeDuration;
		}

        static MMSwipeEvent e;
        public static void Trigger(MMPossibleSwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination, float swipeDuration)
        {
            e.SwipeDirection = direction;
            e.SwipeAngle = angle;
            e.SwipeLength = length;
            e.SwipeOrigin = origin;
            e.SwipeDestination = destination;
            e.SwipeDuration = swipeDuration;
            MMEventManager.TriggerEvent(e);
        }
    }
	[RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("More Mountains/Tools/Controls/MMSwipeZone")]
    public class MMSwipeZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		public float MinimalSwipeLength = 50f;
		public float MaximumPressLength = 10f;
		public SwipeEvent ZoneSwiped;
		public UnityEvent ZonePressed;

		[Header("Mouse Mode")]
		[MMInformation("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).", MMInformationAttribute.InformationType.Info,false)]
		public bool MouseMode = false;

		protected Vector2 _firstTouchPosition;
		protected float _angle;
		protected float _length;
		protected Vector2 _destination;
		protected Vector2 _deltaSwipe;
		protected MMPossibleSwipeDirections _swipeDirection;
        protected float _lastPointerUpAt = 0f;
        protected float _swipeStartedAt = 0f;
        protected float _swipeEndedAt = 0f;

		protected virtual void Swipe()
		{
			float duration = _swipeEndedAt - _swipeStartedAt;
			MMSwipeEvent swipeEvent = new MMSwipeEvent (_swipeDirection, _angle, _length, _firstTouchPosition, _destination, duration);
			MMEventManager.TriggerEvent(swipeEvent);
			if (ZoneSwiped != null)
			{
				ZoneSwiped.Invoke (swipeEvent);
			}
		}

		protected virtual void Press()
		{
			if (ZonePressed != null)
			{
				ZonePressed.Invoke ();
			}
		}
		public virtual void OnPointerDown(PointerEventData data)
		{
			_firstTouchPosition = Input.mousePosition;
			_swipeStartedAt = Time.unscaledTime;
		}
		public virtual void OnPointerUp(PointerEventData data)
		{
            if (Time.frameCount == _lastPointerUpAt)
            {
                return;
            }

			_destination = Input.mousePosition;
			_deltaSwipe = _destination - _firstTouchPosition;
			_length = _deltaSwipe.magnitude;
			if (_length > MinimalSwipeLength)
			{
				_angle = MMMaths.AngleBetween (_deltaSwipe, Vector2.right);
				_swipeDirection = AngleToSwipeDirection (_angle);
				_swipeEndedAt = Time.unscaledTime;
				Swipe ();
			}
			if (_deltaSwipe.magnitude < MaximumPressLength)
			{
				Press ();
			}

            _lastPointerUpAt = Time.frameCount;
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
		protected virtual MMPossibleSwipeDirections AngleToSwipeDirection(float angle)
		{
			if ((angle < 45) || (angle >= 315))
			{
				return MMPossibleSwipeDirections.Right;
			}
			if ((angle >= 45) && (angle < 135))
			{
				return MMPossibleSwipeDirections.Up;
			}
			if ((angle >= 135) && (angle < 225))
			{
				return MMPossibleSwipeDirections.Left;
			}
			if ((angle >= 225) && (angle < 315))
			{
				return MMPossibleSwipeDirections.Down;
			}
			return MMPossibleSwipeDirections.Right;
		}
	}
}