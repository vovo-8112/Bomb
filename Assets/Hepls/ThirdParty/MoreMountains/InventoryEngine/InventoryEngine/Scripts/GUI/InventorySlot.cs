using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{
	public class InventorySlot : Button 
	{
		[MMInformation("Inventory slots are used inside an InventoryDisplay to present the content of each inventory slot. It's best to not touch these directly but rather make changes from the InventoryDisplay's inspector.",MMInformationAttribute.InformationType.Info,false)]
		public Sprite MovedSprite;
		public InventoryDisplay ParentInventoryDisplay;
		public int Index;
		public bool SlotEnabled=true;

		protected const float _disabledAlpha = 0.5f;
		protected const float _enabledAlpha = 1.0f;
		protected override void Start()
		{
			base.Start();
			this.onClick.AddListener(SlotClicked);
		}
		public virtual void DrawIcon(InventoryItem item, int index)
		{
			if (ParentInventoryDisplay!=null)
			{				
				if (!InventoryItem.IsNull(item))
				{
					GameObject itemIcon = new GameObject("Icon", typeof(RectTransform));
					itemIcon.transform.SetParent(this.transform);
					UnityEngine.UI.Image itemIconImage = itemIcon.AddComponent<Image>();
					itemIconImage.sprite = item.Icon;
					RectTransform itemRectTransform = itemIcon.GetComponent<RectTransform>();
					itemRectTransform.localPosition=Vector3.zero;
					itemRectTransform.localScale=Vector3.one;
					MMGUI.SetSize(itemRectTransform, ParentInventoryDisplay.IconSize);
					if (item.Quantity>1)
					{
						GameObject textObject = new GameObject("Slot "+index+" Quantity", typeof(RectTransform));
						textObject.transform.SetParent(this.transform);
						Text textComponent = textObject.AddComponent<Text>();
						textComponent.text=item.Quantity.ToString();
						textComponent.font=ParentInventoryDisplay.QtyFont;
						textComponent.fontSize=ParentInventoryDisplay.QtyFontSize;
						textComponent.color=ParentInventoryDisplay.QtyColor;
						textComponent.alignment=ParentInventoryDisplay.QtyAlignment;
						RectTransform textObjectRectTransform = textObject.GetComponent<RectTransform>();
						textObjectRectTransform.localPosition=Vector3.zero;
						textObjectRectTransform.localScale=Vector3.one;
						MMGUI.SetSize(textObjectRectTransform, (ParentInventoryDisplay.SlotSize - Vector2.one * ParentInventoryDisplay.QtyPadding)); 
					}
				}
			}
		}
		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (ParentInventoryDisplay!=null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				MMInventoryEvent.Trigger(MMInventoryEventType.Select, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index);
			}
		}
		public virtual void SlotClicked () 
		{
			if (ParentInventoryDisplay!=null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				MMInventoryEvent.Trigger(MMInventoryEventType.Click, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index);
				if (ParentInventoryDisplay.CurrentlyBeingMovedItemIndex!=-1)
				{
					Move();
				}
			}
		}
		public virtual void Move()
		{
			if (!SlotEnabled) { return; }
			if (ParentInventoryDisplay.CurrentlyBeingMovedItemIndex == -1)
			{
				if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index);
					return;
				}
				if (ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
				{
					GetComponent<Image>().sprite = ParentInventoryDisplay.MovedSlotImage;
					ParentInventoryDisplay.CurrentlyBeingMovedItemIndex=Index;
				}
			}
			else
			{
				if (!ParentInventoryDisplay.TargetInventory.MoveItem(ParentInventoryDisplay.CurrentlyBeingMovedItemIndex,Index))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index);
				}
				else
				{
					ParentInventoryDisplay.CurrentlyBeingMovedItemIndex=-1;
					MMInventoryEvent.Trigger(MMInventoryEventType.Move, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
				}
			}
		}
		public virtual void Use()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UseRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
		}
		public virtual void Equip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
		}
		public virtual void UnEquip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
		}
		public virtual void Drop()
		{
			if (!SlotEnabled) { return; }
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index);
				return;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
			{
				return;
			}
            if (ParentInventoryDisplay.TargetInventory.Content[Index].Drop())
            {
                ParentInventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
                MMInventoryEvent.Trigger(MMInventoryEventType.Drop, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index);
            }            
		}
		public virtual void DisableSlot()
		{
			this.interactable=false;
			SlotEnabled=false;
			GetComponent<CanvasGroup> ().alpha = _disabledAlpha;
		}
		public virtual void EnableSlot()
		{
			this.interactable=true;
			SlotEnabled=true;
			GetComponent<CanvasGroup> ().alpha = _enabledAlpha;
		}
		public virtual bool Equippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsEquippable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		public virtual bool Usable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsUsable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		public virtual bool Movable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		public virtual bool Droppable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		public virtual bool Unequippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (ParentInventoryDisplay.TargetInventory.InventoryType != Inventory.InventoryTypes.Equipment)
			{
				return false;
			}
			else
			{
				return true;
			}
		}	
	}
}