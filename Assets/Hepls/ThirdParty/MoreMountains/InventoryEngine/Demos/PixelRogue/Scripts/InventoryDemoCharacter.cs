using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	

	[RequireComponent(typeof(Rigidbody2D))]
	public class InventoryDemoCharacter : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[MMInformation("A very basic demo character controller, that makes the character move around on the xy axis. Here you can change its speed and bind sprites and equipment inventories.",MMInformationAttribute.InformationType.Info,false)]
	    public float CharacterSpeed = 10f;
	    public SpriteRenderer WeaponSprite;
		public Inventory ArmorInventory;
		public Inventory WeaponInventory;

	    protected int _currentArmor=0;
	    protected int _currentWeapon=0;
	    protected float _horizontalMove = 0f;
	    protected float _verticalMove = 0f;
	    protected Vector2 _movement;
	    protected Animator _animator;
	    protected Rigidbody2D _rigidBody2D;
	    protected bool _isFacingRight = true;
	    protected virtual void Start()
	    {
	        _animator = GetComponent<Animator>();
	        _rigidBody2D = GetComponent<Rigidbody2D>();
	    }
	    protected virtual void FixedUpdate()
	    {
	        Movement();
	        UpdateAnimator();
	    }
	    public virtual void SetMovement(float movementX, float movementY)
		{
			_horizontalMove = movementX;
	    	_verticalMove = movementY;
		}
		public virtual void SetHorizontalMove(float value)
		{
			_horizontalMove = value;
		}
		public virtual void SetVerticalMove(float value)
		{
			_verticalMove = value;
		}
	    protected virtual void Movement()
	    {
	        if (_horizontalMove > 0.1f)
	        {
	            if (!_isFacingRight)
	                Flip();
	        }
	        else if (_horizontalMove < -0.1f)
	        {
	            if (_isFacingRight)
	                Flip();
	        }
	        _movement = new Vector2(_horizontalMove, _verticalMove);
	        _movement *= CharacterSpeed;
	        _rigidBody2D.velocity = _movement;
	    }
		protected virtual void Flip()
	    {
	        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	        _isFacingRight = transform.localScale.x > 0;
	    }
	    protected virtual void UpdateAnimator()
	    {
	        if (_animator != null)
	        {
	            _animator.SetFloat("Speed", _rigidBody2D.velocity.magnitude);
                _animator.SetInteger("Armor", _currentArmor);
	        }
	    }
	    public virtual void SetArmor(int index)
	    {
	    	_currentArmor = index;
	    }
	    public virtual void SetWeapon(Sprite newSprite, InventoryItem item)
	    {
			WeaponSprite.sprite = newSprite;
	    }
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == "RogueArmorInventory")
				{
					if (ArmorInventory != null)
					{
						if (!InventoryItem.IsNull(ArmorInventory.Content [0]))
						{
							ArmorInventory.Content [0].Equip ();	
						}
					}
				}
				if (inventoryEvent.TargetInventoryName == "RogueWeaponInventory")
				{
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							WeaponInventory.Content [0].Equip ();
						}
					}
				}
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