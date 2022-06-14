using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using System;

namespace MoreMountains.Feedbacks
{
	public class MMMiniPoolableObject : MonoBehaviour 
	{
		public delegate void Events();
		public event Events OnSpawnComplete;
		public float LifeTime = 0f;
		public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}
	    protected virtual void OnEnable()
		{
			if (LifeTime>0)
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
			if(OnSpawnComplete != null)
			{
				OnSpawnComplete();
			}
		}
	}
}
