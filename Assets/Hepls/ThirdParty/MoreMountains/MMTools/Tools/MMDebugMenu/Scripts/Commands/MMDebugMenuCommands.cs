using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    public class MMDebugMenuCommands : MonoBehaviour
    {
        [MMDebugLogCommand]
        public static void Now()
        {
            string message = "Time.time is " + Time.time;
            MMDebug.DebugLogTime(message, "", 3, true);
        }
        [MMDebugLogCommand]
        public static void Clear()
        {
            MMDebug.DebugLogClear();
        }
        [MMDebugLogCommand]
        public static void Restart()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
        }
        [MMDebugLogCommand]
        public static void Reload()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
        }
        [MMDebugLogCommand]
        public static void Sysinfo()
        {
            MMDebug.DebugLogTime(MMDebug.GetSystemInfo());
        }
        [MMDebugLogCommand]
        public static void Quit()
        {
            InternalQuit();
        }
        [MMDebugLogCommand]
        public static void Exit()
        {
            InternalQuit();
        }
        [MMDebugLogCommand]
        public static void Help()
        {
            string result = "LIST OF COMMANDS";
            foreach (MethodInfo method in MMDebug.Commands.OrderBy(m => m.Name))
            {
                result += "\n- <color=#FFFFFF>"+method.Name+"</color>";
            }
            MMDebug.DebugLogTime(result, "#FFC400", 3, true);
        }
        private static void InternalQuit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
        [MMDebugLogCommandArgumentCount(1)]
        [MMDebugLogCommand]
        public static void Vsync(string[] args)
        {
            if (int.TryParse(args[1], out int vSyncCount))
            {
                QualitySettings.vSyncCount = vSyncCount;
                MMDebug.DebugLogTime("VSyncCount set to " + vSyncCount, "#FFC400", 3, true);
            }
        }
        [MMDebugLogCommandArgumentCount(1)]
        [MMDebugLogCommand]
        public static void Framerate(string[] args)
        {
            if (int.TryParse(args[1], out int framerate))
            {
                Application.targetFrameRate = framerate;
                MMDebug.DebugLogTime("Framerate set to " + framerate, "#FFC400", 3, true);
            }
        }
        [MMDebugLogCommandArgumentCount(1)]
        [MMDebugLogCommand]
        public static void Timescale(string[] args)
        {
            if (float.TryParse(args[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float timescale))
            {
                Time.timeScale = timescale;
                MMDebug.DebugLogTime("Timescale set to " + timescale, "#FFC400", 3, true);
            }
        }
        [MMDebugLogCommandArgumentCount(2)]
        [MMDebugLogCommand]
        public static void Biggest(string[] args)
        {
            if (int.TryParse(args[1], out int i1) && int.TryParse(args[2], out int i2))
            {
                string result;
                int biggest = (i1 >= i2) ? i1 : i2;
                result = biggest + " is the biggest number";                
                MMDebug.DebugLogTime(result, "#FFC400", 3, true);
            }
        }
        
    }
}
