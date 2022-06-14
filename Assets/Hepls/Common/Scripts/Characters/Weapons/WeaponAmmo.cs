using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[RequireComponent(typeof(Weapon))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Ammo")]
    public class WeaponAmmo : MonoBehaviour, MMEventListener<MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates>>, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
	{
		[Header("Ammo")]
		[Tooltip("the ID of this ammo, to be matched on the ammo display if you use one")]
		public string AmmoID;
		[Tooltip("the name of the inventory where the system should look for ammo")]
		public string AmmoInventoryName = "MainInventory";
		[Tooltip("the theoretical maximum of ammo")]
		public int MaxAmmo = 100;
		[Tooltip("if this is true, everytime you equip this weapon, it'll auto fill with ammo")]
		public bool ShouldLoadOnStart = true;
		[Tooltip("if this is true, everytime you equip this weapon, it'll auto fill with ammo")]
		public bool ShouldEmptyOnSave = true;
		[MMReadOnly]
		[Tooltip("the current amount of ammo available in the inventory")]
		public int CurrentAmmoAvailable;
		public Inventory AmmoInventory { get; set; }

		protected Weapon _weapon;
		protected InventoryItem _ammoItem;
		protected bool _emptied = false;
		protected virtual void Start()
		{
			GameObject ammoInventoryTmp = GameObject.Find (AmmoInventoryName);
			if (ammoInventoryTmp != null) { AmmoInventory = ammoInventoryTmp.GetComponent<Inventory> (); }
			_weapon = GetComponent<Weapon> ();
			if (ShouldLoadOnStart)
			{
				LoadOnStart ();	
			}
		}
		protected virtual void LoadOnStart()
		{
			FillWeaponWithAmmo ();
		}
		protected virtual void RefreshCurrentAmmoAvailable()
		{
			CurrentAmmoAvailable = AmmoInventory.GetQuantity (AmmoID);
		}
		public virtual bool EnoughAmmoToFire()
		{
			if (AmmoInventory == null)
			{
				Debug.LogWarning (this.name + " couldn't find the associated inventory. Is there one present in the scene? It should be named '" + AmmoInventoryName + "'.");
				return false;
			}

			RefreshCurrentAmmoAvailable ();

			if (_weapon.MagazineBased)
			{
				if (_weapon.CurrentAmmoLoaded >= _weapon.AmmoConsumedPerShot)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (CurrentAmmoAvailable >= _weapon.AmmoConsumedPerShot)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		protected virtual void ConsumeAmmo()
		{
			if (_weapon.MagazineBased)
			{
				_weapon.CurrentAmmoLoaded = _weapon.CurrentAmmoLoaded - _weapon.AmmoConsumedPerShot;
			}
			else
			{
				for (int i = 0; i < _weapon.AmmoConsumedPerShot; i++)
				{
					AmmoInventory.UseItem (AmmoID);	
					CurrentAmmoAvailable--;
				}	
			}

			if (CurrentAmmoAvailable < _weapon.AmmoConsumedPerShot)
            {
                if (_weapon.AutoDestroyWhenEmpty)
                {
                    StartCoroutine(_weapon.WeaponDestruction());
                }
            }
        }
		public virtual void FillWeaponWithAmmo()
		{
			if (AmmoInventory != null)
			{
				RefreshCurrentAmmoAvailable ();
			}

			if (_weapon.MagazineBased)
			{
                int counter = 0;
				int stock = CurrentAmmoAvailable - _weapon.CurrentAmmoLoaded;
                
				for (int i = _weapon.CurrentAmmoLoaded; i < _weapon.MagazineSize; i++)
				{
					if (stock > 0) 
					{
						stock--;
						counter++;
						
						AmmoInventory.UseItem (AmmoID);	
                    }									
				}
				_weapon.CurrentAmmoLoaded += counter;
			}

			if (_ammoItem == null)
			{
				List<int> list = AmmoInventory.InventoryContains(AmmoID);
				if (list.Count > 0)
				{
					_ammoItem = AmmoInventory.Content[list[list.Count - 1]];
				}
			}
			
			RefreshCurrentAmmoAvailable();
		}
        public virtual void EmptyMagazine()
        {
	        if (AmmoInventory != null)
	        {
		        RefreshCurrentAmmoAvailable ();
	        }

	        if ((_ammoItem == null) || (AmmoInventory == null))
	        {
		        return;
	        }

	        if (_emptied)
	        {
		        return;
	        }

	        if (_weapon.MagazineBased)
	        {
		        int stock = _weapon.CurrentAmmoLoaded;
		        int counter = 0;
                
		        for (int i = 0; i < stock; i++)
		        {
			        AmmoInventory.AddItem(_ammoItem, 1);
			        counter++;
		        }
		        _weapon.CurrentAmmoLoaded -= counter;

		        if (AmmoInventory.Persistent)
		        {
			        AmmoInventory.SaveInventory();
		        }
	        }
	        RefreshCurrentAmmoAvailable();
	        _emptied = true;
        }
		public virtual void OnMMEvent(MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates> weaponEvent)
		{
			if (weaponEvent.Target != this.gameObject)
			{
				return;
			}

			switch (weaponEvent.NewState)
			{
				case MoreMountains.TopDownEngine.Weapon.WeaponStates.WeaponUse:
					ConsumeAmmo ();
					break;

				case MoreMountains.TopDownEngine.Weapon.WeaponStates.WeaponReloadStop:
					FillWeaponWithAmmo();
					break;
			}
		}
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
	        switch (inventoryEvent.InventoryEventType)
	        {
		        case MMInventoryEventType.Pick:
			        if (inventoryEvent.EventItem.ItemClass == ItemClasses.Ammo)
			        {
				        RefreshCurrentAmmoAvailable ();
			        }
			        break;				
	        }
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
	        switch (gameEvent.EventName)
	        {
		        case "Save":
			        if (ShouldEmptyOnSave)
			        {
				        EmptyMagazine();    
			        }
			        break;				
	        }
        }

        protected void OnDestroy()
        {
	        EmptyMagazine();
        }
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates>>();
			this.MMEventStartListening<MMInventoryEvent> ();
			this.MMEventStartListening<MMGameEvent>();
		}
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates>>();
			this.MMEventStopListening<MMInventoryEvent> ();
			this.MMEventStartListening<MMGameEvent>();
		}
	}
}