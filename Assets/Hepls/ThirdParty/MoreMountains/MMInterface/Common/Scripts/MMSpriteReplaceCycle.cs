using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.MMInterface
{
	public class MMSpriteReplaceCycle : MonoBehaviour 
	{
		public Sprite[] Sprites;
		public int StartIndex = 0;

		protected Image _image;
		protected MMTouchButton _mmTouchButton;
		protected int _currentIndex = 0;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			_mmTouchButton = GetComponent<MMTouchButton> ();
			if (_mmTouchButton != null)
			{
				_mmTouchButton.ReturnToInitialSpriteAutomatically = false;
			}
			_image = GetComponent<Image> ();
			if (_image == null) { return; }

			SwitchToIndex(StartIndex);
		}
		public virtual void Swap()
		{
			_currentIndex++;
			if (_currentIndex >= Sprites.Length)
			{
				_currentIndex = 0;
			}
			{
				SwitchToIndex (_currentIndex);
			}
		}
		public virtual void SwitchToIndex(int index)
		{
			if (_image == null) { return; }
			if (Sprites.Length <= index) { return; }
			if (Sprites[index] == null) { return; }
			_image.sprite = Sprites[index];
			_currentIndex = index;
		}


	}
}
