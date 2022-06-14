using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/GUI/LevelSelector")]
    public class LevelSelector : MonoBehaviour
	{
		[Tooltip("the exact name of the target level")]
		public string LevelName;
		[Tooltip("if this is true, GoToLevel will ignore the LevelManager and do a direct call")]
		public bool DoNotUseLevelManager = false;
	    public virtual void GoToLevel()
		{
			LoadScene(LevelName);
		}
		protected virtual void LoadScene(string newSceneName)
		{
			if (DoNotUseLevelManager)
			{
				MMAdditiveSceneLoadingManager.LoadScene(newSceneName);    
			}
			else
			{
				LevelManager.Instance.GotoLevel(newSceneName);   
			}
		}
        public virtual void RestartLevel()
        {
            if (GameManager.Instance.Paused)
            {
                TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
            }            
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnStarted, null);
        }
	    public virtual void ReloadLevel()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			LoadScene(SceneManager.GetActiveScene().name);
	    }
		
	}
}