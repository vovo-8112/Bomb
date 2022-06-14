using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Cinemachine;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("")]
    [FeedbackPath("Camera/Cinemachine Transition")]
    [FeedbackHelp("This feedback will let you change the priorities of your cameras. It requires a bit of setup : " +
        "adding a MMCinemachinePriorityListener to your different cameras, with unique Channel values on them. " +
        "Optionally, you can add a MMCinemachinePriorityBrainListener on your Cinemachine Brain to handle different transition types and durations. " +
        "Then all you have to do is pick a channel and a new priority on your feedback, and play it. Magic transition!")]
    public class MMFeedbackCinemachineTransition : MMFeedback
    {
        public enum Modes { Event, Binding }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(BlendDefintion.m_Time); } set { BlendDefintion.m_Time = value; } }

        [Header("Cinemachine Transition")]
        [Tooltip("the selected mode (either via event, or via direct binding of a specific camera)")]
        public Modes Mode = Modes.Event;
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the virtual camera to target")]
        [MMFEnumCondition("Mode", (int)Modes.Binding)]
        public CinemachineVirtualCamera TargetVirtualCamera;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetValuesAfterTransition = true;

        [Header("Priority")]
        [Tooltip("the new priority to apply to all virtual cameras on the specified channel")]
        public int NewPriority = 10;
        [Tooltip("whether or not to force all virtual cameras on other channels to reset their priority to zero")]
        public bool ForceMaxPriority = true;
        [Tooltip("whether or not to apply a new blend")]
        public bool ForceTransition = false;
        [Tooltip("the new blend definition to apply")]
        [MMFCondition("ForceTransition", true)]
        public CinemachineBlendDefinition BlendDefintion;

        protected CinemachineBlendDefinition _tempBlend;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                _tempBlend = BlendDefintion;
                _tempBlend.m_Time = FeedbackDuration;
                if (Mode == Modes.Event)
                {
                    MMCinemachinePriorityEvent.Trigger(Channel, ForceMaxPriority, NewPriority, ForceTransition, _tempBlend, ResetValuesAfterTransition, Timing.TimescaleMode);    
                }
                else
                {
                    MMCinemachinePriorityEvent.Trigger(Channel, ForceMaxPriority, 0, ForceTransition, _tempBlend, ResetValuesAfterTransition, Timing.TimescaleMode); 
                    TargetVirtualCamera.Priority = NewPriority;
                }
            }
        }
    }
}
