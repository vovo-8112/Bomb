using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public struct MMAchievementUnlockedEvent
	{
		public MMAchievement Achievement;
		public MMAchievementUnlockedEvent(MMAchievement newAchievement)
		{
			Achievement = newAchievement;
        }

        static MMAchievementUnlockedEvent e;
        public static void Trigger(MMAchievement newAchievement)
        {
            e.Achievement = newAchievement;
            MMEventManager.TriggerEvent(e);
        }
    }
}