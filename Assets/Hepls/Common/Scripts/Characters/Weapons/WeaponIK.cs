using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Weapons/Weapon IK")]
    public class WeaponIK : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("The transform to use as a target for the left hand")]
        public Transform LeftHandTarget = null;
        [Tooltip("The transform to use as a target for the right hand")]
        public Transform RightHandTarget = null;

        [Header("Attachments")]
        [Tooltip("whether or not to attach the left hand to its target")]
        public bool AttachLeftHand = true;
        [Tooltip("whether or not to attach the right hand to its target")]
        public bool AttachRightHand = true;
        
        [Header("Head")]
        [MMVector("Min","Max")]
        public Vector2 HeadWeights = new Vector2(0f, 1f);
        
        protected Animator _animator;

		protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
		}
		protected virtual void OnAnimatorIK(int layerIndex)
		{
			if (_animator == null)
			{
				return;
			}

            if (AttachLeftHand)
            {
                if (LeftHandTarget != null)
                {
                    AttachHandToHandle(AvatarIKGoal.LeftHand, LeftHandTarget);

                    _animator.SetLookAtWeight(HeadWeights.y);
                    _animator.SetLookAtPosition(LeftHandTarget.position);
                }
                else
                {
                    DetachHandFromHandle(AvatarIKGoal.LeftHand);
                }
            }
			
            if (AttachRightHand)
            {
                if (RightHandTarget != null)
                {
                    AttachHandToHandle(AvatarIKGoal.RightHand, RightHandTarget);
                }
                else
                {
                    DetachHandFromHandle(AvatarIKGoal.RightHand);
                }
            }
			

		}
		protected virtual void AttachHandToHandle(AvatarIKGoal hand, Transform handle)
		{
			_animator.SetIKPositionWeight(hand,1);
			_animator.SetIKRotationWeight(hand,1);  
			_animator.SetIKPosition(hand,handle.position);
			_animator.SetIKRotation(hand,handle.rotation);
		}
		protected virtual void DetachHandFromHandle(AvatarIKGoal hand)
		{
			_animator.SetIKPositionWeight(hand,0);
			_animator.SetIKRotationWeight(hand,0); 
			_animator.SetLookAtWeight(HeadWeights.x);
		}
		public virtual void SetHandles(Transform leftHand, Transform rightHand)
		{
			LeftHandTarget = leftHand;
			RightHandTarget = rightHand;
		}
	}
}
