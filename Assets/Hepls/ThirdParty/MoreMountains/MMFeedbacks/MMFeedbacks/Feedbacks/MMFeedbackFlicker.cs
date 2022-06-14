﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you flicker the color of a specified renderer (sprite, mesh, etc) for a certain duration, at the specified octave, and with the specified color. Useful when a character gets hit, for example (but so much more!).")]
    [FeedbackPath("Renderer/Flicker")]
    public class MMFeedbackFlicker : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif
        public enum Modes { Color, PropertyName }

        [Header("Flicker")]
        [Tooltip("the renderer to flicker when played")]
        public Renderer BoundRenderer;
        [Tooltip("the selected mode to flicker the renderer")]
        public Modes Mode = Modes.Color;
        [MMFEnumCondition("Mode", (int)Modes.PropertyName)]
        [Tooltip("the name of the property to target")]
        public string PropertyName = "_Tint";
        [Tooltip("the duration of the flicker when getting damage")]
        public float FlickerDuration = 0.2f;
        [Tooltip("the frequency at which to flicker")]
        public float FlickerOctave = 0.04f;
        [Tooltip("the color we should flicker the sprite to")]
        [ColorUsage(true, true)]
        public Color FlickerColor = new Color32(255, 20, 20, 255);
        [Tooltip("the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0")]
        public int[] MaterialIndexes;
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
        public bool UseMaterialPropertyBlocks = false;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

        protected const string _colorPropertyName = "_Color";
        
        protected Color[] _initialFlickerColors;
        protected int[] _propertyIDs;
        protected bool[] _propertiesFound;
        protected Coroutine[] _coroutines;
        protected MaterialPropertyBlock _propertyBlock;
        protected override void CustomInitialization(GameObject owner)
        {
            if (MaterialIndexes.Length == 0)
            {
                MaterialIndexes = new int[1];
                MaterialIndexes[0] = 0;
            }

            _coroutines = new Coroutine[MaterialIndexes.Length];

            _initialFlickerColors = new Color[MaterialIndexes.Length];
            _propertyIDs = new int[MaterialIndexes.Length];
            _propertiesFound = new bool[MaterialIndexes.Length];
            _propertyBlock = new MaterialPropertyBlock();
            BoundRenderer.GetPropertyBlock(_propertyBlock);

            for (int i = 0; i < MaterialIndexes.Length; i++)
            {
                _propertiesFound[i] = false;

                if (Active && (BoundRenderer == null) && (owner != null))
                {
                    if (owner.MMFGetComponentNoAlloc<Renderer>() != null)
                    {
                        BoundRenderer = owner.GetComponent<Renderer>();
                    }
                    if (BoundRenderer == null)
                    {
                        BoundRenderer = owner.GetComponentInChildren<Renderer>();
                    }
                }

                if (Active && (BoundRenderer != null))
                {
                    if (Mode == Modes.Color)
                    {
                        _propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].HasProperty(_colorPropertyName) : BoundRenderer.materials[i].HasProperty(_colorPropertyName);
                        if (_propertiesFound[i])
                        {
                            _initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].color : BoundRenderer.materials[i].color;
                        }
                    }
                    else
                    {
                        _propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].HasProperty(PropertyName) : BoundRenderer.materials[i].HasProperty(PropertyName); 
                        if (_propertiesFound[i])
                        {
                            _propertyIDs[i] = Shader.PropertyToID(PropertyName);
                            _initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[i].GetColor(_propertyIDs[i]) : BoundRenderer.materials[i].GetColor(_propertyIDs[i]);
                        }
                    }
                }
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (BoundRenderer != null))
            {
                for (int i = 0; i < MaterialIndexes.Length; i++)
                {
                    _coroutines[i] = StartCoroutine(Flicker(BoundRenderer, i, _initialFlickerColors[i], FlickerColor, FlickerOctave, FeedbackDuration));
                }
            }
        }
        protected override void CustomReset()
        {
            base.CustomReset();

            if (InCooldown)
            {
                return;
            }

            if (Active && (BoundRenderer != null))
            {
                for (int i = 0; i < MaterialIndexes.Length; i++)
                {
                    SetColor(i, _initialFlickerColors[i]);
                }
            }
        }

        public virtual IEnumerator Flicker(Renderer renderer, int materialIndex, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
        {
            if (renderer == null)
            {
                yield break;
            }

            if (!_propertiesFound[materialIndex])
            {
                yield break;
            }

            if (initialColor == flickerColor)
            {
                yield break;
            }

            float flickerStop = FeedbackTime + flickerDuration;

            while (FeedbackTime < flickerStop)
            {
                SetColor(materialIndex, flickerColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
                SetColor(materialIndex, initialColor);
                if (Timing.TimescaleMode == TimescaleModes.Scaled)
                {
                    yield return MMFeedbacksCoroutine.WaitFor(flickerSpeed);
                }
                else
                {
                    yield return MMFeedbacksCoroutine.WaitForUnscaled(flickerSpeed);
                }
            }

            SetColor(materialIndex, initialColor);
        }

        protected virtual void SetColor(int materialIndex, Color color)
        {
            if (!_propertiesFound[materialIndex])
            {
                return;
            }
            
            if (Mode == Modes.Color)
            {
                if (UseMaterialPropertyBlocks)
                {
                    BoundRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(_colorPropertyName, color);
                    BoundRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
                }
                else
                {
                    BoundRenderer.materials[materialIndex].color = color;
                }
            }
            else
            {
                if (UseMaterialPropertyBlocks)
                {
                    BoundRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(_propertyIDs[materialIndex], color);
                    BoundRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
                }
                else
                {
                    BoundRenderer.materials[materialIndex].SetColor(_propertyIDs[materialIndex], color);
                }
            }            
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                for (int i = 0; i < _coroutines.Length; i++)
                {
                    if (_coroutines[i] != null)
                    {
                        StopCoroutine(_coroutines[i]);    
                    }
                    _coroutines[i] = null;    
                }
            }
        }
    }
}
