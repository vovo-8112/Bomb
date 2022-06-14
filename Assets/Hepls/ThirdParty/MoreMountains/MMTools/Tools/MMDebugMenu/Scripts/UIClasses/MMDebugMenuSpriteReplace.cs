using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	public class MMDebugMenuSpriteReplace : MonoBehaviour 
	{
		public Sprite OnSprite;
		public Sprite OffSprite;
		public bool StartsOn = true;
		public bool CurrentValue { get { return (_image.sprite == OnSprite); } }

		protected Image _image;
		protected MMTouchButton _mmTouchButton;
		protected virtual void Awake()
		{
		}
		public virtual void Initialization()
		{
			_image = this.gameObject.GetComponent<Image> ();
			_mmTouchButton = this.gameObject.GetComponent<MMTouchButton> ();
			if (_mmTouchButton != null)
			{
				_mmTouchButton.ReturnToInitialSpriteAutomatically = false;
			}

			if (_image == null) { return; }
			if ((OnSprite == null) || (OffSprite == null)) { return; }

			if (StartsOn)
			{
				_image.sprite = OnSprite;
			}
			else
			{
				_image.sprite = OffSprite;
			}
		}
		public virtual void Swap()
		{
			if (_image.sprite != OnSprite)
			{
				SwitchToOnSprite ();
			}
			else
			{
				SwitchToOffSprite ();
			}
		}
		public virtual void SwitchToOffSprite()
		{
			if (_image == null) { return; }
			if (OffSprite == null) { return; }

			SpriteOff ();
		}
		protected virtual void SpriteOff()
		{
			_image.sprite = OffSprite;
		}
		public virtual void SwitchToOnSprite()
		{
			if (_image == null) { return; }
			if (OnSprite == null) { return; }

			SpriteOn ();
		}
		protected virtual void SpriteOn()
		{
			_image.sprite = OnSprite;
		}
	}
}
