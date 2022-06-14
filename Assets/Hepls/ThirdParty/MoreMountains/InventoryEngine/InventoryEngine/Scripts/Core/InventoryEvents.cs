using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
	public enum MMInventoryEventType { Pick, Select, Click, Move, UseRequest, ItemUsed, EquipRequest, ItemEquipped, UnEquipRequest, ItemUnEquipped, Drop, Destroy, Error, Redraw, ContentChanged, InventoryOpens, InventoryCloseRequest, InventoryCloses, InventoryLoaded }
	public struct MMInventoryEvent
	{
		public MMInventoryEventType InventoryEventType;
		public InventorySlot Slot;
		public string TargetInventoryName;
		public InventoryItem EventItem;
		public int Quantity;
		public int Index;

		public MMInventoryEvent(MMInventoryEventType eventType, InventorySlot slot, string targetInventoryName, InventoryItem eventItem, int quantity, int index)
		{
			InventoryEventType = eventType;
			Slot = slot;
			TargetInventoryName = targetInventoryName;
			EventItem = eventItem;
			Quantity = quantity;
			Index = index;
		}

        static MMInventoryEvent e;
        public static void Trigger(MMInventoryEventType eventType, InventorySlot slot, string targetInventoryName, InventoryItem eventItem, int quantity, int index)
        {
            e.InventoryEventType = eventType;
            e.Slot = slot;
            e.TargetInventoryName = targetInventoryName;
            e.EventItem = eventItem;
            e.Quantity = quantity;
            e.Index = index;
            MMEventManager.TriggerEvent(e);
        }
    }
}
