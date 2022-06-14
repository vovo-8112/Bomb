using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMPoolableObject")]
    public class MMPoolableObject : MMObjectBounds 
	{
		public delegate void Events();
		public event Events OnSpawnComplete;

        [Header("Poolable Object")]
		public float LifeTime = 0f;
		public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}
		protected virtual void Update()
		{

		}
	    protected virtual void OnEnable()
		{
			Size = GetBounds().extents * 2;
			if (LifeTime > 0f)
			{
				Invoke("Destroy", LifeTime);	
			}
		}
	    protected virtual void OnDisable()
		{
			CancelInvoke();
		}
		public virtual void TriggerOnSpawnComplete()
		{
            OnSpawnComplete?.Invoke();
        }
	}
}
