﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Cinemachine;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("")]
    [FeedbackPath("Camera/Cinemachine Impulse")]
    [FeedbackHelp("This feedback lets you trigger a Cinemachine Impulse event. You'll need a Cinemachine Impulse Listener on your camera for this to work.")]
    public class MMFeedbackCinemachineImpulse : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
        #endif

        [Header("Cinemachine Impulse")]
        [Tooltip("the impulse definition to broadcast")]
        [CinemachineImpulseDefinitionProperty]
        public CinemachineImpulseDefinition m_ImpulseDefinition;
        [Tooltip("the velocity to apply to the impulse shake")]
        public Vector3 Velocity;
        [Tooltip("whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback")]
        public bool ClearImpulseOnStop = false;
        public override float FeedbackDuration { get { return m_ImpulseDefinition.m_TimeEnvelope.Duration; } }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                m_ImpulseDefinition.CreateEvent(position, Velocity * intensityMultiplier);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active && ClearImpulseOnStop)
            {
                CinemachineImpulseManager.Instance.Clear();
            }
        }
    }
}
