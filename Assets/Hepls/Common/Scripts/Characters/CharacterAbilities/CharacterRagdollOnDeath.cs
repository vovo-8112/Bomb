using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Ragdoll on Death")]
    public class CharacterRagdollOnDeath : MonoBehaviour
    {
        [Header("Binding")]
        [Tooltip("the MMRagdoller for this character")]
        public MMRagdoller Ragdoller;
        [Tooltip("A list of optional objects to disable on death")]
        public List<GameObject> ObjectsToDisableOnDeath;
        [Tooltip("A list of optional monos to disable on death")]
        public List<MonoBehaviour> MonosToDisableOnDeath;

        [Header("Force")]
        [Tooltip("the force by which the impact will be multiplied")]
        public float ForceMultiplier = 10000f;

        [Header("Test")]
        [MMInspectorButton("Ragdoll")]
        [Tooltip("A test button to trigger the ragdoll from the inspector")]
        public bool RagdollButton;
        
        protected TopDownController _controller;
        protected Health _health;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _health = this.gameObject.GetComponent<Health>();
            _controller = this.gameObject.GetComponent<TopDownController>();
        }
        protected virtual void OnDeath()
        {
            Ragdoll();
        }
        protected virtual void Ragdoll()
        {
            foreach (GameObject go in ObjectsToDisableOnDeath)
            {
                go.SetActive(false);
            }
            foreach (MonoBehaviour mono in MonosToDisableOnDeath)
            {
                mono.enabled = false;
            }
            Ragdoller.Ragdolling = true;
            Ragdoller.transform.SetParent(null);
            Ragdoller.MainRigidbody.AddForce(_controller.AppliedImpact.normalized * ForceMultiplier, ForceMode.Acceleration);
        }
        protected virtual void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDeath += OnDeath;
            }
        }
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
            }
        }
    }
}
