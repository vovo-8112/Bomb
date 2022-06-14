using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Environment/Key Operated Zone")]
	public class KeyOperatedZone : ButtonActivated 
	{
		[Header("Key")]
		[Tooltip("whether this zone actually requires a key")]
		public bool RequiresKey = true;
		[Tooltip("the key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory")]
		public string KeyID;
		[Tooltip("the method that should be triggered when the key is used")]
		public UnityEvent KeyAction;
        
		protected GameObject _collidingObject;
		protected List<int> _keyList;
		protected virtual void Start()
		{
			_keyList = new List<int> ();
        }
		protected override void OnTriggerEnter2D(Collider2D collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter2D (collider);
		}

        protected override void OnTriggerEnter(Collider collider)
        {
            _collidingObject = collider.gameObject;
            base.OnTriggerEnter(collider);
        }
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
            {
                PromptError();
                return;
			}

			if (_collidingObject == null) { return; }

			if (RequiresKey)
			{
				CharacterInventory characterInventory = _collidingObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterInventory> ();
				if (characterInventory == null)
				{
                    PromptError();
                    return;
				}	

				_keyList.Clear ();
				_keyList = characterInventory.MainInventory.InventoryContains (KeyID);
				if (_keyList.Count == 0)
				{
                    PromptError();
                    return;
				}
				else
				{
					base.TriggerButtonAction ();
					characterInventory.MainInventory.UseItem(KeyID);
				}
			}

            TriggerKeyAction ();
			ActivateZone ();
		}
		protected virtual void TriggerKeyAction()
		{
			if (KeyAction != null)
			{
				KeyAction.Invoke ();
			}
		}
	}
}
