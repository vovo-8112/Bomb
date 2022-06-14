using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you change the scene's skybox on play, replacing it with another one, either a specific one, or one picked at random among multiple skyboxes.")]
    [FeedbackPath("Renderer/Skybox")]
    public class MMFeedbackSkybox : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif
        public enum Modes { Single, Random }

        [Header("Skybox")]
        public Modes Mode = Modes.Single;
        public Material SingleSkybox;
        public Material[] RandomSkyboxes;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                if (Mode == Modes.Single)
                {
                    RenderSettings.skybox = SingleSkybox;
                }
                else if (Mode == Modes.Random)
                {
                    RenderSettings.skybox = RandomSkyboxes[Random.Range(0, RandomSkyboxes.Length)];
                }
            }
        }
    }    
}

