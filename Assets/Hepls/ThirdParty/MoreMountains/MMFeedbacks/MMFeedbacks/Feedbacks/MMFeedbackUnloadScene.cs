using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you unload a scene by name or build index")]
    [FeedbackPath("Scene/Unload Scene")]
    public class MMFeedbackUnloadScene : MMFeedback
    {
        public enum ColorModes { Instant, Gradient, Interpolate }
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }
        #endif
        
        public enum Methods { BuildIndex, SceneName }

        [Header("Unload Scene")]
        [Tooltip("whether to unload a scene by build index or by name")]
        public Methods Method = Methods.SceneName;
        [Tooltip("the build ID of the scene to unload, find it in your Build Settings")]
        [MMFEnumCondition("Method", (int)Methods.BuildIndex)]
        public int BuildIndex = 0;
        [Tooltip("the name of the scene to unload")]
        [MMFEnumCondition("Method", (int)Methods.SceneName)]
        public string SceneName = "";
        [Tooltip("whether or not to output warnings if the scene doesn't exist or can't be loaded")]
        public bool OutputWarningsIfNeeded = true;
        
        protected Scene _sceneToUnload;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (Method == Methods.BuildIndex)
            {
                _sceneToUnload = SceneManager.GetSceneByBuildIndex(BuildIndex);
            }
            else
            {
                _sceneToUnload = SceneManager.GetSceneByName(SceneName);
            }

            if ((_sceneToUnload != null) && (_sceneToUnload.isLoaded))
            {
                SceneManager.UnloadSceneAsync(_sceneToUnload);    
            }
            else
            {
                if (OutputWarningsIfNeeded)
                {
                    Debug.LogWarning("Unload Scene Feedback : you're trying to unload a scene that hasn't been loaded.");    
                }
            }
        }
    }
}
