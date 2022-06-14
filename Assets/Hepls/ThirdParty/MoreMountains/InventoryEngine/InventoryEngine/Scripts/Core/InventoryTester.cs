using System.Collections;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    public class InventoryTester : MonoBehaviour
    {
        [Header("Add item")]
        public InventoryItem AddItem;
        public int AddItemQuantity;
        public Inventory AddItemInventory;
        [MMInspectorButton("AddItemTest")]
        public bool AddItemTestButton;
        
        [Header("Add item at")]
        public InventoryItem AddItemAtItem;
        public int AddItemAtQuantity;
        public int AddItemAtIndex;
        public Inventory AddItemAtInventory;
        [MMInspectorButton("AddItemAtTest")] 
        public bool AddItemAtTestButton;

        [Header("Move Item")]
        public int MoveItemOrigin;
        public int MoveItemDestination;
        public Inventory MoveItemInventory;
        [MMInspectorButton("MoveItemTest")] 
        public bool MoveItemTestButton;

        [Header("Move Item To Inventory")]
        public int MoveItemToInventoryOriginIndex;
        public int MoveItemToInventoryDestinationIndex = -1;
        public Inventory MoveItemToOriginInventory;
        public Inventory MoveItemToDestinationInventory;
        [MMInspectorButton("MoveItemToInventory")] 
        public bool MoveItemToInventoryTestButton;

        [Header("Remove Item")]
        public int RemoveItemIndex;
        public int RemoveItemQuantity;
        public Inventory RemoveItemInventory;
        [MMInspectorButton("RemoveItemTest")] 
        public bool RemoveItemTestButton;

        [Header("Empty Inventory")]
        public Inventory EmptyTargetInventory;
        [MMInspectorButton("EmptyInventoryTest")] 
        public bool EmptyInventoryTestButton;
        protected virtual void AddItemTest()
        {
            AddItemInventory.AddItem(AddItem, AddItemQuantity);
        }
        protected virtual void AddItemAtTest()
        {
            AddItemAtInventory.AddItemAt(AddItemAtItem, AddItemAtQuantity, AddItemAtIndex);
        }
        protected virtual void MoveItemTest()
        {
            MoveItemInventory.MoveItem(MoveItemOrigin, MoveItemDestination);
        }
        protected virtual void MoveItemToInventory()
        {
            MoveItemToOriginInventory.MoveItemToInventory(MoveItemToInventoryOriginIndex, MoveItemToDestinationInventory, MoveItemToInventoryDestinationIndex);
        }
        protected virtual void RemoveItemTest()
        {
            RemoveItemInventory.RemoveItem(RemoveItemIndex, RemoveItemQuantity);
        }
        protected virtual void EmptyInventoryTest()
        {
            EmptyTargetInventory.EmptyInventory();
        }
    }
}