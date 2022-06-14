using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
    [AddComponentMenu("More Mountains/Tools/Particles/MMDelayParticles")]
    public class MMDelayParticles : MonoBehaviour 
	{
		[Header("Delay")]
		public float Delay;
		public bool DelayChildren = true;
		public bool ApplyDelayOnStart = false;

		[MMInspectorButtonAttribute("ApplyDelay")]
		public bool ApplyDelayButton;

		protected Component[] particleSystems;

		protected virtual void Start()
		{
			if (ApplyDelayOnStart)
			{
				ApplyDelay();
			}
		}

		protected virtual void ApplyDelay()
		{
			if (this.gameObject.GetComponent<ParticleSystem>() != null)
			{
                ParticleSystem.MainModule main = this.gameObject.GetComponent<ParticleSystem>().main;
				main.startDelay = main.startDelay.constant + Delay;
			}

			particleSystems = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem system in particleSystems)
			{
                ParticleSystem.MainModule main = system.main;
				main.startDelay = main.startDelay.constant + Delay;
			}

		}		
	}
}
