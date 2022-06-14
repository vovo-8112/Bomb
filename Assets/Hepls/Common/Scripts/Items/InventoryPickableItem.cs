using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Items/InventoryPickableItem")]
    public class InventoryPickableItem : ItemPicker 
	{
		[Tooltip("The effect to instantiate when the coin is hit")]
		public GameObject Effect;
		[Tooltip("The sound effect to play when the object gets picked")]
		public AudioClip PickSfx;

		protected override void PickSuccess()
		{
			base.PickSuccess ();
			Effects ();
		}
		protected virtual void Effects()
		{
			if (!Application.isPlaying)
			{
				return;
			}				
			else
			{
				if (PickSfx!=null) 
				{	
					MMSoundManagerSoundPlayEvent.Trigger(PickSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
				}

				if (Effect != null)
				{
					Instantiate(Effect, transform.position, transform.rotation);				
				}	
			}
		}
	}
}
