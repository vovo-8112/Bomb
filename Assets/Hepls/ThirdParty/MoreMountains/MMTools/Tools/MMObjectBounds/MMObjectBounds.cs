﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Object Bounds/MMObjectBounds")]
    public class MMObjectBounds : MonoBehaviour
	{
		public enum WaysToDetermineBounds { Collider, Collider2D, Renderer, Undefined }

        [Header("Bounds")]
        public WaysToDetermineBounds BoundsBasedOn;  


		public Vector3 Size { get; set; }
		protected virtual void Reset() 
		{
			DefineBoundsChoice();
   		}
		protected virtual void DefineBoundsChoice()
   		{
   			BoundsBasedOn = WaysToDetermineBounds.Undefined;
			if (GetComponent<Renderer>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Renderer;
			}
			if (GetComponent<Collider>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Collider;
			}
			if (GetComponent<Collider2D>()!=null)
			{
				BoundsBasedOn = WaysToDetermineBounds.Collider2D;
			}
   		}
   		public virtual Bounds GetBounds()
		{
			if (BoundsBasedOn==WaysToDetermineBounds.Renderer)
			{
				if (GetComponent<Renderer>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Renderer based bounds but no Renderer component can be found.");
				}
				return GetComponent<Renderer>().bounds;
			}

			if (BoundsBasedOn==WaysToDetermineBounds.Collider)
			{
				if (GetComponent<Collider>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Collider based bounds but no Collider component can be found.");
				}
				return GetComponent<Collider>().bounds;				
			}

			if (BoundsBasedOn==WaysToDetermineBounds.Collider2D)
			{
				if (GetComponent<Collider2D>()==null)
				{
					throw new Exception("The PoolableObject "+gameObject.name+" is set as having Collider2D based bounds but no Collider2D component can be found.");
				}
				return GetComponent<Collider2D>().bounds;				
			}

			return new Bounds(Vector3.zero,Vector3.zero);
   		}



	}
}
