using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/PathedProjectile")]
    public class PathedProjectile : MonoBehaviour
	{
		[MMInformation("A GameObject with this component will move towards its target and get destroyed when it reaches it. Here you can define what object to instantiate on impact. Use the Initialize method to set its destination and speed.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("The effect to instantiate when the object gets destroyed")]
		public GameObject DestroyEffect;
		[Tooltip("the destination of the projectile")]
		protected Transform _destination;
		[Tooltip("the movement speed")]
		protected float _speed;
		public virtual void Initialize(Transform destination, float speed)
		{
			_destination=destination;
			_speed=speed;
		}
		protected virtual void Update () 
		{
			transform.position=Vector3.MoveTowards(transform.position,_destination.position,Time.deltaTime * _speed);
			var distanceSquared = (_destination.transform.position - transform.position).sqrMagnitude;
			if(distanceSquared > .01f * .01f)
				return;
			
			if (DestroyEffect!=null)
			{
				Instantiate(DestroyEffect,transform.position,transform.rotation); 
			}
			
			Destroy(gameObject);
		}	
	}
}