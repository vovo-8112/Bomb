using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
	public static class MMAchievementManager
	{
		public static List<MMAchievement> AchievementsList { get { return _achievements; }}

		private static List<MMAchievement> _achievements;
		private static MMAchievement _achievement = null;

		private const string _defaultFileName = "Achievements";
		private const string _saveFolderName = "MMAchievements/";
		private const string _saveFileExtension = ".achievements";

		private static string _saveFileName;
		private static string _listID;
		public static void LoadAchievementList()
		{
			_achievements = new List<MMAchievement> ();
			MMAchievementList achievementList = (MMAchievementList) Resources.Load("Achievements/AchievementList");

			if (achievementList == null)
			{
				return;
			}
			_listID = achievementList.AchievementsListID;

			foreach (MMAchievement achievement in achievementList.Achievements)
			{
				_achievements.Add (achievement.Copy());
			}
		}
		public static void UnlockAchievement(string achievementID)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.UnlockAchievement();
			}
		}
		public static void LockAchievement(string achievementID)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.LockAchievement();
			}
		}
		public static void AddProgress(string achievementID, int newProgress)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.AddProgress(newProgress);
			}
		}
		public static void SetProgress(string achievementID, int newProgress)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.SetProgress(newProgress);
			}
		}
		private static MMAchievement AchievementManagerContains(string searchedID)
		{
			if (_achievements.Count == 0)
			{
				return null;
			}
			foreach(MMAchievement achievement in _achievements)
			{
				if (achievement.AchievementID == searchedID)
				{
					return achievement;					
				}
			}
			return null;
		}
		public static void ResetAchievements(string listID)
		{
			if (_achievements != null)
			{
				foreach(MMAchievement achievement in _achievements)
				{
					achievement.ProgressCurrent = 0;
					achievement.UnlockedStatus = false;
				}	
			}

			DeterminePath (listID);
			MMSaveLoadManager.DeleteSave(_saveFileName + _saveFileExtension, _saveFolderName);
			Debug.LogFormat ("Achievements Reset");
		}

		public static void ResetAllAchievements()
		{
			LoadAchievementList ();
			ResetAchievements (_listID);
		}
		public static void LoadSavedAchievements()
		{
			DeterminePath ();
			SerializedMMAchievementManager serializedMMAchievementManager = (SerializedMMAchievementManager)MMSaveLoadManager.Load(typeof(SerializedMMAchievementManager), _saveFileName+ _saveFileExtension, _saveFolderName);
			ExtractSerializedMMAchievementManager(serializedMMAchievementManager);
		}
		public static void SaveAchievements()
		{
			DeterminePath ();
			SerializedMMAchievementManager serializedMMAchievementManager = new SerializedMMAchievementManager();
			FillSerializedMMAchievementManager(serializedMMAchievementManager);
			MMSaveLoadManager.Save(serializedMMAchievementManager, _saveFileName+_saveFileExtension, _saveFolderName);
		}
		private static void DeterminePath(string specifiedFileName = "")
		{
			string tempFileName = (!string.IsNullOrEmpty(_listID)) ? _listID : _defaultFileName;
			if (!string.IsNullOrEmpty(specifiedFileName))
			{
				tempFileName = specifiedFileName;
			}

			_saveFileName = tempFileName;
		}
		public static void FillSerializedMMAchievementManager(SerializedMMAchievementManager serializedAchievements)
		{
			serializedAchievements.Achievements = new SerializedMMAchievement[_achievements.Count];

			for (int i = 0; i < _achievements.Count(); i++)
			{
				SerializedMMAchievement newAchievement = new SerializedMMAchievement (_achievements[i].AchievementID, _achievements[i].UnlockedStatus, _achievements[i].ProgressCurrent);
				serializedAchievements.Achievements [i] = newAchievement;
			}
		}
		public static void ExtractSerializedMMAchievementManager(SerializedMMAchievementManager serializedAchievements)
		{
			if (serializedAchievements == null)
			{
				return;
			}

			for (int i = 0; i < _achievements.Count(); i++)
			{
				for (int j=0; j<serializedAchievements.Achievements.Length; j++)
				{
					if (_achievements[i].AchievementID == serializedAchievements.Achievements[j].AchievementID)
					{
						_achievements [i].UnlockedStatus = serializedAchievements.Achievements [j].UnlockedStatus;
						_achievements [i].ProgressCurrent = serializedAchievements.Achievements [j].ProgressCurrent;
					}
				}
			}
		}
	}
}