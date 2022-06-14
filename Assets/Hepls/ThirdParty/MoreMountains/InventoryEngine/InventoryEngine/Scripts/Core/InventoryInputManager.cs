using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    public class InventoryInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        [Header("Targets")]
        [MMInformation("Bind here your inventory container (the CanvasGroup that you want to turn on/off when opening/closing the inventory), your main InventoryDisplay, and the overlay that will be displayed under the InventoryDisplay when opened.", MMInformationAttribute.InformationType.Info, false)]
        public CanvasGroup TargetInventoryContainer;
        public InventoryDisplay TargetInventoryDisplay;
        public CanvasGroup Overlay;

        [Header("Start Behaviour")]
        [MMInformation("If you set HideContainerOnStart to true, the TargetInventoryContainer defined right above this field will be automatically hidden on Start, even if you've left it visible in Scene view. Useful for setup.", MMInformationAttribute.InformationType.Info, false)]
        public bool HideContainerOnStart = true;

        [Header("Permissions")]
        [MMInformation("Here you can decide to have your inventory catch input only when open, or not.", MMInformationAttribute.InformationType.Info, false)]
        public bool InputOnlyWhenOpen = true;

        [Header("Key Mapping")]
        [MMInformation("Here you need to set the various key bindings you prefer. There are some by default but feel free to change them.", MMInformationAttribute.InformationType.Info, false)]
        public KeyCode ToggleInventoryKey = KeyCode.I;
        public KeyCode ToggleInventoryAltKey = KeyCode.Joystick1Button6;
        public KeyCode CancelKey = KeyCode.Escape;
        public string MoveKey = "insert";
        public string MoveAltKey = "joystick button 2";
        public string EquipKey = "home";
        public string EquipAltKey = "home";
        public string UseKey = "end";
        public string UseAltKey = "end";
        public string EquipOrUseKey = "space";
        public string EquipOrUseAltKey = "joystick button 0";
        public string DropKey = "delete";
        public string DropAltKey = "joystick button 1";
        public string NextInvKey = "page down";
        public string NextInvAltKey = "joystick button 4";
        public string PrevInvKey = "page up";
        public string PrevInvAltKey = "joystick button 5";

        public enum ManageButtonsModes { Interactable, SetActive }
        
        [Header("Buttons")]
        public bool ManageButtons = false;
        [MMCondition("ManageButtons", true)] 
        public ManageButtonsModes ManageButtonsMode = ManageButtonsModes.SetActive;
        [MMCondition("ManageButtons", true)]
        public Button EquipUseButton;
        [MMCondition("ManageButtons", true)]
        public Button MoveButton;
        [MMCondition("ManageButtons", true)]
        public Button DropButton;
        [MMCondition("ManageButtons", true)]
        public Button EquipButton;
        [MMCondition("ManageButtons", true)]
        public Button UseButton;
        [MMCondition("ManageButtons", true)]
        public Button UnEquipButton;
        public InventorySlot CurrentlySelectedInventorySlot { get; set; }

        [Header("State")]
        [MMReadOnly]
        public bool InventoryIsOpen;

        protected CanvasGroup _canvasGroup;
        protected bool _pause = false;
        protected GameObject _currentSelection;
        protected InventorySlot _currentInventorySlot;
        protected List<InventoryHotbar> _targetInventoryHotbars;
        protected InventoryDisplay _currentInventoryDisplay;
        private bool _isEquipUseButtonNotNull;
        private bool _isEquipButtonNotNull;
        private bool _isUseButtonNotNull;
        private bool _isUnEquipButtonNotNull;
        private bool _isMoveButtonNotNull;
        private bool _isDropButtonNotNull;
        protected virtual void Start()
        {
            _isDropButtonNotNull = DropButton != null;
            _isMoveButtonNotNull = MoveButton != null;
            _isUnEquipButtonNotNull = UnEquipButton != null;
            _isUseButtonNotNull = UseButton != null;
            _isEquipButtonNotNull = EquipButton != null;
            _isEquipUseButtonNotNull = EquipUseButton != null;
            _currentInventoryDisplay = TargetInventoryDisplay;
            InventoryIsOpen = false;
            _targetInventoryHotbars = new List<InventoryHotbar>();
            _canvasGroup = GetComponent<CanvasGroup>();
            foreach (InventoryHotbar go in FindObjectsOfType(typeof(InventoryHotbar)) as InventoryHotbar[])
            {
                _targetInventoryHotbars.Add(go);
            }
            if (HideContainerOnStart)
            {
                if (TargetInventoryContainer != null) { TargetInventoryContainer.alpha = 0; }
                if (Overlay != null) { Overlay.alpha = 0; }
                EventSystem.current.sendNavigationEvents = false;
                if (_canvasGroup != null)
                {
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }
        protected virtual void Update()
        {
            HandleInventoryInput();
            HandleHotbarsInput();
            CheckCurrentlySelectedSlot();
            HandleButtons();
        }
        protected virtual void CheckCurrentlySelectedSlot()
        {
            _currentSelection = EventSystem.current.currentSelectedGameObject;
            if (_currentSelection == null)
            {
                return;
            }
            _currentInventorySlot = _currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>();
            if (_currentInventorySlot != null)
            {
                CurrentlySelectedInventorySlot = _currentInventorySlot;
            }
        }
        protected virtual void HandleButtons()
        {
            if (!ManageButtons)
            {
                return;
            }
            
            if (CurrentlySelectedInventorySlot != null)
            {
                if (_isUseButtonNotNull)
                {
                    SetButtonState(UseButton, CurrentlySelectedInventorySlot.Usable());
                }

                if (_isEquipButtonNotNull)
                {
                    SetButtonState(EquipButton, CurrentlySelectedInventorySlot.Equippable());
                }

                if (_isEquipUseButtonNotNull)
                {
                    SetButtonState(EquipUseButton, CurrentlySelectedInventorySlot.Usable() ||
                                                   CurrentlySelectedInventorySlot.Equippable());
                }

                if (_isUnEquipButtonNotNull)
                {
                    SetButtonState(UnEquipButton, CurrentlySelectedInventorySlot.Unequippable());
                }

                if (_isMoveButtonNotNull)
                {
                    SetButtonState(MoveButton, CurrentlySelectedInventorySlot.Movable());
                }

                if (_isDropButtonNotNull)
                {
                    SetButtonState(DropButton, CurrentlySelectedInventorySlot.Droppable());
                }
            }
            else
            {
                SetButtonState(UseButton, false);
                SetButtonState(EquipButton, false);
                SetButtonState(EquipUseButton, false);
                SetButtonState(DropButton, false);
                SetButtonState(MoveButton, false);
                SetButtonState(UnEquipButton, false);
            }
        }
        protected virtual void SetButtonState(Button targetButton, bool state)
        {
            if (ManageButtonsMode == ManageButtonsModes.Interactable)
            {
                targetButton.interactable = state;
            }
            else
            {
                targetButton.gameObject.SetActive(state);
            }
        }
        public virtual void ToggleInventory()
        {
            if (InventoryIsOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
        public virtual void OpenInventory()
        {
            _pause = true;
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryOpens, null, TargetInventoryDisplay.TargetInventoryName, TargetInventoryDisplay.TargetInventory.Content[0], 0, 0);
            MMGameEvent.Trigger("inventoryOpens");
            InventoryIsOpen = true;

            StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 1f));
            StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, 0.85f));
        }
        public virtual void CloseInventory()
        {
            _pause = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloses, null, TargetInventoryDisplay.TargetInventoryName, null, 0, 0);
            MMGameEvent.Trigger("inventoryCloses");
            InventoryIsOpen = false;

            StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 0f));
            StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, 0f));
        }
        protected virtual void HandleInventoryInput()
        {
            if (_currentInventoryDisplay == null)
            {
                return;
            }
            if (Input.GetKeyDown(ToggleInventoryKey) || Input.GetKeyDown(ToggleInventoryAltKey))
            {
                if (!InventoryIsOpen)
                {
                    OpenInventory();
                }
                else
                {
                    CloseInventory();
                }
            }

            if (Input.GetKeyDown(CancelKey))
            {
                if (InventoryIsOpen)
                {
                    CloseInventory();
                }
            }
            if (InputOnlyWhenOpen && !InventoryIsOpen)
            {
                return;
            }
            if (Input.GetKeyDown(PrevInvKey) || Input.GetKeyDown(PrevInvAltKey))
            {
                if (_currentInventoryDisplay.GoToInventory(-1) != null)
                {
                    _currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(-1);
                }
            }
            if (Input.GetKeyDown(NextInvKey) || Input.GetKeyDown(NextInvAltKey))
            {
                if (_currentInventoryDisplay.GoToInventory(1) != null)
                {
                    _currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(1);
                }
            }
            if (Input.GetKeyDown(MoveKey) || Input.GetKeyDown(MoveAltKey))
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Move();
                }
            }
            if (Input.GetKeyDown(EquipOrUseKey) || Input.GetKeyDown(EquipOrUseAltKey))
            {
                EquipOrUse();
            }
            if (Input.GetKeyDown(EquipKey) || Input.GetKeyDown(EquipAltKey))
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Equip();
                }
            }
            if (Input.GetKeyDown(UseKey) || Input.GetKeyDown(UseAltKey))
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Use();
                }
            }
            if (Input.GetKeyDown(DropKey) || Input.GetKeyDown(DropAltKey))
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Drop();
                }
            }
        }
        protected virtual void HandleHotbarsInput()
        {
            if (!InventoryIsOpen)
            {
                foreach (InventoryHotbar hotbar in _targetInventoryHotbars)
                {
                    if (hotbar != null)
                    {
                        if (Input.GetKeyDown(hotbar.HotbarKey) || Input.GetKeyDown(hotbar.HotbarAltKey))
                        {
                            hotbar.Action();
                        }
                    }
                }
            }
        }
        public virtual void EquipOrUse()
        {
            if (CurrentlySelectedInventorySlot.Equippable())
            {
                CurrentlySelectedInventorySlot.Equip();
            }
            if (CurrentlySelectedInventorySlot.Usable())
            {
                CurrentlySelectedInventorySlot.Use();
            }
        }

        public virtual void Equip()
        {
            CurrentlySelectedInventorySlot.Equip();
        }

        public virtual void Use()
        {
            CurrentlySelectedInventorySlot.Use();
        }

        public virtual void UnEquip()
        {
            CurrentlySelectedInventorySlot.UnEquip();
        }
        public virtual void Move()
        {
            CurrentlySelectedInventorySlot.Move();
        }
        public virtual void Drop()
        {
            CurrentlySelectedInventorySlot.Drop();
        }
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloseRequest)
            {
                CloseInventory();
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