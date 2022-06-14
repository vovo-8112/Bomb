﻿using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;

namespace MoreMountains.Feedbacks
{
    [Serializable]
    public class BlinkPhase
    {
        public float PhaseDuration = 1f;
        public float OffDuration = 0.2f;
        public float OnDuration = 0.1f;
        public float OffLerpDuration = 0.05f;
        public float OnLerpDuration = 0.05f;
    }
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMBlink")]
    public class MMBlink : MonoBehaviour
    {
        public enum States { On, Off }
        public enum Methods { SetGameObjectActive, MaterialAlpha, MaterialEmissionIntensity, ShaderFloatValue }
        
        [Header("Blink Method")]
        [Tooltip("the selected method to blink the target object")]
        public Methods Method = Methods.SetGameObjectActive;
        [Tooltip("the object to set active/inactive if that method was chosen")]
        [MMFEnumCondition("Method", (int)Methods.SetGameObjectActive)]
        public GameObject TargetGameObject;
        [Tooltip("the target renderer to work with")]
        [MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
        public Renderer TargetRenderer;
        [Tooltip("the shader property to alter a float on")]
        [MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
        public string ShaderPropertyName = "_Color";
        [Tooltip("the value to apply when blinking is off")]
        [MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
        public float OffValue = 0f;
        [Tooltip("the value to apply when blinking is on")]
        [MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
        public float OnValue = 1f;
        [Tooltip("whether to lerp these values or not")]
        [MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
        public bool LerpValue = true;
        [Tooltip("the curve to apply to the lerping")]
        [MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.05f), new Keyframe(1, 0));
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
        public bool UseMaterialPropertyBlocks = false;

        [Header("State")]
        [Tooltip("whether the object should blink or not")]
        public bool Blinking = true;
        [Tooltip("whether or not to force a certain state on exit")]
        public bool ForceStateOnExit = false;
        [Tooltip("the state to apply on exit")]
        [MMFCondition("ForceStateOnExit", true)]
        public States StateOnExit = States.On;

        [Header("Timescale")]
        [Tooltip("whether or not this MMBlink should operate on unscaled time")]
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        [Header("Sequence")]
        [Tooltip("how many times the sequence should repeat (-1 : infinite)")]
        public int RepeatCount = 0;
        [Tooltip("The list of phases to apply blinking with")]
        public List<BlinkPhase> Phases;
        
        [Header("Debug")]
        [Tooltip("Test button")]
        [MMFInspectorButton("ToggleBlinking")]
        public bool ToggleBlinkingButton;
        [Tooltip("Test button")]
        [MMFInspectorButton("StartBlinking")]
        public bool StartBlinkingButton;
        [Tooltip("Test button")]
        [MMFInspectorButton("StopBlinking")]
        public bool StopBlinkingButton;
        [Tooltip("is the blinking object in an active state right now?")]
        [MMFReadOnly]
        public bool Active = false;
        [Tooltip("the index of the phase we're currently in")]
        [MMFReadOnly]
        public int CurrentPhaseIndex = 0;
        
        
        public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

        protected float _lastBlinkAt = 0f;
        protected float _currentPhaseStartedAt = 0f;
        protected float _currentBlinkDuration;
        protected float _currentLerpDuration;
        protected int _propertyID;
        protected float _initialShaderFloatValue;
        protected Color _initialColor;
        protected Color _currentColor;
        protected int _repeatCount;
        protected MaterialPropertyBlock _propertyBlock;
        public virtual void ToggleBlinking()
        {
            Blinking = !Blinking;
            ResetBlinkProperties();
        }
        public virtual void StartBlinking()
        {
            this.enabled = true;
            Blinking = true;
            ResetBlinkProperties();
        }
        public virtual void StopBlinking()
        {
            Blinking = false;
            ResetBlinkProperties();
        }
        protected virtual void Update()
        {
            DetermineState();

            if (!Blinking)
            {
                return;
            }

            Blink();
        }
        protected virtual void DetermineState()
        {
            DetermineCurrentPhase();
            
            if (!Blinking)
            {
                return;
            }

            if (Active)
            {
                if (GetTime() - _lastBlinkAt > Phases[CurrentPhaseIndex].OnDuration)
                {
                    Active = false;
                    _lastBlinkAt = GetTime();
                }
            }
            else
            {
                if (GetTime() - _lastBlinkAt > Phases[CurrentPhaseIndex].OffDuration)
                {
                    Active = true;
                    _lastBlinkAt = GetTime();
                }
            }
            _currentBlinkDuration = Active ? Phases[CurrentPhaseIndex].OnDuration : Phases[CurrentPhaseIndex].OffDuration;
            _currentLerpDuration = Active ? Phases[CurrentPhaseIndex].OnLerpDuration : Phases[CurrentPhaseIndex].OffLerpDuration;
        }
        protected virtual void Blink()
        {
            float currentValue = _currentColor.a;
            float initialValue = Active ? OffValue : OnValue;
            float targetValue = Active ? OnValue : OffValue;
            float newValue = targetValue;

            if (LerpValue && (GetTime() - _lastBlinkAt < _currentLerpDuration))
            {
                float t = MMFeedbacksHelpers.Remap(GetTime() - _lastBlinkAt, 0f, _currentLerpDuration, 0f, 1f);
                newValue = Curve.Evaluate(t);
                newValue = MMFeedbacksHelpers.Remap(newValue, 0f, 1f, initialValue, targetValue);
            }
            else
            {
                newValue = targetValue;
            }
            
            ApplyBlink(Active, newValue);
        }

        protected virtual void ApplyBlink(bool active, float value)
        {
            switch (Method)
            {
                case Methods.SetGameObjectActive:
                    TargetGameObject.SetActive(active);
                    break;
                case Methods.MaterialAlpha:
                    _currentColor.a = value;
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetColor(_propertyID, _currentColor);
                        TargetRenderer.SetPropertyBlock(_propertyBlock);
                    }
                    else
                    {
                        TargetRenderer.material.SetColor(_propertyID, _currentColor);    
                    }
                    break;
                case Methods.MaterialEmissionIntensity:
                    _currentColor = _initialColor * value;
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetColor(_propertyID, _currentColor);
                        TargetRenderer.SetPropertyBlock(_propertyBlock);
                    }
                    else
                    {
                        TargetRenderer.material.SetColor(_propertyID, _currentColor);    
                    }
                    break;
                case Methods.ShaderFloatValue:
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetFloat(_propertyID, value);
                        TargetRenderer.SetPropertyBlock(_propertyBlock);
                    }
                    else
                    {
                        TargetRenderer.material.SetFloat(_propertyID, value); 
                    }
                    break;
            }
        }
        protected virtual void DetermineCurrentPhase()
        {
            if (Phases[CurrentPhaseIndex].PhaseDuration <= 0)
            {
                return;
            }
            if (GetTime() - _currentPhaseStartedAt > Phases[CurrentPhaseIndex].PhaseDuration)
            {
                CurrentPhaseIndex++;
                _currentPhaseStartedAt = GetTime();
            }
            if (CurrentPhaseIndex > Phases.Count -1)
            {
                CurrentPhaseIndex = 0;
                if (RepeatCount != -1)
                {
                    _repeatCount--;
                    if (_repeatCount < 0)
                    {
                        ResetBlinkProperties();

                        if (ForceStateOnExit)
                        {
                            if (StateOnExit == States.Off)
                            {
                                ApplyBlink(false, 0f);
                            }
                            else
                            {
                                ApplyBlink(true, 1f);
                            }
                        }

                        Blinking = false;
                    }
                }                
            }
        }
        protected virtual void OnEnable()
        {
            InitializeBlinkProperties();            
        }
        protected virtual void InitializeBlinkProperties()
        {
            if (Phases.Count == 0)
            {
                Debug.LogError("MMBlink : You need to define at least one phase for this component to work.");
                this.enabled = false;
                return;
            }
            
            _currentPhaseStartedAt = GetTime();
            CurrentPhaseIndex = 0;
            _repeatCount = RepeatCount;
            _propertyBlock = new MaterialPropertyBlock();
            if (TargetRenderer != null)
            {
                TargetRenderer.GetPropertyBlock(_propertyBlock);    
            }

            switch (Method)
            {
                case Methods.MaterialAlpha:
                    _propertyID = Shader.PropertyToID(ShaderPropertyName);
                    _initialColor = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterial.GetColor(_propertyID) : TargetRenderer.material.GetColor(_propertyID);
                    _currentColor = _initialColor;
                    break;
                case Methods.MaterialEmissionIntensity:
                    _propertyID = Shader.PropertyToID(ShaderPropertyName);
                    _initialColor = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterial.GetColor(_propertyID) : TargetRenderer.material.GetColor(_propertyID);
                    _currentColor = _initialColor;
                    break;
                case Methods.ShaderFloatValue:
                    _propertyID = Shader.PropertyToID(ShaderPropertyName);
                    _initialShaderFloatValue = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterial.GetFloat(_propertyID) : TargetRenderer.material.GetFloat(_propertyID);
                    break;
            }
        }
        protected virtual void ResetBlinkProperties()
        {
            _currentPhaseStartedAt = GetTime();
            CurrentPhaseIndex = 0;
            _repeatCount = RepeatCount;

            float value = 1f;
            if (Method == Methods.ShaderFloatValue)
            {
                value = _initialShaderFloatValue;
            }
            
            ApplyBlink(false, value);
        }
    }
}
