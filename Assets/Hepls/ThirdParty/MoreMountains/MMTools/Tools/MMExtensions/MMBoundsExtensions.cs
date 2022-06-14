using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	public class MMBoundsExtensions : MonoBehaviour 
	{
        public static Vector3 MMRandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
		public static Bounds GetColliderBounds(GameObject theObject)
	    {
	    	Bounds returnBounds;
			if (theObject.GetComponent<Collider>()!=null)
	    	{
				returnBounds = theObject.GetComponent<Collider>().bounds;
				return returnBounds;
	    	}
			if (theObject.GetComponent<Collider2D>()!=null) 
			{
				returnBounds = theObject.GetComponent<Collider2D>().bounds;
				return returnBounds;
			}
			if (theObject.GetComponentInChildren<Collider>()!=null)
			{
				Bounds totalBounds = theObject.GetComponentInChildren<Collider>().bounds;
				Collider[] colliders = theObject.GetComponentsInChildren<Collider>();
				foreach (Collider col in colliders) 
				{
					totalBounds.Encapsulate(col.bounds);
				}
				returnBounds = totalBounds;
				return returnBounds;
			}
			if (theObject.GetComponentInChildren<Collider2D>()!=null)
			{
				Bounds totalBounds = theObject.GetComponentInChildren<Collider2D>().bounds;
				Collider2D[] colliders = theObject.GetComponentsInChildren<Collider2D>();
				foreach (Collider2D col in colliders) 
				{
					totalBounds.Encapsulate(col.bounds);
				}
				returnBounds = totalBounds;
				return returnBounds;
			}

			returnBounds = new Bounds(Vector3.zero, Vector3.zero);
			return returnBounds;
		}
		public static Bounds GetRendererBounds(GameObject theObject)
	    {
	    	Bounds returnBounds;
			if (theObject.GetComponent<Renderer>()!=null)
	    	{
				returnBounds = theObject.GetComponent<Renderer>().bounds;
				return returnBounds;
	    	}
			if (theObject.GetComponentInChildren<Renderer>()!=null)
			{
				Bounds totalBounds = theObject.GetComponentInChildren<Renderer>().bounds;
				Renderer[] renderers = theObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in renderers) 
				{
					totalBounds.Encapsulate(renderer.bounds);
				}
				returnBounds = totalBounds;
				return returnBounds;
			}

			returnBounds = new Bounds(Vector3.zero, Vector3.zero);
			return returnBounds;
		}
	}
}
