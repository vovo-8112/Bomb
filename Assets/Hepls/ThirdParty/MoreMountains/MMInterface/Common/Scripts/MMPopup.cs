using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MoreMountains.Tools;

namespace MoreMountains.MMInterface
{
	public class MMPopup : MonoBehaviour 
	{
		public bool CurrentlyOpen = false;

		[Header("Fader")]
		public float FaderOpenDuration = 0.2f;
		public float FaderCloseDuration = 0.2f;
		public float FaderOpacity = 0.8f;
        public MMTweenType Tween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        public int ID = 0;

        protected Animator _animator;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			_animator = GetComponent<Animator> ();
		}
		protected virtual void Update()
		{
			if (_animator != null)
			{
				_animator.SetBool ("Closed", !CurrentlyOpen);
			}
		}
		public virtual void Open()
		{
			if (CurrentlyOpen)
			{
				return;
			}

			MMFadeEvent.Trigger(FaderOpenDuration, FaderOpacity, Tween, ID);
			_animator.SetTrigger ("Open");
			CurrentlyOpen = true;
		}
		public virtual void Close()
		{
			if (!CurrentlyOpen)
			{
				return;
			}

			MMFadeEvent.Trigger(FaderCloseDuration, 0f, Tween, ID);
			_animator.SetTrigger ("Close");
			CurrentlyOpen = false;
		}

	}
}
