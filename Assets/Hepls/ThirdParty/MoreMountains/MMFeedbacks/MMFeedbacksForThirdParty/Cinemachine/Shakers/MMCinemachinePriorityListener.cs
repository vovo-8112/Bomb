using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachinePriorityListener")]
    [RequireComponent(typeof(CinemachineVirtualCameraBase))]
    public class MMCinemachinePriorityListener : MonoBehaviour
    {
        
        [HideInInspector] 
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
        public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
        [Header("Priority Listener")]
        [Tooltip("the channel to listen to")]
        public int Channel = 0;

        protected CinemachineVirtualCameraBase _camera;
        protected virtual void Awake()
        {
            _camera = this.gameObject.GetComponent<CinemachineVirtualCameraBase>();
        }
        public virtual void OnMMCinemachinePriorityEvent(int channel, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode)
        {
            TimescaleMode = timescaleMode;
            if (channel == Channel)
            {
                _camera.Priority = newPriority;
            }
            else
            {
                if (forceMaxPriority)
                {
                    _camera.Priority = 0;
                }
            }
        }
        protected virtual void OnEnable()
        {
            MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
        }
        protected virtual void OnDisable()
        {
            MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
        }
    }
    public struct MMCinemachinePriorityEvent
    {
        public delegate void Delegate(int channel, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode);
        static private event Delegate OnEvent;

        static public void Register(Delegate callback)
        {
            OnEvent += callback;
        }

        static public void Unregister(Delegate callback)
        {
            OnEvent -= callback;
        }

        static public void Trigger(int channel, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode)
        {
            OnEvent?.Invoke(channel, forceMaxPriority, newPriority, forceTransition, blendDefinition, resetValuesAfterTransition, timescaleMode);
        }
    }
}
