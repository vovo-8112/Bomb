using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
	public class InventoryDetails : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[MMInformation("Specify here the name of the inventory whose content's details you want to display in this Details panel. You can also decide to make it global. If you do so, it'll display the details of all items, regardless of their inventory.",MMInformationAttribute.InformationType.Info,false)]
		public string TargetInventoryName;
		public bool Global = false;
		public bool Hidden { get; protected set; }

		[Header("Default")]
		[MMInformation("By checking HideOnEmptySlot, the Details panel won't be displayed if you select an empty slot.",MMInformationAttribute.InformationType.Info,false)]
		public bool HideOnEmptySlot=true;
		[MMInformation("Here you can set default values for all fields of the details panel. These values will be displayed when no item is selected (and if you've chosen not to hide the panel in that case).",MMInformationAttribute.InformationType.Info,false)]
		public string DefaultTitle;
		public string DefaultShortDescription;
		public string DefaultDescription;
		public string DefaultQuantity;
		public Sprite DefaultIcon;

		[Header("Behaviour")]
		[MMInformation("Here you can decide whether or not to hide the details panel on start.",MMInformationAttribute.InformationType.Info,false)]
		public bool HideOnStart = true;

		[Header("Components")]
		[MMInformation("Here you need to bind the panel components.",MMInformationAttribute.InformationType.Info,false)]
		public Image Icon;
		public Text Title;
		public Text ShortDescription;
		public Text Description;
		public Text Quantity;

		protected float _fadeDelay=0.2f;
		protected CanvasGroup _canvasGroup;
		protected virtual void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();

			if (HideOnStart)
			{
				_canvasGroup.alpha = 0;
			}

			if (_canvasGroup.alpha == 0)
			{
				Hidden = true;
			}
			else
			{
				Hidden = false;
			}
		}
		public virtual void DisplayDetails(InventoryItem item)
		{
			if (InventoryItem.IsNull(item))
			{
				if (HideOnEmptySlot && !Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
					Hidden=true;
				}
				if (!HideOnEmptySlot)
				{
					StartCoroutine(FillDetailFieldsWithDefaults(0));
				}
			}
			else
			{
				StartCoroutine(FillDetailFields(item,0f));

				if (HideOnEmptySlot && Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,1f));
					Hidden=false;
				}
			}
		}
		protected virtual IEnumerator FillDetailFields(InventoryItem item, float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = item.ItemName ; }
			if (ShortDescription!=null) { ShortDescription.text = item.ShortDescription;}
			if (Description!=null) { Description.text = item.Description;}
			if (Quantity!=null) { Quantity.text = item.Quantity.ToString();}
			if (Icon!=null) { Icon.sprite = item.Icon;}
			
			if (HideOnEmptySlot && !Hidden && (item.Quantity == 0))
			{
				StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
				Hidden=true;
			}
		}
		protected virtual IEnumerator FillDetailFieldsWithDefaults(float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = DefaultTitle ;}
			if (ShortDescription!=null) { ShortDescription.text = DefaultShortDescription;}
			if (Description!=null) { Description.text = DefaultDescription;}
			if (Quantity!=null) { Quantity.text = DefaultQuantity;}
			if (Icon!=null) { Icon.sprite = DefaultIcon;}
		}
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (!Global && (inventoryEvent.TargetInventoryName != this.TargetInventoryName))
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.UseRequest:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.InventoryOpens:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.Drop:
					DisplayDetails (null);
					break;
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