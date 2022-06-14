using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace MoreMountains.InventoryEngine
{	
	[SelectionBase]
	public class InventoryDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Binding")]
		[MMInformation("An InventoryDisplay is a component that will handle the visualization of the data contained in an Inventory. Start by specifying the name of the inventory you want to display.",MMInformationAttribute.InformationType.Info,false)]
		public string TargetInventoryName = "MainInventory";

		protected Inventory _targetInventory = null;
		public Inventory TargetInventory 
		{ 
			get 
			{ 
				if (TargetInventoryName==null)
				{
					return null;
				}
				if (_targetInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if (inventory.name == TargetInventoryName)
						{
							_targetInventory = inventory;
						}
					}	
				}
				return _targetInventory;
			}
		}

		[Header("Inventory Size")]
		[MMInformation("An InventoryDisplay presents an inventory's data in slots containing one item each, and displayed in a grid. Here you can set how many rows and columns of slots you want. Once you're happy with your settings, you can press the 'auto setup' button at the bottom of this inspector to see your changes.",MMInformationAttribute.InformationType.Info,false)]
		public int NumberOfRows = 3;
		public int NumberOfColumns = 2;
		public int InventorySize { get { return NumberOfRows * NumberOfColumns; } set {} }		

		[Header("Equipment")]
		[MMInformation("If this displays the contents of an Equipment Inventory, you should bind here a Choice Inventory. A Choice Inventory is the inventory in which you'll pick items for your equipment. Usually the Choice Inventory is the Main Inventory. Again, if this is an equipment inventory, you can specify what class of items you want to authorize.",MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay TargetChoiceInventory;
		public ItemClasses ItemClass;

		[Header("Behaviour")]
		[MMInformation("If you set this to true, empty slots will be drawn, otherwise they'll be hidden from the player.",MMInformationAttribute.InformationType.Info,false)]
		public bool DrawEmptySlots=true;

		[Header("Inventory Padding")]
		[MMInformation("Here you can define the padding between the borders of the inventory panel and the slots.",MMInformationAttribute.InformationType.Info,false)]
		public int PaddingTop=20;
		public int PaddingRight=20;
		public int PaddingBottom=20;
		public int PaddingLeft=20;

		[Header("Slots")]
		[MMInformation("When pressing the auto setup button at the bottom of this inventory, the InventoryDisplay will fill itself with slots ready to display your inventory's contents. Here you can define the slot's size, margins, and define the images to use when the slot is empty, filled, etc.",MMInformationAttribute.InformationType.Info,false)]
		public Vector2 SlotSize = new Vector2(50,50);
		public Vector2 IconSize = new Vector2(30,30);
		public Vector2 SlotMargin = new Vector2(5,5);
		public Sprite EmptySlotImage;
		public Sprite FilledSlotImage;
		public Sprite HighlightedSlotImage;
		public Sprite PressedSlotImage;
		public Sprite DisabledSlotImage;
		public Sprite MovedSlotImage;
		public Image.Type SlotImageType;

		[Header("Navigation")]
		[MMInformation("Here you can decide whether or not you want to use the built-in navigation system (allowing the player to move from slot to slot using keyboard arrows or a joystick), and whether or not this inventory display panel should be focused whent the scene starts. Usually you'll want your main inventory to get focus.",MMInformationAttribute.InformationType.Info,false)]
		public bool EnableNavigation = true;
		public bool GetFocusOnStart = false;

		[Header("Title Text")]
		[MMInformation("Here you can decide to display (or not) a title next to your inventory display panel. For it you can specify the title, font, font size, color etc.",MMInformationAttribute.InformationType.Info,false)]
		public bool DisplayTitle=true;
		public string Title;
		public Font TitleFont;
		public int TitleFontSize=20;
		public Color TitleColor = Color.black;
		public Vector3 TitleOffset=Vector3.zero;
		public TextAnchor TitleAlignment = TextAnchor.LowerRight;

		[Header("Quantity Text")]
		[MMInformation("If your inventory contains stacked items (more than one item of a certain sort in a single slot, like coins or potions maybe) you'll probably want to display the quantity next to the item's icon. For that, you can specify here the font to use, the color, and position of that quantity text.",MMInformationAttribute.InformationType.Info,false)]
		public Font QtyFont;
		public int QtyFontSize=12;
		public Color QtyColor = Color.black;
		public float QtyPadding=10f;
		public TextAnchor QtyAlignment = TextAnchor.LowerRight;

		[Header("Extra Inventory Navigation")]
		[MMInformation("The InventoryInputManager comes with controls allowing you to go from one inventory panel to the next. Here you can define what inventory the player should go to from this panel when pressing the previous or next inventory button.",MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay PreviousInventory;
		public InventoryDisplay NextInventory;
		public GridLayoutGroup InventoryGrid { get; protected set; }
		public InventoryDisplayTitle InventoryTitle { get; protected set; }
		public RectTransform InventoryRectTransform { get { return GetComponent<RectTransform>(); }}
		public List<GameObject> SlotContainer { get; protected set; }
		public InventoryDisplay ReturnInventory { get; protected set; }
		[MMHidden]
		public int CurrentlyBeingMovedItemIndex=-1;

		protected bool _inventoryWindowIsOpen;
		protected List<InventoryItem> _contentLastUpdate;		
		protected List<int> _changes;		
		protected List<int> _comparison;	
		protected SpriteState _spriteState = new SpriteState();
		protected InventorySlot _currentlySelectedSlot;

		protected GameObject _slotPrefab = null;
		public virtual void SetupInventoryDisplay()
		{
			if (TargetInventoryName == "")
			{
				Debug.LogError("The " + this.name + " Inventory Display doesn't have a TargetInventoryName set. You need to set one from its inspector, matching an Inventory's name.");
				return;
			}

			if (TargetInventory == null)
			{
				Debug.LogError("The " + this.name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventoryName + "), or set that TargetInventoryName to one that exists.");
				return;
			}
			if (this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer> ().SetupInventorySoundPlayer ();
			}

			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryTitle();
			ResizeInventoryDisplay ();
			DrawInventoryContent();
		}
		protected virtual void Awake()
		{
			_contentLastUpdate = new List<InventoryItem>();		
			SlotContainer = new List<GameObject>() ;		
			_comparison = new List<int>();
			if (!TargetInventory.Persistent)
			{
				RedrawInventoryDisplay (); 	
			}
		}
		protected virtual void RedrawInventoryDisplay()
		{
			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryContent();		
			FillLastUpdateContent();	
		}
		protected virtual void InitializeSprites()
		{
			_spriteState.disabledSprite= DisabledSlotImage;
			_spriteState.highlightedSprite= HighlightedSlotImage;
			_spriteState.pressedSprite= PressedSlotImage;
		}
		protected virtual void DrawInventoryTitle()
		{
			if (!DisplayTitle)
			{
				return;
			}
			if (GetComponentInChildren<InventoryDisplayTitle>()!=null)
			{
				if (!Application.isPlaying)
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						DestroyImmediate(title.gameObject);
					}
				}
				else
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						Destroy(title.gameObject);
					}
				}
			}
			GameObject inventoryTitle = new GameObject();
			InventoryTitle = inventoryTitle.AddComponent<InventoryDisplayTitle>();
			inventoryTitle.name="InventoryTitle";
			inventoryTitle.GetComponent<RectTransform>().SetParent(this.transform);
			inventoryTitle.GetComponent<RectTransform>().sizeDelta=GetComponent<RectTransform>().sizeDelta;
			inventoryTitle.GetComponent<RectTransform>().localPosition=TitleOffset;
			inventoryTitle.GetComponent<RectTransform>().localScale=Vector3.one;
			InventoryTitle.text=Title;
			InventoryTitle.color=TitleColor;
			InventoryTitle.font=TitleFont;
			InventoryTitle.fontSize=TitleFontSize;
			InventoryTitle.alignment=TitleAlignment;
			InventoryTitle.raycastTarget=false;
		}
		protected virtual void AddGridLayoutGroup()
		{
			if (GetComponentInChildren<InventoryDisplayGrid>() == null)
			{
				GameObject inventoryGrid=new GameObject("InventoryDisplayGrid");
				inventoryGrid.transform.parent=this.transform;
				inventoryGrid.transform.position=transform.position;
				inventoryGrid.transform.localScale=Vector3.one;
				inventoryGrid.AddComponent<InventoryDisplayGrid>();
				InventoryGrid = inventoryGrid.AddComponent<GridLayoutGroup>();
			}
			if (InventoryGrid == null)
			{
				InventoryGrid = GetComponentInChildren<GridLayoutGroup>();
			}
			InventoryGrid.padding.top = PaddingTop;
			InventoryGrid.padding.right = PaddingRight;
			InventoryGrid.padding.bottom = PaddingBottom;
			InventoryGrid.padding.left = PaddingLeft;
			InventoryGrid.cellSize = SlotSize;
			InventoryGrid.spacing = SlotMargin;
		}
		protected virtual void ResizeInventoryDisplay()
		{

			float newWidth = PaddingLeft + SlotSize.x * NumberOfColumns + SlotMargin.x * (NumberOfColumns-1) + PaddingRight;
			float newHeight = PaddingTop + SlotSize.y * NumberOfRows + SlotMargin.y * (NumberOfRows-1) + PaddingBottom;

			TargetInventory.ResizeArray(NumberOfRows * NumberOfColumns);	

			Vector2 newSize= new Vector2(newWidth,newHeight);
			InventoryRectTransform.sizeDelta = newSize;
			InventoryGrid.GetComponent<RectTransform>().sizeDelta = newSize;
		}
		protected virtual void DrawInventoryContent ()             
		{            
			if (SlotContainer!=null)
			{
				SlotContainer.Clear();
			}
			else
			{
				SlotContainer=new List<GameObject>();
			}
			if (EmptySlotImage==null)
			{
				InitializeSprites();
			}
			foreach (InventorySlot slot in transform.GetComponentsInChildren<InventorySlot>())
			{	 			
				if (!Application.isPlaying)
				{
					DestroyImmediate (slot.gameObject);
				}
				else
				{
					Destroy(slot.gameObject);
				}				
			}
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{    
				DrawSlot(i);
			}

			if (Application.isPlaying)
			{
				Destroy (_slotPrefab);	
			}
			else
			{
				DestroyImmediate (_slotPrefab);	
			}

			if (EnableNavigation)
			{
				SetupSlotNavigation();
			}
		}
		protected virtual void ContentHasChanged()
		{
			if (!(Application.isPlaying))
			{
				AddGridLayoutGroup();
				DrawInventoryContent();
				#if UNITY_EDITOR
					EditorUtility.SetDirty(gameObject);
				#endif
			}
			else
			{
				if (!DrawEmptySlots)
				{
					DrawInventoryContent();
				}
				UpdateInventoryContent();
			}
		}
		protected virtual void FillLastUpdateContent()		
		{		
			_contentLastUpdate.Clear();		
			_comparison.Clear();
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 		
			{  		
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					_contentLastUpdate.Add(TargetInventory.Content[i].Copy());	
				}
				else
				{
					_contentLastUpdate.Add(null);	
				}	
			}	
		}
		protected virtual void UpdateInventoryContent ()             
		{      
			if (_contentLastUpdate == null || _contentLastUpdate.Count == 0)
			{
				FillLastUpdateContent();
			}
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{
				if ((TargetInventory.Content[i] == null) && (_contentLastUpdate[i] != null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i] == null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i] != null))
				{
					if ((TargetInventory.Content[i].ItemID != _contentLastUpdate[i].ItemID) || (TargetInventory.Content[i].Quantity != _contentLastUpdate[i].Quantity))
					{
						_comparison.Add(i);
					}
				}
			}
			if (_comparison.Count>0)
			{
				foreach (int comparison in _comparison)
				{
					UpdateSlot(comparison);
				}
			} 	    
			FillLastUpdateContent();
		}
		protected virtual void UpdateSlot(int i)
		{
			if (SlotContainer.Count < i)
			{
				Debug.LogWarning ("It looks like your inventory display wasn't properly initialized. If you're not triggering any Load events, you may want to mark your inventory as non persistent in its inspector. Otherwise, you may want to reset and empty saved inventories and try again.");
			}

			if (SlotContainer.Count <= i)
			{
				return;
			}
			
			if (SlotContainer[i] == null)
			{
				return;
			}
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				SlotContainer[i].GetComponent<Image>().sprite = FilledSlotImage;   
			}
			else
			{
				SlotContainer[i].GetComponent<Image>().sprite = EmptySlotImage;    	
			}
			foreach(Transform child in SlotContainer[i].transform)
			{
				GameObject.Destroy(child.gameObject);
			}
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				SlotContainer[i].GetComponent<InventorySlot>().DrawIcon(TargetInventory.Content[i],i);
			}			   
		}
		protected virtual void InitializeSlotPrefab()
		{
			_slotPrefab = new GameObject();
			_slotPrefab.AddComponent<RectTransform>();

			_slotPrefab.AddComponent<Image> ();
			_slotPrefab.MMGetComponentNoAlloc<Image> ().raycastTarget = true;

			_slotPrefab.AddComponent<InventorySlot> ();
			_slotPrefab.MMGetComponentNoAlloc<InventorySlot> ().transition = Selectable.Transition.SpriteSwap;

			Navigation explicitNavigation = new Navigation ();
			explicitNavigation.mode = Navigation.Mode.Explicit;
			_slotPrefab.GetComponent<InventorySlot> ().navigation = explicitNavigation;

			_slotPrefab.MMGetComponentNoAlloc<InventorySlot> ().interactable = true;

			_slotPrefab.AddComponent<CanvasGroup> ();
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().alpha = 1;
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().interactable = true;
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().blocksRaycasts = true;
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().ignoreParentGroups = false;

			_slotPrefab.name = "SlotPrefab";
		}
		protected virtual void DrawSlot(int i)
		{
			if (!DrawEmptySlots)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					return;
				}
			}

			if (_slotPrefab == null)
			{
				InitializeSlotPrefab ();
			}

			GameObject theSlot = (GameObject)Instantiate(_slotPrefab);

			theSlot.transform.SetParent(InventoryGrid.transform);
			theSlot.GetComponent<RectTransform>().localScale=Vector3.one;
			theSlot.transform.position = transform.position;
			theSlot.name="Slot "+i;
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				theSlot.GetComponent<Image>().sprite = FilledSlotImage;   
			}
			else
			{
				theSlot.GetComponent<Image>().sprite = EmptySlotImage;      	
			}
			theSlot.GetComponent<Image>().type = SlotImageType;
			theSlot.GetComponent<InventorySlot>().spriteState=_spriteState;
			theSlot.GetComponent<InventorySlot>().MovedSprite=MovedSlotImage;
			theSlot.GetComponent<InventorySlot>().ParentInventoryDisplay = this;
			theSlot.GetComponent<InventorySlot>().Index=i;

			SlotContainer.Add(theSlot);	

			theSlot.SetActive(true)	;

			theSlot.GetComponent<InventorySlot>().DrawIcon(TargetInventory.Content[i],i);
		}
		protected virtual void SetupSlotNavigation()
		{
			if (!EnableNavigation)
			{
				return;
			}

			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (SlotContainer[i]==null)
				{
					return;
				}
				Navigation navigation = SlotContainer[i].GetComponent<InventorySlot>().navigation;
				if (i-NumberOfColumns >= 0) 
				{
					navigation.selectOnUp = SlotContainer[i-NumberOfColumns].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnUp=null;
				}
				if (i+NumberOfColumns < SlotContainer.Count) 
				{
					navigation.selectOnDown = SlotContainer[i+NumberOfColumns].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnDown=null;
				}
				if ((i%NumberOfColumns != 0) && (i>0))
				{
					navigation.selectOnLeft = SlotContainer[i-1].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnLeft=null;
				}
				if (((i+1)%NumberOfColumns != 0)  && (i<SlotContainer.Count - 1))
				{
					navigation.selectOnRight = SlotContainer[i+1].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnRight=null;
				}
				SlotContainer[i].GetComponent<InventorySlot>().navigation = navigation;
			}
		}
		public virtual void Focus()		
		{
			if (!EnableNavigation)
			{
				return;
			}
			if (transform.GetComponentInChildren<InventorySlot> () != null) 		
			{		
				transform.GetComponentInChildren<InventorySlot> ().Select ();	

				if (EventSystem.current.currentSelectedGameObject == null) 		
				{	
					EventSystem.current.SetSelectedGameObject (transform.GetComponentInChildren<InventorySlot> ().gameObject);		
				}		
			}					
		}
		public virtual InventorySlot CurrentlySelectedInventorySlot()
		{
			return _currentlySelectedSlot;
		}
		public virtual void SetCurrentlySelectedSlot(InventorySlot slot)
		{
			_currentlySelectedSlot = slot;
		}
		public virtual InventoryDisplay GoToInventory(int direction)
		{
			if (direction==-1)
			{
				if (PreviousInventory==null)
				{
					return null;
				}
				PreviousInventory.Focus();
				return PreviousInventory;
			}
			else
			{
				if (NextInventory==null)
				{
					return null;
				}
				NextInventory.Focus();	
				return NextInventory;			
			}
		}
		public virtual void SetReturnInventory(InventoryDisplay inventoryDisplay)
		{
			ReturnInventory = inventoryDisplay;
		}
		public virtual void ReturnInventoryFocus()
		{
			if (ReturnInventory==null)
			{
				return;
			}
			else
			{
				ResetDisabledStates();
				ReturnInventory.Focus();
				ReturnInventory=null;
			}
		}
		public virtual void DisableAllBut(ItemClasses itemClass)
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					continue;
				}
				if (TargetInventory.Content[i].ItemClass!=itemClass)
				{
					SlotContainer[i].GetComponent<InventorySlot>().DisableSlot();
				}
			}
		}
		public virtual void ResetDisabledStates()
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				SlotContainer[i].GetComponent<InventorySlot>().EnableSlot();
			}
		}
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.TargetInventoryName != this.TargetInventoryName)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Click:
					ReturnInventoryFocus ();
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Move:
					this.ReturnInventoryFocus();
                    UpdateSlot(inventoryEvent.Index);

                    break;

				case MMInventoryEventType.ItemUsed:
					this.ReturnInventoryFocus();
					break;
				
				case MMInventoryEventType.EquipRequest:
					if (this.TargetInventory.InventoryType == Inventory.InventoryTypes.Equipment)
					{
						if (TargetChoiceInventory == null)
						{
							Debug.LogWarning ("InventoryEngine Warning : " + this + " has no choice inventory associated to it.");
							return;
						}
						TargetChoiceInventory.DisableAllBut (this.ItemClass);
						TargetChoiceInventory.Focus ();
						TargetChoiceInventory.SetReturnInventory (this);
					}
					break;
				
				case MMInventoryEventType.ItemEquipped:
					ReturnInventoryFocus();
					break;

				case MMInventoryEventType.Drop:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.ItemUnEquipped:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.InventoryOpens:
					Focus();
					CurrentlyBeingMovedItemIndex = -1;
					_inventoryWindowIsOpen=true;
					EventSystem.current.sendNavigationEvents=true;
					break;

				case MMInventoryEventType.InventoryCloses:
					CurrentlyBeingMovedItemIndex = -1;
					_inventoryWindowIsOpen = false;
					EventSystem.current.sendNavigationEvents=false;
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
				break;

				case MMInventoryEventType.ContentChanged:
					ContentHasChanged ();
					break;

				case MMInventoryEventType.Redraw:
					RedrawInventoryDisplay ();
					break;

				case MMInventoryEventType.InventoryLoaded:
					RedrawInventoryDisplay ();
					if (GetFocusOnStart)
					{
						Focus();
					}
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
