using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(MMConeOfVision))]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Cone of Vision")]
    public class CharacterConeOfVision : MonoBehaviour
    {
        protected MMConeOfVision _coneOfVision;
        protected CharacterOrientation3D _characterOrientation;
        protected virtual void Awake()
        {
            _characterOrientation = this.gameObject.GetComponentInParent<CharacterOrientation3D>();
            _coneOfVision = this.gameObject.GetComponent<MMConeOfVision>();
        }
        protected virtual void Update()
        {
            UpdateDirection();   
        }
        protected virtual void UpdateDirection()
        {
            if (_characterOrientation == null)
            {
                _coneOfVision.SetDirectionAndAngles(this.transform.forward, this.transform.eulerAngles);              
            }
            else
            {
                _coneOfVision.SetDirectionAndAngles(_characterOrientation.ModelDirection, _characterOrientation.ModelAngles);              
            }
        }
    }
}
