using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("TopDown Engine/Environment/Crouch Zone")]
    public class CrouchZone : MonoBehaviour
    {
        protected CharacterCrouch _characterCrouch;
        protected virtual void Start()
        {
            this.gameObject.MMGetComponentNoAlloc<Collider>().isTrigger = true;
        }
        protected virtual void OnTriggerEnter(Collider collider)
        {
            _characterCrouch = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterCrouch>();
            if (_characterCrouch != null)
            {
                _characterCrouch.StartForcedCrouch();
            }
        }
        protected virtual void OnTriggerExit(Collider collider)
        {
            _characterCrouch = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterCrouch>();
            if (_characterCrouch != null)
            {
                _characterCrouch.StopForcedCrouch();
            }
        }
    }
}
