using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Damage/KillZone")]
    public class KillZone : MonoBehaviour
    {
        [Header("Targets")]
        [MMInformation("This component will make your object kill objects that collide with it. Here you can define what layers will be killed.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the layers containing the objects that will be damaged by this object")]
        public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;

        protected Health _colliderHealth;
        protected virtual void Awake()
        {

        }
        protected virtual void OnEnable()
        {

        }
        public virtual void OnTriggerStay2D(Collider2D collider)
        {
            Colliding(collider.gameObject);
        }
        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Colliding(collider.gameObject);
        }
        public virtual void OnTriggerStay(Collider collider)
        {
            Colliding(collider.gameObject);
        }
        public virtual void OnTriggerEnter(Collider collider)
        {
            Colliding(collider.gameObject);
        }
        protected virtual void Colliding(GameObject collider)
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
            {
                return;
            }

            _colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();
            if (_colliderHealth != null)
            {
                if (_colliderHealth.CurrentHealth > 0)
                {
                    _colliderHealth.Kill();
                }                
            }
        }
    }
}