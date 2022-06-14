using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public abstract class MMAchievementRules : MonoBehaviour, MMEventListener<MMGameEvent>
	{
		protected virtual void Awake()
		{
			MMAchievementManager.LoadAchievementList ();
			MMAchievementManager.LoadSavedAchievements ();
		}
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
		}
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
		}
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "Save":
					MMAchievementManager.SaveAchievements ();
					break;
			}
		} 
	}
}