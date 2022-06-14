using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
	public class ChangeLevel : MonoBehaviour 
	{
		[MMInformation("This demo component, when added to a BoxCollider2D, will change the scene to the one specified in the field below when the character enters the collider.", MMInformationAttribute.InformationType.Info,false)]
		public string Destination;
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			if ((Destination != null) && (collider.gameObject.GetComponent<InventoryDemoCharacter>() != null))
			{
				MMGameEvent.Trigger("Save");
				SceneManager.LoadScene(Destination);
			}
		}
	}
}