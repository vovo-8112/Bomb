using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
	public class InventoryDemoGameManager : MMSingleton<InventoryDemoGameManager> 
	{
		public InventoryDemoCharacter Player { get; protected set; }

		protected override void Awake () 
		{
			base.Awake ();
			Player = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryDemoCharacter>()	;
		}
		protected virtual void Start()
		{
            MMGameEvent.Trigger("Load");
		}
	}
}