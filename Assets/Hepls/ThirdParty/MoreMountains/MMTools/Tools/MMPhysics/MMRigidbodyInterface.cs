using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Rigidbody Interface/MMRigidbodyInterface")]
	public class MMRigidbodyInterface : MonoBehaviour 
	{
		public Vector3 position
	    {
	        get
	        {
	            if (_rigidbody2D != null)
	            {
	                return _rigidbody2D.position;
	            }
	            if (_rigidbody != null)
	            {
	                return _rigidbody.position;
	            }
	            return Vector3.zero;
	        }
	        set { }
	    }
		public Rigidbody2D InternalRigidBody2D 
		{
			get {
				return _rigidbody2D;
			}
		}
		public Rigidbody InternalRigidBody 
		{
			get {
				return _rigidbody;
			}
		}
		public Vector3 Velocity 
		{
			get 
			{ 
				if (_mode == "2D") 
				{
					return(_rigidbody2D.velocity);
				}
				else 
				{
					if (_mode == "3D") 
					{
						return(_rigidbody.velocity);
					}
					else
					{
						return new Vector3(0,0,0);
					}
				}
			}
			set 
			{
				if (_mode == "2D") {
					_rigidbody2D.velocity = value;
				}
				if (_mode == "3D") {
					_rigidbody.velocity = value;
				}
			}
		}
		public Bounds ColliderBounds 
		{ 
			get 
			{  
				if (_rigidbody2D != null) 
				{
					return _collider2D.bounds;
				}
				if (_rigidbody != null) 
				{
					return _collider.bounds;
				}
				return new Bounds();
			}
		}
		public bool isKinematic
		{
			get
			{
				if (_mode == "2D") 
				{
					return(_rigidbody2D.isKinematic);
				}
				if (_mode == "3D")
				{			
					return(_rigidbody.isKinematic);
				}
				return false;
			}
		}

		protected string _mode;
		protected Rigidbody2D _rigidbody2D;
		protected Rigidbody _rigidbody;
		protected Collider2D _collider2D;
		protected Collider _collider;
		protected Bounds _colliderBounds;
		protected virtual void Awake () 
		{
			_rigidbody2D=GetComponent<Rigidbody2D>();
			_rigidbody=GetComponent<Rigidbody>();

			if (_rigidbody2D != null) 
			{
				_mode="2D";
				_collider2D = GetComponent<Collider2D> ();
			}
			if (_rigidbody != null) 
			{
				_mode="3D";
				_collider = GetComponent<Collider> ();
			}
			if (_rigidbody==null && _rigidbody2D==null)
			{
				Debug.LogWarning("A RigidBodyInterface has been added to "+gameObject+" but there's no Rigidbody or Rigidbody2D on it.", gameObject);
			}
		}
		public virtual void AddForce(Vector3 force)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.AddForce(force,ForceMode2D.Impulse);
			}
			if (_mode == "3D")
			{
				_rigidbody.AddForce(force);
			}
		}
		public virtual void AddRelativeForce(Vector3 force)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.AddRelativeForce(force,ForceMode2D.Impulse);
			}
			if (_mode == "3D")
			{
				_rigidbody.AddRelativeForce(force);
			}
		}
	    public virtual void MovePosition(Vector3 newPosition)
	    {
	        if (_mode == "2D")
	        {
	            _rigidbody2D.MovePosition(newPosition);
	        }
	        if (_mode == "3D")
	        {
	            _rigidbody.MovePosition(newPosition);
	        }
	    }
		public virtual void ResetAngularVelocity()
		{
			if (_mode == "2D")
			{
				_rigidbody2D.angularVelocity = 0;
			}
			if (_mode == "3D")
			{
				_rigidbody.angularVelocity = Vector3.zero;
			}	
		}
		public virtual void ResetRotation()
		{
			if (_mode == "2D")
			{
				_rigidbody2D.rotation = 0;
			}
			if (_mode == "3D")
			{
				_rigidbody.rotation = Quaternion.identity;
			}	
		}
		public virtual void IsKinematic(bool status)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.isKinematic=status;
			}
			if (_mode == "3D")
			{			
				_rigidbody.isKinematic=status;
			}
		}
		public virtual void EnableBoxCollider(bool status)
		{
			if (_mode == "2D") 
			{
				GetComponent<Collider2D>().enabled=status;
			}
			if (_mode == "3D")
			{			
				GetComponent<Collider>().enabled=status;
			}
		}
		public bool Is3D 
		{ 
			get
	        {
				if (_mode=="3D") 
				{ 
					return true; 
				} 
				else 
				{ 
					return false; 
				}
			}
		}
		public bool Is2D 
		{ 
			get
	        {
				if (_mode=="2D") 
				{ 
					return true; 
				} 
				else
				{ 
					return false; 
				}
			}
		}
	}
}
