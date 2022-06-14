using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
	public class ItemPicker : MonoBehaviour 
	{
		[MMInformation("Add this component to a Trigger box collider 2D and it'll make it pickable, and will add the specified item to its target inventory. Just drag a previously created item into the slot below. For more about how to create items, have a look at the documentation. Here you can also specify how many of that item should be picked when picking the object.",MMInformationAttribute.InformationType.Info,false)]
		public InventoryItem Item ;
		[Header("Pick Quantity")]
		public int Quantity = 1;
		[Header("Conditions")]
		public bool PickableIfInventoryIsFull = false;
		public bool DisableObjectWhenDepleted = false;

		protected int _pickedQuantity = 0;

		protected Inventory _targetInventory;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			FindTargetInventory (Item.TargetInventoryName);
		}
        public virtual void OnTriggerEnter(Collider collider)
        {
            if (!collider.CompareTag("Player"))
            {
                return;
            }

            Pick(Item.TargetInventoryName);
        }
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
            if (!collider.CompareTag("Player"))
			{
				return;
			}

			Pick(Item.TargetInventoryName);
		}
		public virtual void Pick()
		{
			Pick(Item.TargetInventoryName);
		}
		public virtual void Pick(string targetInventoryName)
		{
			FindTargetInventory (targetInventoryName);
			if (_targetInventory==null)
			{
				return;
			}

			if (!Pickable()) 
			{
				PickFail ();
				return;
			}

			DetermineMaxQuantity ();
			if (!Application.isPlaying)
			{
				_targetInventory.AddItem(Item, 1);
			}				
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, Item.TargetInventoryName, Item, _pickedQuantity, 0);
			}				
			if (Item.Pick())
            {
                Quantity = Quantity - _pickedQuantity;
                PickSuccess();
                DisableObjectIfNeeded();
            }			
		}
		protected virtual void PickSuccess()
		{
			
		}
		protected virtual void PickFail()
		{

		}
		protected virtual void DisableObjectIfNeeded()
		{
			if (DisableObjectWhenDepleted && Quantity <= 0)
			{
				gameObject.SetActive(false);	
			}
		}
		protected virtual void DetermineMaxQuantity()
		{
			_pickedQuantity = _targetInventory.NumberOfStackableSlots (Item.ItemID, Item.MaximumStack);
			if (Quantity < _pickedQuantity)
			{
				_pickedQuantity = Quantity;
			}
		}
		public virtual bool Pickable()
		{
			if (!PickableIfInventoryIsFull && _targetInventory.NumberOfFreeSlots == 0)
			{
				return false;
			}

			return true;
		}
		public virtual void FindTargetInventory(string targetInventoryName)
		{
			_targetInventory = null;
			if (targetInventoryName==null)
			{
				return;
			}
			foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
			{				
				if (inventory.name==targetInventoryName)
				{
					_targetInventory = inventory;
				}
			}
		}
	}
}