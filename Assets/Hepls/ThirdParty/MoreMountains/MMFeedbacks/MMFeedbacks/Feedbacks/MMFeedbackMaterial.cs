using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you change the material of the target renderer everytime it's played.")]
    [FeedbackPath("Renderer/Material")]
    public class MMFeedbackMaterial : MMFeedback
    {
        public override float FeedbackDuration { get { return (InterpolateTransition) ? TransitionDuration : 0f; } set { if (InterpolateTransition) { TransitionDuration = value; } } }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif
        public enum Methods { Sequential, Random }

        [Header("Material")]
        [Tooltip("the renderer to change material on")]
        public Renderer TargetRenderer;
        [Tooltip("the selected method")]
        public Methods Method;
        [MMFEnumCondition("Method", (int)Methods.Sequential)]
        [Tooltip("whether or not the sequential order should loop")]
        public bool Loop = true;
        [MMFEnumCondition("Method", (int)Methods.Random)]        
        [Tooltip("whether or not to always pick a new material in random mode")]
        public bool AlwaysNewMaterial = true;
        [Tooltip("the initial index to start with")]
        public int InitialIndex = 0;
        [Tooltip("the list of materials to pick from")]
        public List<Material> Materials;

        [Header("Interpolation")]
        public bool InterpolateTransition = false;
        public float TransitionDuration = 1f;
        public AnimationCurve TransitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        public virtual float GetTime() { return (Timing.TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (Timing.TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
        protected int _currentIndex;
        protected float _startedAt;
        protected Coroutine _coroutine;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            _currentIndex = InitialIndex;
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Materials.Count == 0)
            {
                Debug.LogError("[MMFeedbackMaterial on " + this.name + "] The Materials array is empty.");
                return;
            }

            int newIndex = DetermineNextIndex();

            if (Materials[newIndex] == null)
            {
                Debug.LogError("[MMFeedbackMaterial on " + this.name + "] Attempting to switch to a null material.");
                return;
            }

            if (InterpolateTransition)
            {
                _coroutine = StartCoroutine(TransitionMaterial(TargetRenderer.material, Materials[newIndex]));
            }
            else
            {
                TargetRenderer.material = Materials[newIndex];
            }            
        }
        protected virtual IEnumerator TransitionMaterial(Material originalMaterial, Material newMaterial)
        {
            _startedAt = GetTime();
            while (GetTime() - _startedAt < TransitionDuration)
            {
                float time = MMFeedbacksHelpers.Remap(GetTime() - _startedAt, 0f, TransitionDuration, 0f, 1f);
                float t = TransitionCurve.Evaluate(time);
                TargetRenderer.material.Lerp(originalMaterial, newMaterial, t);
                yield return null;
            }
            float finalt = TransitionCurve.Evaluate(1f);
            TargetRenderer.material.Lerp(originalMaterial, newMaterial, finalt);
        }
        protected virtual int DetermineNextIndex()
        {
            switch(Method)
            {
                case Methods.Random:
                    int random = Random.Range(0, Materials.Count);
                    if (AlwaysNewMaterial)
                    {
                        while (_currentIndex == random)
                        {
                            random = Random.Range(0, Materials.Count);
                        }
                    }
                    _currentIndex = random;
                    return _currentIndex;                    

                case Methods.Sequential:
                    _currentIndex++;
                    if (_currentIndex >= Materials.Count)
                    {
                        _currentIndex = Loop ? 0 : _currentIndex;
                    }
                    return _currentIndex;
            }
            return 0;
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
