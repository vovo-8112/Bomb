using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{

	public class MMImage : MonoBehaviour 
	{
	    public static IEnumerator Flicker(Renderer renderer, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
	    {
	    	if (renderer==null)
	    	{
	    		yield break;
	    	}

	    	if (!renderer.material.HasProperty("_Color"))
	    	{
	    		yield break;
	    	}

			if (initialColor == flickerColor)
	        {
				yield break;
	        }

	        float flickerStop = Time.time + flickerDuration;

	        while (Time.time<flickerStop)
			{
				renderer.material.color = flickerColor;
				yield return MMCoroutine.WaitFor(flickerSpeed);
	            renderer.material.color = initialColor;
	            yield return MMCoroutine.WaitFor(flickerSpeed);
	        }

	        renderer.material.color = initialColor;        
	    }
	}
}

