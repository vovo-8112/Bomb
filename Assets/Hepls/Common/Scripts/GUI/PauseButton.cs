using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/GUI/PauseButton")]
    public class PauseButton : MonoBehaviour
	{
	    public virtual void PauseButtonAction()
	    {
            StartCoroutine(PauseButtonCo());

        }
        public virtual void UnPause()
        {
            StartCoroutine(PauseButtonCo());
        }
        protected virtual IEnumerator PauseButtonCo()
        {
            yield return null;
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.TogglePause, null);
        }

    }
}