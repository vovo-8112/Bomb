using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MoreMountains.MMInterface
{
	public class MMRadioButton : MMSpriteReplace 
	{
		public string RadioButtonGroupName;

		protected List<MMRadioButton> _group;
		protected override void Initialization()
		{
			base.Initialization ();
			FindAllRadioButtonsFromTheSameGroup ();
		}
		protected virtual void FindAllRadioButtonsFromTheSameGroup ()
		{
			_group = new List<MMRadioButton> ();

			MMRadioButton[] radioButtons = FindObjectsOfType(typeof(MMRadioButton)) as MMRadioButton[];
			foreach (MMRadioButton radioButton in radioButtons) 
			{
				if ((radioButton.RadioButtonGroupName == RadioButtonGroupName)
					&& (radioButton != this))
				{
					_group.Add (radioButton);
				}
			}
		}
		protected override void SpriteOn()
		{
			base.SpriteOn ();
			if (_group.Count >= 1)
			{
				foreach (MMRadioButton radioButton in _group) 
				{
					radioButton.SwitchToOffSprite ();
				}	
			}
		}
	}
}
