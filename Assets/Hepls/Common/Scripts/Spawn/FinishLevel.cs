using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Spawn/Finish Level")]
	public class FinishLevel : ButtonActivated
	{
		[Header("Finish Level")]
        [Tooltip("the exact name of the level to transition to ")]
		public string LevelName;
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction ();
			GoToNextLevel();
		}
	    public virtual void GoToNextLevel()
	    {
	    	if (LevelManager.Instance != null)
	    	{
				LevelManager.Instance.GotoLevel(LevelName);
	    	}
	    	else
	    	{
		        MMSceneLoadingManager.LoadScene(LevelName);
			}
	    }
	}
}