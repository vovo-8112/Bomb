using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	public enum AchievementTypes { Simple, Progress }

	[Serializable]
	public class MMAchievement  
	{
		[Header("Identification")]
		public string AchievementID;
		public AchievementTypes AchievementType;
		public bool HiddenAchievement;
		public bool UnlockedStatus;

		[Header("Description")]
		public string Title;
		public string Description;
		public int Points;

		[Header("Image and Sounds")]
		public Sprite LockedImage;
		public Sprite UnlockedImage;
		public AudioClip UnlockedSound;

		[Header("Progress")]
		public int ProgressTarget;
		public int ProgressCurrent;

		protected MMAchievementDisplayItem _achievementDisplayItem;
		public virtual void UnlockAchievement()
		{
			if (UnlockedStatus)
			{
				return;
			}

			UnlockedStatus = true;

			MMGameEvent.Trigger("Save");
			MMAchievementUnlockedEvent.Trigger(this);
		}
		public virtual void LockAchievement()
		{
			UnlockedStatus = false;
		}
		public virtual void AddProgress(int newProgress)
		{
			ProgressCurrent += newProgress;
			EvaluateProgress();
		}
		public virtual void SetProgress(int newProgress)
		{
			ProgressCurrent = newProgress;
			EvaluateProgress();
		}
		protected virtual void EvaluateProgress()
		{
			if (ProgressCurrent >= ProgressTarget)
			{
				ProgressCurrent = ProgressTarget;
				UnlockAchievement();
			}
		}
		public virtual MMAchievement Copy()
		{
			MMAchievement clone = new MMAchievement ();
			clone = JsonUtility.FromJson<MMAchievement>(JsonUtility.ToJson(this));
			return clone;
		}
	}
}