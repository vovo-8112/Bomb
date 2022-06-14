using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.MMInterface
{
	public class MMCarousel : MonoBehaviour
	{
		[Header("Binding")]
		public HorizontalLayoutGroup Content;

		public Camera UICamera;

		[Header("Optional Buttons Binding")]
		public MMTouchButton LeftButton;
		public MMTouchButton RightButton;

		[Header("Carousel Setup")]
		public int CurrentIndex = 0;
		public int Pagination = 1;
		public float ThresholdInPercent = 1f;

		[Header("Speed")]
		public float MoveDuration = 0.05f;

		[Header("Focus")]
		public GameObject InitialFocus;
        public bool ForceMouseVisible = true;

		[Header("Keyboard/Gamepad")]
		protected float _elementWidth;
		protected int _contentLength = 0;
		protected float _spacing;
		protected Vector2 _initialPosition;
		protected RectTransform _rectTransform;

		protected bool _lerping = false;
		protected float _lerpStartedTimestamp;
		protected Vector2 _startPosition;
		protected Vector2 _targetPosition;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			_rectTransform = Content.gameObject.GetComponent<RectTransform> ();
			_initialPosition = _rectTransform.anchoredPosition;
			_contentLength = 0;
			foreach (Transform tr in Content.transform) 
			{ 
				_elementWidth = tr.gameObject.MMGetComponentNoAlloc<RectTransform>().sizeDelta.x;
				_contentLength++;
			}
			_spacing = Content.spacing;
			_rectTransform.anchoredPosition = DeterminePosition ();

			if (InitialFocus != null)
			{
				EventSystem.current.SetSelectedGameObject(InitialFocus, null);
			}

            if (ForceMouseVisible)
            {
                Cursor.visible = true;
            }
		}
		public virtual void MoveLeft()
		{
			if (!CanMoveLeft())
			{
				return;
			}
			else
			{				
				CurrentIndex -= Pagination;
				MoveToCurrentIndex ();	
			}
		}
		public virtual void MoveRight()
		{
			if (!CanMoveRight())
			{
				return;
			}
			else
			{
				CurrentIndex += Pagination;
				MoveToCurrentIndex ();	
			}
		}
		protected virtual void MoveToCurrentIndex ()
		{
			_startPosition = _rectTransform.anchoredPosition;
			_targetPosition = DeterminePosition ();
			_lerping = true;
			_lerpStartedTimestamp = Time.time;
		}
		protected virtual Vector2 DeterminePosition()
		{
			return _initialPosition - (Vector2.right * CurrentIndex * (_elementWidth + _spacing));
		}

		public virtual bool CanMoveLeft()
		{
			return (CurrentIndex - Pagination >= 0);
				
		}
		public virtual bool CanMoveRight()
		{
			return (CurrentIndex + Pagination < _contentLength);
		}
		protected virtual void Update()
		{
			if (_lerping)
			{
				LerpPosition ();
			}
			HandleButtons ();
			HandleFocus ();
		}

		protected virtual void HandleFocus()
		{
			if (!_lerping && Time.timeSinceLevelLoad > 0.5f)
			{
				if (EventSystem.current.currentSelectedGameObject != null)
				{
					if (UICamera.WorldToScreenPoint(EventSystem.current.currentSelectedGameObject.transform.position).x < 0)
					{
						MoveLeft ();
					}
					if (UICamera.WorldToScreenPoint(EventSystem.current.currentSelectedGameObject.transform.position).x > Screen.width)
					{
						MoveRight ();
					}	
				}
			}
		}
		protected virtual void HandleButtons()
		{
			if (LeftButton != null) 
			{ 
				if (CanMoveLeft())
				{
					LeftButton.EnableButton (); 
				}
				else
				{
					LeftButton.DisableButton (); 
				}	
			}
			if (RightButton != null) 
			{ 
				if (CanMoveRight())
				{
					RightButton.EnableButton (); 
				}
				else
				{
					RightButton.DisableButton (); 
				}	
			}
		}
		protected virtual void LerpPosition()
		{
			float timeSinceStarted = Time.time - _lerpStartedTimestamp;
			float percentageComplete = timeSinceStarted / MoveDuration;

			_rectTransform.anchoredPosition = Vector2.Lerp (_startPosition, _targetPosition, percentageComplete);
			if(percentageComplete >= ThresholdInPercent)
			{
				_lerping = false;
			}
		}
	}
}