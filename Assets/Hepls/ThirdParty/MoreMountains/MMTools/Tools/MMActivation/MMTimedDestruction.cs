using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Activation/MMTimedDestruction")]
    public class MMTimedDestruction : MonoBehaviour
	{
		public enum TimedDestructionModes { Destroy, Disable }
		public TimedDestructionModes TimeDestructionMode = TimedDestructionModes.Destroy;
	    public float TimeBeforeDestruction=2;
		protected virtual void Start ()
		{
            StartCoroutine(Destruction());
		}
	    protected virtual IEnumerator Destruction()
	    {
            yield return MMCoroutine.WaitFor(TimeBeforeDestruction);

            if (TimeDestructionMode == TimedDestructionModes.Destroy)
			{
				Destroy(gameObject);
			}
			else
			{
				gameObject.SetActive(false);
			}	        
	    }
	}
}
