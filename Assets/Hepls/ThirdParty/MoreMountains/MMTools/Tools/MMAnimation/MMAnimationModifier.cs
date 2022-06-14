using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Animation/MMAnimationModifier")]
    public class MMAnimationModifier : StateMachineBehaviour
    {
        [MMVectorAttribute("Min", "Max")]
        public Vector2 StartPosition = new Vector2(0, 0);

        [MMVectorAttribute("Min", "Max")]
        public Vector2 AnimationSpeed = new Vector2(1, 1);

        protected bool _enteredState = false;
        protected float _initialSpeed;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _initialSpeed = animator.speed;
            animator.speed = Random.Range(AnimationSpeed.x, AnimationSpeed.y);
            if (!_enteredState)
            {
                animator.Play(stateInfo.fullPathHash, layerIndex, Random.Range(StartPosition.x, StartPosition.y));
            }
            _enteredState = !_enteredState;
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            animator.speed = _initialSpeed;            
        }
    }
}
