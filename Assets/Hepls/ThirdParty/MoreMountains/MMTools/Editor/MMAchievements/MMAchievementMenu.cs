using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{	
	public static class MMAchievementMenu 
	{
		[MenuItem("Tools/More Mountains/Reset all achievements", false,21)]
		private static void EnableHelpInInspectors()
		{
			MMAchievementManager.ResetAllAchievements ();
		}
	}
}