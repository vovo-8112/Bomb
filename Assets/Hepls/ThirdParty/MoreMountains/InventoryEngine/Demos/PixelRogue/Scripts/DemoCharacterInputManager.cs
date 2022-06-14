using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace MoreMountains.InventoryEngine
{
	public class DemoCharacterInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[MMInformation("This component is a very simple input manager that handles the demo character's input and makes it move. If you remove it from the scene your character won't move anymore.", MMInformationAttribute.InformationType.Info,false)]
		public InventoryDemoCharacter DemoCharacter ;

		protected bool _pause = false;
		protected virtual void Update ()
		{
			HandleDemoCharacterInput();
		}
		protected virtual void HandleDemoCharacterInput()
		{
			if (_pause)
			{
				DemoCharacter.SetMovement(0,0);
				return;
			}
			DemoCharacter.SetMovement(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
		}
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryOpens)
			{
				_pause = true;
			}
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloses)
			{
				_pause = false;
			}
		}
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}