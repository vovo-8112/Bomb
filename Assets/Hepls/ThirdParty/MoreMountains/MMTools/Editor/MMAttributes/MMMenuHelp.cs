using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{
	public static class MMMenuHelp
    {
		[MenuItem("Tools/More Mountains/Enable Help in Inspectors", false,0)]
		private static void EnableHelpInInspectors()
	    {
			SetHelpEnabled(true);
		}

		[MenuItem("Tools/More Mountains/Enable Help in Inspectors", true)]
		private static bool EnableHelpInInspectorsValidation()
		{
			return !HelpEnabled();
		}

		[MenuItem("Tools/More Mountains/Disable Help in Inspectors", false,1)]
		private static void DisableHelpInInspectors()
	    {
			SetHelpEnabled(false);
		}
		 
		[MenuItem("Tools/More Mountains/Disable Help in Inspectors", true)]
		private static bool DisableHelpInInspectorsValidation()
		{
			return HelpEnabled();
		}
		private static bool HelpEnabled()
		{
			if (EditorPrefs.HasKey("MMShowHelpInInspectors"))
			{
				return EditorPrefs.GetBool("MMShowHelpInInspectors");
			}
			else
			{
				EditorPrefs.SetBool("MMShowHelpInInspectors",true);
				return true;
			}
		}
		private static void SetHelpEnabled(bool status)
		{
			EditorPrefs.SetBool("MMShowHelpInInspectors",status);
			SceneView.RepaintAll();

		}
	}
}