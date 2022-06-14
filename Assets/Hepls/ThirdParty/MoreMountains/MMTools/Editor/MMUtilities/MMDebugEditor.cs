using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{
    public class MMDebugEditor
    {
        [MenuItem("Tools/More Mountains/Enable Debug Logs", false, 100)]
        private static void EnableDebugLogs()
        {
            MMDebug.SetDebugLogsEnabled(true);
        }
        [MenuItem("Tools/More Mountains/Enable Debug Logs", true)]
        private static bool EnableDebugLogsValidation()
        {
            return !MMDebug.DebugLogsEnabled;
        }
        [MenuItem("Tools/More Mountains/Disable Debug Logs", false, 101)]
        private static void DisableDebugLogs()
        {
            MMDebug.SetDebugLogsEnabled(false);
        }
        [MenuItem("Tools/More Mountains/Disable Debug Logs", true)]
        private static bool DisableDebugLogsValidation()
        {
            return MMDebug.DebugLogsEnabled;
        }
        [MenuItem("Tools/More Mountains/Enable Debug Draws", false, 102)]
        private static void EnableDebugDraws()
        {
            MMDebug.SetDebugDrawEnabled(true);
        }

        [MenuItem("Tools/More Mountains/Enable Debug Draws", true)]
        private static bool EnableDebugDrawsValidation()
        {
            return !MMDebug.DebugDrawEnabled;
        }

        [MenuItem("Tools/More Mountains/Disable Debug Draws", false, 103)]
        private static void DisableDebugDraws()
        {
            MMDebug.SetDebugDrawEnabled(false);
        }

        [MenuItem("Tools/More Mountains/Disable Debug Draws", true)]
        private static bool DisableDebugDrawsValidation()
        {
            return MMDebug.DebugDrawEnabled;
        }

    }
}
