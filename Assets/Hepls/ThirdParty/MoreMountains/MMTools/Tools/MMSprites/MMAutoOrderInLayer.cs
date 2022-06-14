using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("More Mountains/Tools/Sprites/MMAutoOrderInLayer")]
    public class MMAutoOrderInLayer : MonoBehaviour 
	{
		static int CurrentMaxCharacterOrderInLayer = 0;

		[Header("Global Counter")]
		[MMInformation("Add this component to an object with a sprite renderer, and it'll give it a new order in layer based on the settings defined here. First is the global counter increment, or how much you'd like to increment the layer order between two objects on that same layer.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public int GlobalCounterIncrement = 5;

		[Header("Parent")]
		[MMInformation("You can also decide to determine the new layer order based on the parent sprite's order (it'll have to be on the same layer).",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public bool BasedOnParentOrder = false;
		public int ParentIncrement = 1;

		[Header("Children")]
		[MMInformation("And here you can decide to apply your new layer order to all children.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public bool ApplyNewOrderToChildren = false;
		public int ChildrenIncrement = 0;

		protected SpriteRenderer _spriteRenderer;
		protected virtual void Start()
		{
			Initialization();
			AutomateLayerOrder();
		}
		protected virtual void Initialization()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
		}
		protected virtual void AutomateLayerOrder()
		{
			int newOrder = 0;
			if (_spriteRenderer == null)
			{
				return;
			}
			if (BasedOnParentOrder)
			{
				int maxLayerOrder = 0;
				Component[] spriteRenderers = GetComponentsInParent( typeof(SpriteRenderer) );
		        if( spriteRenderers != null )
		        {
					foreach( SpriteRenderer spriteRenderer in spriteRenderers )
		            {
						if ( (spriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
							&& (spriteRenderer.sortingOrder > maxLayerOrder))
						{
							maxLayerOrder = spriteRenderer.sortingOrder;							
						}
		            }
		            newOrder = maxLayerOrder + ParentIncrement;                
		        }
			}
			else
			{
				newOrder = CurrentMaxCharacterOrderInLayer + GlobalCounterIncrement;
				CurrentMaxCharacterOrderInLayer += GlobalCounterIncrement;
			}
			_spriteRenderer.sortingOrder = newOrder;
			if (ApplyNewOrderToChildren)
			{
				Component[] childrenSpriteRenderers = GetComponentsInChildren( typeof(SpriteRenderer) );
				if( childrenSpriteRenderers != null )
		        {
					foreach( SpriteRenderer childSpriteRenderer in childrenSpriteRenderers )
		            {
						if (childSpriteRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
						{
							childSpriteRenderer.sortingOrder = newOrder + ChildrenIncrement;
						}
		            }	              
		        }
			}
		}
	}
}
