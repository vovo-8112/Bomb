using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will request the load of a new scene, using the method of your choice")]
    [FeedbackPath("Scene/Load Scene")]
    public class MMFeedbackLoadScene : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }
        #endif
        public enum LoadingModes { Direct, MMSceneLoadingManager, MMAdditiveSceneLoadingManager }

        [Header("Scene Names")]
        [Tooltip("the name of the loading screen scene to use - HAS TO BE ADDED TO YOUR BUILD SETTINGS")]
        public string LoadingSceneName = "MMAdditiveLoadingScreen";
        [Tooltip("the name of the destination scene - HAS TO BE ADDED TO YOUR BUILD SETTINGS")]
        public string DestinationSceneName = "";

        [Header("Mode")]
        [Tooltip("the loading mode to use to load the destination scene : " +
                 "- direct : uses Unity's SceneManager API" +
                 "- MMSceneLoadingManager : the simple, original MM way of loading scenes" +
                 "- MMAdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options")]
        public LoadingModes LoadingMode = LoadingModes.MMAdditiveSceneLoadingManager;
        
        [Header("Loading Scene Manager")]
        [Tooltip("the priority to use when loading the new scenes")]
        public ThreadPriority Priority = ThreadPriority.High;
        [Tooltip("whether or not to interpolate progress (slower, but usually looks better and smoother)")]
        public bool InterpolateProgress = true;
        [Tooltip("whether or not to perform extra checks to make sure the loading screen and destination scene are in the build settings")]
        public bool SecureLoad = true;

        [Header("Loading Scene Delays")]
        [Tooltip("a delay (in seconds) to apply before the first fade plays")]
        public float BeforeEntryFadeDelay = 0f;
        [Tooltip("the duration (in seconds) of the entry fade")]
        public float EntryFadeDuration = 0.2f;
        [Tooltip("a delay (in seconds) to apply after the first fade plays")]
        public float AfterEntryFadeDelay = 0f;
        [Tooltip("a delay (in seconds) to apply before the exit fade plays")]
        public float BeforeExitFadeDelay = 0f;
        [Tooltip("the duration (in seconds) of the exit fade")]
        public float ExitFadeDuration = 0.2f;
        
        [Header("Transitions")]
        [Tooltip("the speed at which the progress bar should move if interpolated")]
        public float ProgressInterpolationSpeed = 5f;
        [Tooltip("the order in which to play fades (really depends on the type of fader you have in your loading screen")]
        public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
        [Tooltip("the tween to use on the entry fade")]
        public MMTweenType EntryFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
        [Tooltip("the tween to use on the exit fade")]
        public MMTweenType ExitFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            switch (LoadingMode)
            {
                case LoadingModes.Direct:
                    SceneManager.LoadScene(DestinationSceneName);
                    break;
                case LoadingModes.MMSceneLoadingManager:
                    MMSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName);
                    break;
                case LoadingModes.MMAdditiveSceneLoadingManager:
                    MMAdditiveSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName, 
                        Priority, SecureLoad, InterpolateProgress, 
                        BeforeEntryFadeDelay, EntryFadeDuration,
                        AfterEntryFadeDelay,
                        BeforeExitFadeDelay, ExitFadeDuration,
                        EntryFadeTween, ExitFadeTween,
                        ProgressInterpolationSpeed, FadeMode);
                    break;
            }
        }
    }
}
