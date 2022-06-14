using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Particles/MMAutoDestroyParticleSystem")]
    public class MMAutoDestroyParticleSystem : MonoBehaviour 
	{
		public bool DestroyParent=false;
		public float DestroyDelay=0f;
		
		protected ParticleSystem _particleSystem;
		protected float _startTime;
		protected virtual void Start()
		{
			_particleSystem = GetComponent<ParticleSystem>();
			if (DestroyDelay!=0)
			{
				_startTime = Time.time;
			}
		}
		protected virtual void Update()
		{	
			if ( (DestroyDelay!=0) && (Time.time - _startTime > DestroyDelay) )
			{
				DestroyParticleSystem();
			}	

			if (_particleSystem.isPlaying)
			{
				return;
			}

			DestroyParticleSystem();
		}
		protected virtual void DestroyParticleSystem()
		{
			if (transform.parent!=null)
			{
				if(DestroyParent)
				{	
					Destroy(transform.parent.gameObject);	
				}
			}					
			Destroy (gameObject);
		}
	}
}
