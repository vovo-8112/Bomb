using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("")]
    [FeedbackPath("UI/TopDown Engine Floating Text")]
    [FeedbackHelp("This feedback lets you trigger the appearance of a floating text, that will reflect the damage done to the target Health component." +
        "This requires that a MMFloatingTextSpawner be correctly setup in the scene, otherwise nothing will happen." +
        "To do so, create a new empty object, add a MMFloatingTextSpawner to it. Drag (at least) one MMFloatingText prefab into its PooledSimpleMMFloatingText slot." +
        "You'll find such prefabs already made in the MMTools/Tools/MMFloatingText/Prefabs folder, but feel free to create your own.")]
    public class MMFeedbackTopDownEngineFloatingText : MMFeedbackFloatingText
    {
        [Header("TopDown Engine Settings")]
        [Tooltip("the Health component where damage data should be read")]
        public Health TargetHealth;
        [Tooltip("the number formatting of your choice, feel free to leave it blank")]
        public string Formatting = "";

        [Header("Direction")]
        [Tooltip("whether or not the direction of the damage should impact the direction of the floating text")]
        public bool DamageDirectionImpactsTextDirection = true;
        [Tooltip("the multiplier to apply to the damage direction. Usually you'll want it to be less than 1. With a value of 0.5, a character being hit from the left will spawn a floating text at a 45° up/right angle")]
        public float DamageDirectionMultiplier = 0.5f;
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                if (TargetHealth == null)
                {
                    return;
                }

                if (DamageDirectionImpactsTextDirection)
                {
                    Direction = TargetHealth.LastDamageDirection.normalized * DamageDirectionMultiplier;
                }

                Value = TargetHealth.LastDamage.ToString(Formatting);

                _playPosition = (PositionMode == PositionModes.FeedbackPosition) ? this.transform.position : TargetTransform.position;
                MMFloatingTextSpawnEvent.Trigger(Channel, _playPosition, Value, Direction, Intensity, ForceLifetime, Lifetime, ForceColor, AnimateColorGradient);
            }
        }
    }
}
