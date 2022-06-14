using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	public struct PickableItemEvent
	{
		public GameObject Picker;
		public PickableItem PickedItem;
        public PickableItemEvent(PickableItem pickedItem, GameObject picker) 
		{
            Picker = picker;
			PickedItem = pickedItem;
        }
        static PickableItemEvent e;
        public static void Trigger(PickableItem pickedItem, GameObject picker)
        {
            e.Picker = picker;
            e.PickedItem = pickedItem;
            MMEventManager.TriggerEvent(e);
        }
    }
	public class PickableItem : MonoBehaviour
	{
        [Header("Pickable Item")]
        [Tooltip("a feedback to play when the object gets picked")]
        public MMFeedbacks PickedMMFeedbacks;
        [Tooltip("if this is true, the picker's collider will be disabled on pick")]
        public bool DisableColliderOnPick = false;
		[Tooltip("if this is set to true, the object will be disabled when picked")]
        public bool DisableObjectOnPick = true;
        [MMCondition("DisableObjectOnPick", true)]
        [Tooltip("the duration (in seconds) after which to disable the object, instant if 0")]
        public float DisableDelay = 0f;
        [Tooltip("if this is set to true, the object will be disabled when picked")]
        public bool DisableModelOnPick = false;
        [MMCondition("DisableModelOnPick", true)]
        [Tooltip("the visual representation of this picker")]
        public GameObject Model;

        protected Collider _collider;
        protected Collider2D _collider2D;
        protected GameObject _collidingObject;
		protected Character _character = null;
		protected bool _pickable = false;
		protected ItemPicker _itemPicker = null;
        protected WaitForSeconds _disableDelay;

		protected virtual void Start()
		{
            _disableDelay = new WaitForSeconds(DisableDelay);
            _collider = gameObject.GetComponent<Collider>();
            _collider2D = gameObject.GetComponent<Collider2D>();
			_itemPicker = gameObject.GetComponent<ItemPicker> ();
            PickedMMFeedbacks?.Initialization(this.gameObject);
        }
		public virtual void OnTriggerEnter (Collider collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}
		public virtual void PickItem(GameObject picker)
		{
			if (CheckIfPickable ())
			{
				Effects ();
				PickableItemEvent.Trigger(this, picker);
				Pick (picker);
                if (DisableColliderOnPick)
                {
                    if (_collider != null)
                    {
                        _collider.enabled = false;
                    }
                    if (_collider2D != null)
                    {
                        _collider2D.enabled = false;
                    }
                }
                if (DisableModelOnPick && (Model != null))
                {
                    Model.gameObject.SetActive(false);
                }

				if (DisableObjectOnPick)
				{
                    if (DisableDelay == 0f)
                    {
                        this.gameObject.SetActive(false);
                    }
					else
                    {
                        StartCoroutine(DisablePickerCoroutine());
                    }
				}
			} 
		}

        protected virtual IEnumerator DisablePickerCoroutine()
        {
            yield return _disableDelay;
            this.gameObject.SetActive(false);
        }
        protected virtual bool CheckIfPickable()
		{
			_character = _collidingObject.GetComponent<Character>();
			if (_character == null)
			{
				return false;
			}
			if (_character.CharacterType != Character.CharacterTypes.Player)
			{
				return false;
			}
			if (_itemPicker != null)
			{
				if  (!_itemPicker.Pickable())
				{
					return false;	
				}
			}

			return true;
		}
		protected virtual void Effects()
		{
            PickedMMFeedbacks?.PlayFeedbacks();
		}
		protected virtual void Pick(GameObject picker)
		{
			
		}
	}
}