using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/PathedProjectileSpawner")]
    public class PathedProjectileSpawner : MonoBehaviour 
	{
		[MMInformation("A GameObject with this component will spawn projectiles at the specified fire rate.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("the pathed projectile's destination")]
		public Transform Destination;
		[Tooltip("the projectiles to spawn")]
		public PathedProjectile Projectile;
		[Tooltip("the effect to instantiate at each spawn")]
		public GameObject SpawnEffect;
		[Tooltip("the speed of the projectiles")]
		public float Speed;
		[Tooltip("the frequency of the spawns")]
		public float FireRate;
		
		protected float _nextShotInSeconds;
	    protected virtual void Start () 
		{
			_nextShotInSeconds=FireRate;
		}
	    protected virtual void Update () 
		{
			if((_nextShotInSeconds -= Time.deltaTime)>0)
				return;
				
			_nextShotInSeconds = FireRate;
			var projectile = (PathedProjectile) Instantiate(Projectile, transform.position,transform.rotation);
			projectile.Initialize(Destination,Speed);
			
			if (SpawnEffect!=null)
			{
				Instantiate(SpawnEffect,transform.position,transform.rotation);
			}
		}
		public virtual void OnDrawGizmos()
		{
			if (Destination==null)
				return;
			
			Gizmos.color=Color.gray;
			Gizmos.DrawLine(transform.position,Destination.position);
		}
	}
}