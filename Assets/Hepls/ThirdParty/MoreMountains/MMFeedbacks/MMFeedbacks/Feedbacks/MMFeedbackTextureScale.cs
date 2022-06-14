using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you control the texture scale of a target material over time.")]
    [FeedbackPath("Renderer/Texture Scale")]
    public class MMFeedbackTextureScale : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif
        public enum Modes { OverTime, Instant }

        [Header("Material")]
        [Tooltip("the renderer on which to change texture scale on")]
        public Renderer TargetRenderer;
        [Tooltip("the material index")]
        public int MaterialIndex = 0;
        [Tooltip("the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true")]
        public string MaterialPropertyName = "_MainTex_ST";
        [Tooltip("whether the feedback should affect the material instantly or over a period of time")]
        public Modes Mode = Modes.OverTime;
        [Tooltip("how long the material should change over time")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public float Duration = 0.2f;
        [Tooltip("whether or not the values should be relative")]
        public bool RelativeValues = true;
        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
        public bool AllowAdditivePlays = false;
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
        public bool UseMaterialPropertyBlocks = false;

        [Header("Intensity")]
        [Tooltip("the curve to tween the scale on")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        [Tooltip("the value to remap the scale curve's 0 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public Vector2 RemapZero = Vector2.zero;
        [Tooltip("the value to remap the scale curve's 1 to")]
        [MMFEnumCondition("Mode", (int)Modes.OverTime)]
        public Vector2 RemapOne = Vector2.one;
        [Tooltip("the value to move the intensity to in instant mode")]
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public Vector2 InstantScale;

        protected Vector2 _initialValue;
        protected Coroutine _coroutine;
        protected Vector2 _newValue;
        protected MaterialPropertyBlock _propertyBlock;
        protected Vector4 _propertyBlockVector;
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (UseMaterialPropertyBlocks)
            {
                _propertyBlock = new MaterialPropertyBlock();
                TargetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlockVector.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).w;
                _propertyBlockVector.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).z;
                _initialValue.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
                _initialValue.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;    
            }
            else
            {
                _initialValue = TargetRenderer.materials[MaterialIndex].GetTextureScale(MaterialPropertyName);    
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                
                switch (Mode)
                {
                    case Modes.Instant:      
                        ApplyValue(InstantScale * intensityMultiplier);
                        break;
                    case Modes.OverTime:
                        if (!AllowAdditivePlays && (_coroutine != null))
                        {
                            return;
                        }
                        _coroutine = StartCoroutine(TransitionCo(intensityMultiplier));

                        break;
                }
            }
        }
        protected virtual IEnumerator TransitionCo(float intensityMultiplier)
        {
            float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

                SetMaterialValues(remappedTime, intensityMultiplier);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }
            SetMaterialValues(FinalNormalizedTime, intensityMultiplier);
            
            _coroutine = null;
            yield return null;
        }
        protected virtual void SetMaterialValues(float time, float intensityMultiplier)
        {
            _newValue.x = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.x, RemapOne.x);
            _newValue.y = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.y, RemapOne.y);

            if (RelativeValues)
            {
                _newValue += _initialValue;
            }

            ApplyValue(_newValue * intensityMultiplier);
        }
        protected virtual void ApplyValue(Vector2 newValue)
        {
            if (UseMaterialPropertyBlocks)
            {
                TargetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlockVector.x = newValue.x;
                _propertyBlockVector.y = newValue.y;
                _propertyBlock.SetVector(MaterialPropertyName, _propertyBlockVector);
                TargetRenderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
            }
            else
            {
                TargetRenderer.materials[MaterialIndex].SetTextureScale(MaterialPropertyName, newValue);    
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active && (_coroutine != null))
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
}
