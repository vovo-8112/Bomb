using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
	public class InventoryHotbar : InventoryDisplay 
	{
		public enum HotbarPossibleAction { Use, Equip }
		[Header("Hotbar")]

		[MMInformation("Here you can define the keys your hotbar will listen to to activate the hotbar's action.",MMInformationAttribute.InformationType.Info,false)]
		public string HotbarKey;
		public string HotbarAltKey;
		public HotbarPossibleAction ActionOnKey	;
		public virtual void Action()
		{
			for (int i = TargetInventory.Content.Length-1 ; i>=0 ; i--)
			{
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					if ((ActionOnKey == HotbarPossibleAction.Equip) && (SlotContainer[i] != null))
					{
						SlotContainer[i].MMGetComponentNoAlloc<InventorySlot>().Equip();
					}
					if ((ActionOnKey == HotbarPossibleAction.Use) && (SlotContainer[i] != null))
					{
						SlotContainer[i].MMGetComponentNoAlloc<InventorySlot>().Use();
					}
					return;
				}
			}
		}
	}
}