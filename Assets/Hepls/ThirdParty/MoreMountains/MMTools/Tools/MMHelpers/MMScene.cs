using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    public class MMScene  
    {
        public static Scene[] GetLoadedScenes()
        {
            int sceneCount = SceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[sceneCount];
 
            for (int i = 0; i < sceneCount; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            return loadedScenes;
        }
        public static List<string> GetScenesInBuild()
        {
            List<string> scenesInBuild = new List<string>();
            
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                int lastSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
                scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".", StringComparison.Ordinal) - lastSlash - 1));
            }

            return scenesInBuild;
        }
        public static bool SceneInBuild(string sceneName)
        {
            return GetScenesInBuild().Contains(sceneName);
        }
    }
}