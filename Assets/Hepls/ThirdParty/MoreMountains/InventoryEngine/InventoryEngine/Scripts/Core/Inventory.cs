using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.InventoryEngine
{
    [Serializable]
    public class Inventory : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
    {
        public enum InventoryTypes { Main, Equipment }

        [Header("Debug")]
        [MMInformation("The Inventory component is like the database and controller part of your inventory. It won't show anything on screen, you'll need also an InventoryDisplay for that. Here you can decide whether or not you want to output a debug content in the inspector (useful for debugging).", MMInformationAttribute.InformationType.Info, false)]
        public bool DrawContentInInspector = false;
        [MMInformation("This is a realtime view of your Inventory's contents. Don't modify this list via the inspector, it's visible for control purposes only.", MMInformationAttribute.InformationType.Info, false)]
        public InventoryItem[] Content;

        [Header("Inventory Type")]
        [MMInformation("Here you can define your inventory's type. Main are 'regular' inventories. Equipment inventories will be bound to a certain item class and have dedicated options.", MMInformationAttribute.InformationType.Info, false)]
        public InventoryTypes InventoryType = InventoryTypes.Main;

        [Header("Target Transform")]
        [MMInformation("The TargetTransform is any transform in your scene at which objects dropped from the inventory will spawn.", MMInformationAttribute.InformationType.Info, false)]
        public Transform TargetTransform;

        [Header("Persistency")]
        [MMInformation("Here you can define whether or not this inventory should respond to Load and Save events. If you don't want to have your inventory saved to disk, set this to false. You can also have it reset on start, to make sure it's always empty at the start of this level.", MMInformationAttribute.InformationType.Info, false)]
        public bool Persistent = true;
        public bool ResetThisInventorySaveOnStart = false;
        public GameObject Owner { get; set; }
        public int NumberOfFreeSlots { get { return Content.Length - NumberOfFilledSlots; } }
        public int NumberOfFilledSlots
        {
            get
            {
                int numberOfFilledSlots = 0;
                for (int i = 0; i < Content.Length; i++)
                {
                    if (!InventoryItem.IsNull(Content[i]))
                    {
                        numberOfFilledSlots++;
                    }
                }
                return numberOfFilledSlots;
            }
        }

        public int NumberOfStackableSlots(string searchedName, int maxStackSize)
        {
            int numberOfStackableSlots = 0;
            int i = 0;

            while (i < Content.Length)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    numberOfStackableSlots += maxStackSize;
                }
                else
                {
                    if (Content[i].ItemID == searchedName)
                    {
                        numberOfStackableSlots += maxStackSize - Content[i].Quantity;
                    }
                }
                i++;
            }

            return numberOfStackableSlots;
        }

        public const string _resourceItemPath = "Items/";
        protected const string _saveFolderName = "InventoryEngine/";
        protected const string _saveFileExtension = ".inventory";
        public virtual void SetOwner(GameObject newOwner)
        {
            Owner = newOwner;
        }
        public virtual bool AddItem(InventoryItem itemToAdd, int quantity)
        {
            if (itemToAdd == null)
            {
                Debug.LogWarning(this.name + " : The item you want to add to the inventory is null");
                return false;
            }

            List<int> list = InventoryContains(itemToAdd.ItemID);
            if (list.Count > 0 && itemToAdd.MaximumStack > 1)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (Content[list[i]].Quantity < itemToAdd.MaximumStack)
                    {
                        Content[list[i]].Quantity += quantity;
                        if (Content[list[i]].Quantity > Content[list[i]].MaximumStack)
                        {
                            InventoryItem restToAdd = itemToAdd;
                            int restToAddQuantity = Content[list[i]].Quantity - Content[list[i]].MaximumStack;
                            Content[list[i]].Quantity = Content[list[i]].MaximumStack;
                            AddItem(restToAdd, restToAddQuantity);
                        }
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                        return true;
                    }
                }
            }
            if (NumberOfFilledSlots >= Content.Length)
            {
                return false;
            }
            while (quantity > 0)
            {
                if (quantity > itemToAdd.MaximumStack)
                {
                    AddItem(itemToAdd, itemToAdd.MaximumStack);
                    quantity -= itemToAdd.MaximumStack;
                }
                else
                {
                    AddItemToArray(itemToAdd, quantity);
                    quantity = 0;
                }
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
            return true;
        }
        public virtual bool AddItemAt(InventoryItem itemToAdd, int quantity, int destinationIndex)
        {
            if (!InventoryItem.IsNull(Content[destinationIndex]))
            {
                return false;
            }

            int tempQuantity = quantity;
            if (tempQuantity > itemToAdd.MaximumStack)
            {
                tempQuantity = itemToAdd.MaximumStack;
            }
            
            Content[destinationIndex] = itemToAdd.Copy();
            Content[destinationIndex].Quantity = tempQuantity;
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
            return true;
        }
        public virtual bool MoveItem(int startIndex, int endIndex)
        {
            bool swap = false;
            if (InventoryItem.IsNull(Content[startIndex]))
            {
                Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
                return false;
            }
            if (Content[startIndex].CanSwapObject)
            {
                if (!InventoryItem.IsNull(Content[endIndex]))
                {
                    if (Content[endIndex].CanSwapObject)
                    {
                        swap = true;
                    }
                }
            }
            if (InventoryItem.IsNull(Content[endIndex]))
            {
                Content[endIndex] = Content[startIndex].Copy();
                RemoveItemFromArray(startIndex);
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                return true;
            }
            else
            {
                if (swap)
                {
                    InventoryItem tempItem = Content[endIndex].Copy();
                    Content[endIndex] = Content[startIndex].Copy();
                    Content[startIndex] = tempItem;
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public virtual bool MoveItemToInventory(int startIndex, Inventory targetInventory, int endIndex = -1)
        {
            if (InventoryItem.IsNull(Content[startIndex]))
            {
                Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
                return false;
            }
            if ( (endIndex >=0) && (!InventoryItem.IsNull(targetInventory.Content[endIndex])) )
            {
                Debug.LogWarning("InventoryEngine : the destination slot isn't empty, can't move.");
                return false;
            }

            InventoryItem itemToMove = Content[startIndex].Copy();
            if (endIndex >= 0)
            {
                targetInventory.AddItemAt(itemToMove, itemToMove.Quantity, endIndex);    
            }
            else
            {
                targetInventory.AddItem(itemToMove, itemToMove.Quantity);
            }
            RemoveItem(startIndex, itemToMove.Quantity);

            return true;
        }
        public virtual bool RemoveItem(int i, int quantity)
        {
            if (InventoryItem.IsNull(Content[i]))
            {
                Debug.LogWarning("InventoryEngine : you're trying to remove from an empty slot.");
                return false;
            }
            Content[i].Quantity -= quantity;
            if (Content[i].Quantity <= 0)
            {
                bool suppressionSuccessful = RemoveItemFromArray(i);
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                return suppressionSuccessful;
            }
            else
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
                return true;
            }
        }
        public virtual bool DestroyItem(int i)
        {
            Content[i] = null;

            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
            return true;
        }
        public virtual void EmptyInventory()
        {
            Content = new InventoryItem[Content.Length];

            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0);
        }
        protected virtual bool AddItemToArray(InventoryItem itemToAdd, int quantity)
        {
            if (NumberOfFreeSlots == 0)
            {
                return false;
            }
            int i = 0;
            while (i < Content.Length)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    Content[i] = itemToAdd.Copy();
                    Content[i].Quantity = quantity;
                    return true;
                }
                i++;
            }
            return false;
        }
        protected virtual bool RemoveItemFromArray(int i)
        {
            if (i < Content.Length)
            {
                Content[i].ItemID = null;
                Content[i] = null;
                return true;
            }
            return false;
        }
        public virtual void ResizeArray(int newSize)
        {
            InventoryItem[] temp = new InventoryItem[newSize];
            for (int i = 0; i < Mathf.Min(newSize, Content.Length); i++)
            {
                temp[i] = Content[i];
            }
            Content = temp;
        }
        public virtual int GetQuantity(string searchedName)
        {
            List<int> list = InventoryContains(searchedName);
            int total = 0;
            foreach (int i in list)
            {
                total += Content[i].Quantity;
            }
            return total;
        }
        public virtual List<int> InventoryContains(string searchedName)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (!InventoryItem.IsNull(Content[i]))
                {
                    if (Content[i].ItemID == searchedName)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }
        public virtual List<int> InventoryContains(MoreMountains.InventoryEngine.ItemClasses searchedClass)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    continue;
                }
                if (Content[i].ItemClass == searchedClass)
                {
                    list.Add(i);
                }
            }
            return list;
        }
        public virtual void SaveInventory()
        {
            SerializedInventory serializedInventory = new SerializedInventory();
            FillSerializedInventory(serializedInventory);
            MMSaveLoadManager.Save(serializedInventory, gameObject.name + _saveFileExtension, _saveFolderName);
        }
        public virtual void LoadSavedInventory()
        {
            SerializedInventory serializedInventory = (SerializedInventory)MMSaveLoadManager.Load(typeof(SerializedInventory), gameObject.name + _saveFileExtension, _saveFolderName);
            ExtractSerializedInventory(serializedInventory);
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryLoaded, null, this.name, null, 0, 0);
        }
        protected virtual void FillSerializedInventory(SerializedInventory serializedInventory)
        {
            serializedInventory.InventoryType = InventoryType;
            serializedInventory.DrawContentInInspector = DrawContentInInspector;
            serializedInventory.ContentType = new string[Content.Length];
            serializedInventory.ContentQuantity = new int[Content.Length];
            for (int i = 0; i < Content.Length; i++)
            {
                if (!InventoryItem.IsNull(Content[i]))
                {
                    serializedInventory.ContentType[i] = Content[i].ItemID;
                    serializedInventory.ContentQuantity[i] = Content[i].Quantity;
                }
                else
                {
                    serializedInventory.ContentType[i] = null;
                    serializedInventory.ContentQuantity[i] = 0;
                }
            }
        }

        protected InventoryItem _loadedInventoryItem;
        protected virtual void ExtractSerializedInventory(SerializedInventory serializedInventory)
        {
            if (serializedInventory == null)
            {
                return;
            }

            InventoryType = serializedInventory.InventoryType;
            DrawContentInInspector = serializedInventory.DrawContentInInspector;
            Content = new InventoryItem[serializedInventory.ContentType.Length];
            for (int i = 0; i < serializedInventory.ContentType.Length; i++)
            {
                if ((serializedInventory.ContentType[i] != null) && (serializedInventory.ContentType[i] != ""))
                {
                    _loadedInventoryItem = Resources.Load<InventoryItem>(_resourceItemPath + serializedInventory.ContentType[i]);
                    if (_loadedInventoryItem == null)
                    {
                        Debug.LogError("InventoryEngine : Couldn't find any inventory item to load at "+_resourceItemPath
                            +" named "+serializedInventory.ContentType[i] + ". Make sure all your items definitions names (the name of the InventoryItem scriptable " +
                            "objects) are exactly the same as their ItemID string in their inspector. " +
                            "Once that's done, also make sure you reset all saved inventories as the mismatched names and IDs may have " +
                            "corrupted them.");
                    }
                    else
                    {
                        Content[i] = _loadedInventoryItem.Copy();
                        Content[i].Quantity = serializedInventory.ContentQuantity[i];
                    }
                }
                else
                {
                    Content[i] = null;
                }
            }
        }
        public virtual void ResetSavedInventory()
        {
            MMSaveLoadManager.DeleteSave(gameObject.name + _saveFileExtension, _saveFolderName);
            Debug.LogFormat("save file deleted");
        }
        public virtual bool UseItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return false;
            }
            if (!item.IsUsable)
            {
                return false;
            }
            if (item.Use())
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.ItemUsed, slot, this.name, item.Copy(), 0, index);
                if (item.Consumable)
                {
                    RemoveItem(index, item.ConsumeQuantity);    
                }
            }
            return true;
        }
        public virtual bool UseItem(string itemName)
        {
            List<int> list = InventoryContains(itemName);
            if (list.Count > 0)
            {
                UseItem(Content[list[list.Count - 1]], list[list.Count - 1], null);
                return true;
            }
            else
            {
                return false;
            }
        }
        public virtual void EquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryType == Inventory.InventoryTypes.Main)
            {
                InventoryItem oldItem = null;
                if (InventoryItem.IsNull(item))
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                    return;
                }
                if (!item.IsEquippable)
                {
                    return;
                }
                if (item.TargetEquipmentInventory == null)
                {
                    Debug.LogWarning("InventoryEngine Warning : " + Content[index].ItemName + "'s target equipment inventory couldn't be found.");
                    return;
                }
                if (!item.CanMoveObject)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                    return;
                }

                if (!item.Equip())
                {
                    return;
                }
                if (item.TargetEquipmentInventory.Content.Length == 1)
                {
                    if (!InventoryItem.IsNull(item.TargetEquipmentInventory.Content[0]))
                    {
                        if (
                            (item.CanSwapObject)
                            && (item.TargetEquipmentInventory.Content[0].CanMoveObject)
                            && (item.TargetEquipmentInventory.Content[0].CanSwapObject)
                        )
                        {
                            oldItem = item.TargetEquipmentInventory.Content[0].Copy();
                            item.TargetEquipmentInventory.EmptyInventory();
                        }
                    }
                }
                item.TargetEquipmentInventory.AddItem(item.Copy(), item.Quantity);
                RemoveItem(index, item.Quantity);
                if (oldItem != null)
                {
                    oldItem.Swap();
                    AddItem(oldItem, oldItem.Quantity);
                }
                MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, slot, this.name, item, item.Quantity, index);
            }
        }
        public virtual void DropItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            item.SpawnPrefab();
            
            if (this.name == item.TargetEquipmentInventoryName)
            {
                if (item.UnEquip())
                {
                    DestroyItem(index);
                }
            } else
            {
                DestroyItem(index);
            }

        }

        public virtual void DestroyItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            DestroyItem(index);
        }

        public virtual void UnEquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            if (InventoryType != InventoryTypes.Equipment)
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index);
                return;
            }
            if (!item.UnEquip())
            {
                return;
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, item, item.Quantity, index);
            if (item.TargetInventory != null)
            {
                if (item.TargetInventory.AddItem(item, item.Quantity))
                {
                    DestroyItem(index);
                }
                else
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Drop, slot, this.name, item, item.Quantity, index);
                }
            }
        }
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (inventoryEvent.TargetInventoryName != this.name)
            {
                return;
            }
            switch (inventoryEvent.InventoryEventType)
            {
                case MMInventoryEventType.Pick:
                    AddItem(inventoryEvent.EventItem, inventoryEvent.Quantity);
                    break;

                case MMInventoryEventType.UseRequest:
                    UseItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.EquipRequest:
                    EquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.UnEquipRequest:
                    UnEquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Destroy:
                    DestroyItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Drop:
                    DropItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;
            }
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if ((gameEvent.EventName == "Save") && Persistent)
            {
                SaveInventory();
            }
            if ((gameEvent.EventName == "Load") && Persistent)
            {
                if (ResetThisInventorySaveOnStart)
                {
                    ResetSavedInventory();
                }
                LoadSavedInventory();
            }
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<MMInventoryEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<MMInventoryEvent>();
        }
    }
}