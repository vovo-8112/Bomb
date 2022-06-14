using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Items/InventoryEngineChest")]
    public class InventoryEngineChest : MonoBehaviour 
	{
		protected Animator _animator;
		protected ItemPicker[] _itemPickerList;
		protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
			_itemPickerList = GetComponents<ItemPicker> ();
		}
		public virtual void OpenChest()
		{
			TriggerOpeningAnimation ();
			PickChestContents ();
		}
		protected virtual void TriggerOpeningAnimation()
		{
			if (_animator == null)
			{
				return;
			}
			_animator.SetTrigger ("OpenChest");
		}
		protected virtual void PickChestContents()
		{
			if (_itemPickerList.Length == 0)
			{
				return;
			}
			foreach (ItemPicker picker in _itemPickerList)
			{
				picker.Pick ();
			}
		}
			
	}
}
