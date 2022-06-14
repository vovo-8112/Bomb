using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [System.Serializable]
    public struct AutoPickItem
    {
        public InventoryItem Item;
        public int Quantity;
    }
    [MMHiddenProperties("AbilityStopFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Inventory")] 
	public class CharacterInventory : CharacterAbility, MMEventListener<MMInventoryEvent>
	{
        public enum WeaponRotationModes { Normal, AddEmptySlot, AddInitialWeapon }

        [Header("Bindings")]
		[Tooltip("the name of the main inventory for this character")]
		public string MainInventoryName;
		[Tooltip("the name of the inventory where this character stores weapons")]
		public string WeaponInventoryName;
		[Tooltip("the name of the hotbar inventory for this character")]
		public string HotbarInventoryName;
		[Tooltip("a transform to pass to the inventories, will be passed to the inventories and used as reference for drops. If left empty, this.transform will be used.")]
		public Transform InventoryTransform;

        [Header("Weapon Rotation")]
		[Tooltip("if this is true, will add an empty slot to the weapon rotation")]
        public WeaponRotationModes WeaponRotationMode = WeaponRotationModes.Normal;

        [Header("AutoEquip")]
		[Tooltip("a list of items to automatically add to this Character's inventories on start")]
        public AutoPickItem[] AutoPickItems;
		[Tooltip("a weapon to auto equip on start")]
        public InventoryWeapon AutoEquipWeaponOnStart;
        [Tooltip("the target handle weapon ability - if left empty, will pick the first one it finds")]
        public CharacterHandleWeapon CharacterHandleWeapon;

        public Inventory MainInventory { get; set; }
		public Inventory WeaponInventory { get; set; }
		public Inventory HotbarInventory { get; set; }

		protected List<int> _availableWeapons;
		protected List<string> _availableWeaponsIDs;
		protected string _nextWeaponID;
        protected bool _nextFrameWeapon = false;
        protected string _nextFrameWeaponName;
        protected const string _emptySlotWeaponName = "_EmptySlotWeaponName";
        protected const string _initialSlotWeaponName = "_InitialSlotWeaponName";
		protected override void Initialization () 
		{
			base.Initialization();
			Setup ();
		}
		protected virtual void Setup()
		{
			if (InventoryTransform == null)
			{
				InventoryTransform = this.transform;
			}
			GrabInventories ();
			if (CharacterHandleWeapon == null)
			{
				CharacterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon> ();	
			}
			FillAvailableWeaponsLists ();
            if (AutoPickItems.Length > 0)
            {
                foreach (AutoPickItem item in AutoPickItems)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, item.Item.TargetInventoryName, item.Item, item.Quantity, 0);
                }
            }
            if (AutoEquipWeaponOnStart != null)
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, AutoEquipWeaponOnStart.TargetInventoryName, AutoEquipWeaponOnStart, 1, 0);
                EquipWeapon(AutoEquipWeaponOnStart.ItemID);
            }
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            
            if (_nextFrameWeapon)
            {
                EquipWeapon(_nextFrameWeaponName);
                _nextFrameWeapon = false;
            }
        }
		protected virtual void GrabInventories()
		{
			if (MainInventory == null)
			{
				GameObject mainInventoryTmp = GameObject.Find (MainInventoryName);
				if (mainInventoryTmp != null) { MainInventory = mainInventoryTmp.GetComponent<Inventory> (); }	
			}
			if (WeaponInventory == null)
			{
				GameObject weaponInventoryTmp = GameObject.Find (WeaponInventoryName);
				if (weaponInventoryTmp != null) { WeaponInventory = weaponInventoryTmp.GetComponent<Inventory> (); }	
			}
			if (HotbarInventory == null)
			{
				GameObject hotbarInventoryTmp = GameObject.Find (HotbarInventoryName);
				if (hotbarInventoryTmp != null) { HotbarInventory = hotbarInventoryTmp.GetComponent<Inventory> (); }	
			}
			if (MainInventory != null) { MainInventory.SetOwner (this.gameObject); MainInventory.TargetTransform = InventoryTransform;}
			if (WeaponInventory != null) { WeaponInventory.SetOwner (this.gameObject); WeaponInventory.TargetTransform = InventoryTransform;}
			if (HotbarInventory != null) { HotbarInventory.SetOwner (this.gameObject); HotbarInventory.TargetTransform = InventoryTransform;}
		}
		protected override void HandleInput()
        {
            if (!AbilityAuthorized
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }
            if (_inputManager.SwitchWeaponButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchWeapon ();
			}
		}
		protected virtual void FillAvailableWeaponsLists()
		{
			_availableWeaponsIDs = new List<string> ();
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}
			_availableWeapons = MainInventory.InventoryContains (ItemClasses.Weapon);
			foreach (int index in _availableWeapons)
			{
				_availableWeaponsIDs.Add (MainInventory.Content [index].ItemID);
			}
			if (!InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_availableWeaponsIDs.Add (WeaponInventory.Content [0].ItemID);
			}

			_availableWeaponsIDs.Sort ();
		}
		protected virtual void DetermineNextWeaponName ()
		{
			if (InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_nextWeaponID = _availableWeaponsIDs [0];
				return;
			}

            if ((_nextWeaponID == _emptySlotWeaponName) || (_nextWeaponID == _initialSlotWeaponName))
            {
                _nextWeaponID = _availableWeaponsIDs[0];
                return;
            }

            for (int i = 0; i < _availableWeaponsIDs.Count; i++)
			{
				if (_availableWeaponsIDs[i] == WeaponInventory.Content[0].ItemID)
				{
					if (i == _availableWeaponsIDs.Count - 1)
					{
                        switch (WeaponRotationMode)
                        {
                            case WeaponRotationModes.AddEmptySlot:
                                _nextWeaponID = _emptySlotWeaponName;
                                return;
                            case WeaponRotationModes.AddInitialWeapon:
                                _nextWeaponID = _initialSlotWeaponName;
                                return;
                        }

						_nextWeaponID = _availableWeaponsIDs [0];
					}
					else
					{
						_nextWeaponID = _availableWeaponsIDs [i+1];
					}
				}
			}
		}
		protected virtual void EquipWeapon(string weaponID)
        {
            if ((weaponID == _emptySlotWeaponName) && (CharacterHandleWeapon != null))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0);
                CharacterHandleWeapon.ChangeWeapon(null, _emptySlotWeaponName, false);
                MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0);
                return;
            }

            if ((weaponID == _initialSlotWeaponName) && (CharacterHandleWeapon != null))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0);
                CharacterHandleWeapon.ChangeWeapon(CharacterHandleWeapon.InitialWeapon, _emptySlotWeaponName, false);
                MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0);
                return;
            }

            for (int i = 0; i < MainInventory.Content.Length ; i++)
			{
				if (InventoryItem.IsNull(MainInventory.Content[i]))
				{
					continue;
				}
				if (MainInventory.Content[i].ItemID == weaponID)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, null, MainInventory.name, MainInventory.Content[i], 0, i);
					break;
				}
			}
		}
		protected virtual void SwitchWeapon()
		{
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}

			FillAvailableWeaponsLists ();
			if (_availableWeaponsIDs.Count <= 0)
			{
				return;
			}

			DetermineNextWeaponName ();
			EquipWeapon (_nextWeaponID);
            PlayAbilityStartFeedbacks();
            PlayAbilityStartSfx();
		}
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == WeaponInventoryName)
				{
					this.Setup ();
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							CharacterHandleWeapon.Setup ();
							WeaponInventory.Content [0].Equip ();
						}
					}
				}
            }
            if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
            {
                bool isSubclass = (inventoryEvent.EventItem.GetType().IsSubclassOf(typeof(InventoryWeapon)));
                bool isClass = (inventoryEvent.EventItem.GetType() == typeof(InventoryWeapon));
                if (isClass || isSubclass)
                {
                    InventoryWeapon inventoryWeapon = (InventoryWeapon)inventoryEvent.EventItem;
                    switch (inventoryWeapon.AutoEquipMode)
                    {
                        case InventoryWeapon.AutoEquipModes.NoAutoEquip:
                            break;

                        case InventoryWeapon.AutoEquipModes.AutoEquip:
                            _nextFrameWeapon = true;
                            _nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
                            break;

                        case InventoryWeapon.AutoEquipModes.AutoEquipIfEmptyHanded:
                            if (CharacterHandleWeapon.CurrentWeapon == null)
                            {
                                _nextFrameWeapon = true;
                                _nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
                            }
                            break;
                    }
                }
            }
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            if (WeaponInventory != null)
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0);
            }            
        }
        protected override void OnEnable()
		{
            base.OnEnable();
			this.MMEventStartListening<MMInventoryEvent>();
		}
		protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}
