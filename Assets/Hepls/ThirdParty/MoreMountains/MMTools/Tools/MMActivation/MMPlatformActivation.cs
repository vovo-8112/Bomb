using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Activation/MMPlatformActivation")]
    public class MMPlatformActivation : MonoBehaviour
    {
        public enum ExecutionTimes { Awake, Start, OnEnable }
        public enum PlatformActions { DoNothing, Disable }
        
        [Header("Settings")]
        public ExecutionTimes ExecutionTime = ExecutionTimes.Awake;
        public bool DebugToTheConsole = false;

        [Header("Desktop")]
        public PlatformActions UNITY_STANDALONE_WIN = PlatformActions.DoNothing;
        public PlatformActions UNITY_STANDALONE_OSX = PlatformActions.DoNothing;
        public PlatformActions UNITY_STANDALONE_LINUX = PlatformActions.DoNothing;
        public PlatformActions UNITY_STANDALONE = PlatformActions.DoNothing;

        [Header("Mobile")]
        public PlatformActions UNITY_IOS = PlatformActions.DoNothing;
        public PlatformActions UNITY_IPHONE = PlatformActions.DoNothing;
        public PlatformActions UNITY_ANDROID = PlatformActions.DoNothing;
        public PlatformActions UNITY_TIZEN = PlatformActions.DoNothing;

        [Header("Console")]
        public PlatformActions UNITY_WII = PlatformActions.DoNothing;
        public PlatformActions UNITY_PS4 = PlatformActions.DoNothing;
        public PlatformActions UNITY_XBOXONE = PlatformActions.DoNothing;

        [Header("Others")]
        public PlatformActions UNITY_WEBGL = PlatformActions.DoNothing;
        public PlatformActions UNITY_LUMIN = PlatformActions.DoNothing;
        public PlatformActions UNITY_TVOS = PlatformActions.DoNothing;
        public PlatformActions UNITY_WSA = PlatformActions.DoNothing;
        public PlatformActions UNITY_FACEBOOK = PlatformActions.DoNothing;
        public PlatformActions UNITY_ADS = PlatformActions.DoNothing;
        public PlatformActions UNITY_ANALYTICS = PlatformActions.DoNothing;

        [Header("Active in Editor")]
        public PlatformActions UNITY_EDITOR = PlatformActions.DoNothing;
        public PlatformActions UNITY_EDITOR_WIN = PlatformActions.DoNothing;
        public PlatformActions UNITY_EDITOR_OSX = PlatformActions.DoNothing;
        public PlatformActions UNITY_EDITOR_LINUX = PlatformActions.DoNothing;
        protected virtual void OnEnable()
        {
            if (ExecutionTime == ExecutionTimes.OnEnable)
            {
                Process();
            }
        }
        protected virtual void Awake()
        {
            if (ExecutionTime == ExecutionTimes.Awake)
            {
                Process();
            }            
        }
        protected virtual void Start()
        {
            if (ExecutionTime == ExecutionTimes.Start)
            {
                Process();
            }            
        }
        protected virtual void Process()
        {

            #if UNITY_STANDALONE_WIN
                DisableIfNeeded(UNITY_STANDALONE_WIN, "Windows");
            #endif

            #if UNITY_STANDALONE_OSX
                DisableIfNeeded(UNITY_STANDALONE_OSX, "OSX");
            #endif

            #if UNITY_STANDALONE_LINUX
                DisableIfNeeded(UNITY_STANDALONE_LINUX, "Linux");
            #endif

            #if UNITY_STANDALONE
                DisableIfNeeded(UNITY_STANDALONE, "Standalone");
            #endif

            #if UNITY_IOS
                DisableIfNeeded(UNITY_IOS, "iOS");
            #endif

            #if UNITY_IPHONE
                DisableIfNeeded(UNITY_IPHONE, "iPhone");
            #endif

            #if UNITY_ANDROID
                DisableIfNeeded(UNITY_ANDROID, "Android");
            #endif

            #if UNITY_TIZEN
                DisableIfNeeded(UNITY_TIZEN, "Tizen");
            #endif

            #if UNITY_WII
                DisableIfNeeded(UNITY_WII, "Wii");
            #endif

            #if UNITY_PS4
                DisableIfNeeded(UNITY_PS4, "PS4");
            #endif

            #if UNITY_XBOXONE
                DisableIfNeeded(UNITY_XBOXONE, "XBoxOne");
            #endif

            #if UNITY_WEBGL
                DisableIfNeeded(UNITY_WEBGL, "WebGL");
            #endif

            #if UNITY_LUMIN
                DisableIfNeeded(UNITY_LUMIN, "Lumin");
            #endif

            #if UNITY_TVOS
                DisableIfNeeded(UNITY_TVOS, "TV OS");
            #endif

            #if UNITY_WSA
                DisableIfNeeded(UNITY_WSA, "WSA");
            #endif

            #if UNITY_FACEBOOK
                DisableIfNeeded(UNITY_FACEBOOK, "Facebook");
            #endif

            #if UNITY_ADS
                DisableIfNeeded(UNITY_ADS, "Ads");
            #endif

            #if UNITY_ANALYTICS
                DisableIfNeeded(UNITY_ANALYTICS, "Analytics");
            #endif

            #if UNITY_EDITOR
                DisableIfNeeded(UNITY_EDITOR, "Editor");
            #endif

            #if UNITY_EDITOR_WIN
                DisableIfNeeded(UNITY_EDITOR_WIN, "Editor Windows");
            #endif

            #if UNITY_EDITOR_OSX
                DisableIfNeeded(UNITY_EDITOR_OSX, "Editor OSX");
            #endif

            #if UNITY_EDITOR_LINUX
                DisableIfNeeded(UNITY_EDITOR_LINUX, "Editor Linux");
            #endif
    }
    protected virtual void DisableIfNeeded(PlatformActions platform, string platformName)
        {
            if (this.gameObject.activeInHierarchy && (platform == PlatformActions.Disable))
            {
                this.gameObject.SetActive(false);
                if (DebugToTheConsole)
                {
                    Debug.LogFormat(this.gameObject.name + " got disabled via MMPlatformActivation, platform : " + platformName + ".");
                }
            }
        }
    }
}
