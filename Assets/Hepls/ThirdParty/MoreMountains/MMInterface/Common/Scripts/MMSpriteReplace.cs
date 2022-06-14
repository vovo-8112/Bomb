using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.MMInterface
{
	public class MMSpriteReplace : MonoBehaviour 
	{

        [Header("Sprites")]
        [MMInformation("Add this to an Image or a SpriteRenderer to be able to swap between two sprites.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public Sprite OnSprite;
		public Sprite OffSprite;

        [Header("Start settings")]
		public bool StartsOn = true;

        [Header("Debug")]
        [MMInspectorButton("Swap")]
        public bool SwapButton;
        [MMInspectorButton("SwitchToOffSprite")]
        public bool SwitchToOffSpriteButton;
        [MMInspectorButton("SwitchToOnSprite")]
        public bool SwitchToOnSpriteButton;
        public bool CurrentValue { get { return (_image.sprite == OnSprite); } }
        
        protected Image _image;
        protected SpriteRenderer _spriteRenderer;
		protected MMTouchButton _mmTouchButton;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			_image = GetComponent<Image> ();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _mmTouchButton = GetComponent<MMTouchButton> ();
			if (_mmTouchButton != null)
			{
				_mmTouchButton.ReturnToInitialSpriteAutomatically = false;
			}
			if ((OnSprite == null) || (OffSprite == null))
            {
                return;
            }

            if (_image != null)
            {
                if (StartsOn)
                {
                    _image.sprite = OnSprite;
                }
                else
                {
                    _image.sprite = OffSprite;
                }
            }

            if (_spriteRenderer != null)
            {
                if (StartsOn)
                {
                    _spriteRenderer.sprite = OnSprite;
                }
                else
                {
                    _spriteRenderer.sprite = OffSprite;
                }
            }			
		}
		public virtual void Swap()
		{
            if (_image != null)
            {
                if (_image.sprite != OnSprite)
                {
                    SwitchToOnSprite();
                }
                else
                {
                    SwitchToOffSprite();
                }
            }

            if (_spriteRenderer != null)
            {
                if (_spriteRenderer.sprite != OnSprite)
                {
                    SwitchToOnSprite();
                }
                else
                {
                    SwitchToOffSprite();
                }
            }			
		}
		public virtual void SwitchToOffSprite()
		{
			if ((_image == null) && (_spriteRenderer == null))
            {
                return;
            }
			if (OffSprite == null)
            {
                return;
            }

			SpriteOff ();
		}
		protected virtual void SpriteOff()
		{
            if (_image != null)
            {
                _image.sprite = OffSprite;
            }
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = OffSprite;
            }			
		}
		public virtual void SwitchToOnSprite()
        {
            if ((_image == null) && (_spriteRenderer == null))
            {
                return;
            }
            if (OnSprite == null)
            {
                return;
            }

			SpriteOn ();
		}
		protected virtual void SpriteOn()
		{
			
            if (_image != null)
            {
                _image.sprite = OnSprite;
            }
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = OnSprite;
            }
        }
	}
}
