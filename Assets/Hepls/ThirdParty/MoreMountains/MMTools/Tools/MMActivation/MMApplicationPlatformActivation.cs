using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    [Serializable]
    public class PlatformBindings
    {
        public enum PlatformActions { DoNothing, Disable }
        public RuntimePlatform Platform = RuntimePlatform.WindowsPlayer;
        public PlatformActions PlatformAction = PlatformActions.DoNothing;
    }
    [AddComponentMenu("More Mountains/Tools/Activation/MMApplicationPlatformActivation")]
    public class MMApplicationPlatformActivation : MonoBehaviour
    {
        public enum ExecutionTimes { Awake, Start, OnEnable }
        
        [Header("Settings")]
        public ExecutionTimes ExecutionTime = ExecutionTimes.Awake;
        public bool DebugToTheConsole = false;

        [Header("Platforms")]
        public List<PlatformBindings> Platforms;
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
            foreach (PlatformBindings platform in Platforms)
            {
                if (platform.Platform == Application.platform)
                {
                    DisableIfNeeded(platform.PlatformAction, platform.Platform.ToString());
                }
            }

            if (Application.platform == RuntimePlatform.Android)
            {

            }
        }
        protected virtual void DisableIfNeeded(PlatformBindings.PlatformActions platform, string platformName)
        {
            if (this.gameObject.activeInHierarchy && (platform == PlatformBindings.PlatformActions.Disable))
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