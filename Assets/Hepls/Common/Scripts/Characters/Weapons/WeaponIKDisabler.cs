using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class WeaponIKDisabler : MonoBehaviour
    {
        [Header("Animation Parameter Names")]
        public List<string> AnimationParametersPreventingIK;
        
        [Header("Attachments")]
        public Transform WeaponAttachment;
        public Transform WeaponAttachmentParentNoIK;

        protected Transform _initialParent;
        protected Vector3 _initialLocalPosition;
        protected Vector3 _initialLocalScale;
        protected Quaternion _initialRotation;
        protected WeaponIK _weaponIK;
        protected Animator _animator;
        protected List<int> _animationParametersHashes;
        protected bool _shouldSetIKLast = true;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _weaponIK = this.gameObject.GetComponent<WeaponIK>();
            _animator = this.gameObject.GetComponent<Animator>();
            _animationParametersHashes = new List<int>();
            
            foreach (string _animationParameterName in AnimationParametersPreventingIK)
            {
                int newHash = Animator.StringToHash(_animationParameterName);
                _animationParametersHashes.Add(newHash);
            }

            if (WeaponAttachment != null)
            {
                _initialParent = WeaponAttachment.parent;
                _initialLocalPosition = WeaponAttachment.transform.localPosition;
                _initialLocalScale = WeaponAttachment.transform.localScale;
                _initialRotation = WeaponAttachment.transform.rotation;
            }
        }
        protected virtual void OnAnimatorIK(int layerIndex)
        {
            if ((_animator == null) || (_weaponIK == null) || (WeaponAttachment == null))
            {
                return;
            }

            if (_animationParametersHashes.Count <= 0)
            {
                return;
            }

            bool shouldPreventIK = false;
            foreach (int hash in _animationParametersHashes)
            {
                if (_animator.GetBool(hash))
                {
                    shouldPreventIK = true;
                }
            }

            if (shouldPreventIK != _shouldSetIKLast)
            {
                PreventIK(shouldPreventIK);
            }

            _shouldSetIKLast = shouldPreventIK;
        }
        protected virtual void PreventIK(bool status)
        {
            if (status)
            {
                _weaponIK.AttachLeftHand = false;
                _weaponIK.AttachRightHand = false;
                WeaponAttachment.transform.SetParent(WeaponAttachmentParentNoIK);
            }
            else
            {
                _weaponIK.AttachLeftHand = true;
                _weaponIK.AttachRightHand = true;
                WeaponAttachment.transform.SetParent(_initialParent);
                
                _initialRotation = WeaponAttachment.transform.rotation;
                
                WeaponAttachment.transform.localPosition = _initialLocalPosition;
                WeaponAttachment.transform.localScale = _initialLocalScale;
                WeaponAttachment.transform.rotation = _initialRotation;
            }
        }
    }
}
