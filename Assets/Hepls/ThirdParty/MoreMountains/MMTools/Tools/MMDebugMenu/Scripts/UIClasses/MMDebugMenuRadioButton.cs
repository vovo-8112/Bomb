using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public class MMDebugMenuRadioButton : MMDebugMenuSpriteReplace 
	{
		public string RadioButtonGroupName;

		protected List<MMDebugMenuRadioButton> _group;
		public override void Initialization()
		{
			base.Initialization ();
			FindAllRadioButtonsFromTheSameGroup ();
		}
		protected virtual void FindAllRadioButtonsFromTheSameGroup ()
		{
			_group = new List<MMDebugMenuRadioButton> ();

            MMDebugMenuRadioButton[] radioButtons = FindObjectsOfType(typeof(MMDebugMenuRadioButton)) as MMDebugMenuRadioButton[];
			foreach (MMDebugMenuRadioButton radioButton in radioButtons) 
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
				foreach (MMDebugMenuRadioButton radioButton in _group) 
				{
					radioButton.SwitchToOffSprite ();
				}	
			}
		}
	}
}
