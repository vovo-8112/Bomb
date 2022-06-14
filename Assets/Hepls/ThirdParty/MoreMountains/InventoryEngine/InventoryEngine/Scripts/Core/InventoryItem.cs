using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;


namespace MoreMountains.InventoryEngine
{
	[Serializable]
	public class InventoryItem : ScriptableObject 
	{
		[Header("ID and Target")]
		[MMInformation("The unique name of your object.",MMInformationAttribute.InformationType.Info,false)]
		public string ItemID;
		public string TargetInventoryName = "MainInventory";

		[Header("Permissions")]
		[MMInformation("Here you can determine whether your object is Usable, Equippable, or both. Usable objects are typically bombs, potions, stuff like that. Equippables are usually weapons or armor.",MMInformationAttribute.InformationType.Info,false)]
		public bool Usable = false;
		[MMCondition("Usable", true)] 
		public bool Consumable = true;
		[MMCondition("Consumable", true)] 
		public int ConsumeQuantity = 1;
        public bool Equippable = false;
		public bool Droppable = true;
		public bool CanMoveObject=true;
		public bool CanSwapObject=true;
        public virtual bool IsUsable {  get { return Usable;  } }
        public virtual bool IsEquippable { get { return Equippable; } }

        [HideInInspector]
		public int Quantity = 1;


		[Header("Basic info")]
		[MMInformation("The name of the item as you want it to appear in the display panel",MMInformationAttribute.InformationType.Info,false)]
		public string ItemName;
		[TextArea]
		[MMInformation("The Short and 'long' descriptions will be used to display in the InventoryDetails panel.",MMInformationAttribute.InformationType.Info,false)]
		public string ShortDescription;
		[TextArea]
		public string Description;

		[Header("Image")]
		[MMInformation("The image that will be displayed inside InventoryDisplay panels and InventoryDetails.",MMInformationAttribute.InformationType.Info,false)]
		public Sprite Icon;

		[Header("Prefab Drop")]
		[MMInformation("The prefab that will be spawned in the scene should the item be dropped from its inventory. Here you can also specify the min and max distance at which the prefab should be spawned.",MMInformationAttribute.InformationType.Info,false)]
		public GameObject Prefab;
		public MMSpawnAroundProperties DropProperties;

		[Header("Inventory Properties")]
		[MMInformation("If this object can be stacked (multiple instances in a single inventory slot), you can specify here the maximum size of that stack. You can also specify the item class (useful for equipment items mostly)",MMInformationAttribute.InformationType.Info,false)]
		public int MaximumStack = 1;
		public ItemClasses ItemClass;

		[Header("Equippable")]
		[MMInformation("If this item is equippable, you can set here its target inventory name (for example ArmorInventory). Of course you'll need an inventory with a matching name in your scene. You can also specify a sound to play when this item is equipped. If you don't, a default sound will be played.",MMInformationAttribute.InformationType.Info,false)]
		public string TargetEquipmentInventoryName;
		public AudioClip EquippedSound;

		[Header("Usable")]
		[MMInformation("If this item can be used, you can set here a sound to play when it gets used, if you don't a default sound will be played.",MMInformationAttribute.InformationType.Info,false)]
		public AudioClip UsedSound;

		[Header("Sounds")]
		[MMInformation("Here you can override the default sounds for move and drop events.",MMInformationAttribute.InformationType.Info,false)]
		public AudioClip MovedSound;
		public AudioClip DroppedSound;
		public bool UseDefaultSoundsIfNull = true;

		protected Inventory _targetInventory = null;
		protected Inventory _targetEquipmentInventory = null;
		public virtual Inventory TargetInventory 
		{ 
			get 
			{ 
				if (TargetInventoryName==null)
				{
					return null;
				}
				if (_targetInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if (inventory.name == TargetInventoryName)
						{
							_targetInventory = inventory;
						}
					}	
				}
				return _targetInventory;
			}
		}
		public virtual Inventory TargetEquipmentInventory
		{ 
			get 
			{ 
				if (TargetEquipmentInventoryName == null)
				{
					return null;
				}
				if (_targetEquipmentInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if (inventory.name == TargetEquipmentInventoryName)
						{
							_targetEquipmentInventory = inventory;
						}
					}	
				}
				return _targetEquipmentInventory;
			}
		}
		public static bool IsNull(InventoryItem item)
		{
			if (item==null)
			{
				return true;
			}
			if (item.ItemID==null)
			{
				return true;
			}
			if (item.ItemID=="")
			{
				return true;
			}
			return false;
		}
		public virtual InventoryItem Copy()
		{
			string name = this.name;
			InventoryItem clone = UnityEngine.Object.Instantiate(this) as InventoryItem;
			clone.name = name;
			return clone;
		}
		public virtual void SpawnPrefab()
		{
			if (TargetInventory != null)
			{
				if (Prefab!=null && TargetInventory.TargetTransform!=null)
				{
					GameObject droppedObject=(GameObject)Instantiate(Prefab);
					if (droppedObject.GetComponent<ItemPicker>()!=null)
					{
						droppedObject.GetComponent<ItemPicker>().Quantity=Quantity;
					}

					MMSpawnAround.ApplySpawnAroundProperties(droppedObject, DropProperties,
						TargetInventory.TargetTransform.position);
				}
			}
		}
		public virtual bool Pick() { return true; }
		public virtual bool Use() { return true; }
		public virtual bool Equip() { return true; }
		public virtual bool UnEquip() { return true; }
		public virtual void Swap() {}
		public virtual bool Drop() { return true; }
	}
}