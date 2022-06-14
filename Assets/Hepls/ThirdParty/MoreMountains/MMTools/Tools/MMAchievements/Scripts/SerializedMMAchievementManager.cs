using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	[Serializable]
	public class SerializedMMAchievement
	{
		public string AchievementID;
		public bool UnlockedStatus;
		public int ProgressCurrent;
		public SerializedMMAchievement(string achievementID, bool unlockedStatus, int progressCurrent)
		{
			AchievementID = achievementID;
			UnlockedStatus = unlockedStatus;
			ProgressCurrent = progressCurrent;
		}
	}

	[Serializable]
	public class SerializedMMAchievementManager 
	{
		public SerializedMMAchievement[] Achievements;
	}
}