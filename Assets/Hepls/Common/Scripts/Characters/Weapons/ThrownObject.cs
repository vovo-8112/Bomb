using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[RequireComponent(typeof(Rigidbody2D))]
	[AddComponentMenu("TopDown Engine/Weapons/ThrownObject")]
	public class ThrownObject : Projectile 
	{
		protected Vector2 _throwingForce;
		protected bool _forceApplied = false;
		protected override void Initialization()
		{
			base.Initialization();
			_rigidBody2D = this.GetComponent<Rigidbody2D>();
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			_forceApplied = false;
		}
		public override void Movement()
		{
			if (!_forceApplied && (Direction != Vector3.zero))
			{
				_throwingForce = Direction * Speed;
				_rigidBody2D.AddForce (_throwingForce);
				_forceApplied = true;
			}
		}
	}
}
