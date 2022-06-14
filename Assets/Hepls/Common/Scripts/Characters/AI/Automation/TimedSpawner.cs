using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/TimedSpawner")]
    public class TimedSpawner : MonoBehaviour 
	{
		[Tooltip("whether or not this spawner can spawn")]
		public bool CanSpawn = true;
		[Tooltip("the minimum frequency possible, in seconds")]
		public float MinFrequency = 1f;
		[Tooltip("the maximum frequency possible, in seconds")]
		public float MaxFrequency = 1f;
		public MMObjectPooler ObjectPooler { get; set; }

        [MMInspectorButton("ToggleSpawn")]
        public bool CanSpawnButton;

		protected float _lastSpawnTimestamp = 0f;
		protected float _nextFrequency = 0f;
		protected virtual void Start()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			if (GetComponent<MMMultipleObjectPooler>() != null)
			{
				ObjectPooler = GetComponent<MMMultipleObjectPooler>();
			}
			if (GetComponent<MMSimpleObjectPooler>() != null)
			{
				ObjectPooler = GetComponent<MMSimpleObjectPooler>();
			}
			if (ObjectPooler == null)
			{
				Debug.LogWarning(this.name+" : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
				return;
			}
			DetermineNextFrequency ();
		}
		protected virtual void Update()
		{
			if ((Time.time - _lastSpawnTimestamp > _nextFrequency)  && CanSpawn)
            {
				Spawn ();
			}
		}
		protected virtual void Spawn()
		{
			GameObject nextGameObject = ObjectPooler.GetPooledGameObject();
			if (nextGameObject==null) { return; }
			if (nextGameObject.GetComponent<MMPoolableObject>()==null)
			{
				throw new Exception(gameObject.name+" is trying to spawn objects that don't have a PoolableObject component.");		
			}
			nextGameObject.gameObject.SetActive(true);
			nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();
			Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health> ();
			if (objectHealth != null) 
			{
				objectHealth.Revive ();
            }
            nextGameObject.transform.position = this.transform.position;
            _lastSpawnTimestamp = Time.time;
			DetermineNextFrequency ();
		}
		protected virtual void DetermineNextFrequency()
		{
			_nextFrequency = UnityEngine.Random.Range (MinFrequency, MaxFrequency);
		}
        public virtual void ToggleSpawn()
        {
            CanSpawn = !CanSpawn;
        }
        public virtual void TurnSpawnOff()
        {
            CanSpawn = false;
        }
        public virtual void TurnSpawnOn()
        {
            CanSpawn = true;
        }
	}
}