using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace MoreMountains.TopDownEngine
{
    public struct TopDownCharacterAnimationParameter
    {
        public string ParameterName;
        public AnimatorControllerParameterType ParameterType;

        public TopDownCharacterAnimationParameter(string name, AnimatorControllerParameterType type)
        {
            ParameterName = name;
            ParameterType = type;
        }
    }
    public class CharacterAnimationParametersInitializer : MonoBehaviour
    {
        [Header("Initialization")]
        [Tooltip("if this is true, this component will remove itself after adding the character parameters")]
        public bool AutoRemoveAfterInitialization = true;
        [MMInspectorButton("AddAnimationParameters")]
        public bool AddAnimationParametersButton;

        protected TopDownCharacterAnimationParameter[] ParametersArray = new TopDownCharacterAnimationParameter[]
        {
            new TopDownCharacterAnimationParameter("Alive", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Grounded", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Idle", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Walking", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Running", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Activating", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Crouching", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Crawling", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Damage", AnimatorControllerParameterType.Trigger),
            new TopDownCharacterAnimationParameter("Dashing", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("DashingDirectionX", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("DashingDirectionY", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("DashingDirectionZ", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("DashStarted", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Death", AnimatorControllerParameterType.Trigger),
            new TopDownCharacterAnimationParameter("FallingDownHole", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("WeaponEquipped", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("WeaponEquippedID", AnimatorControllerParameterType.Int),
            new TopDownCharacterAnimationParameter("Jumping", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("HitTheGround", AnimatorControllerParameterType.Bool),
            new TopDownCharacterAnimationParameter("Random", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RandomConstant", AnimatorControllerParameterType.Int),
            new TopDownCharacterAnimationParameter("Direction", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("Speed", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("xSpeed", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("ySpeed", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("zSpeed", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("HorizontalDirection", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("VerticalDirection", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RelativeForwardSpeed", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RelativeLateralSpeed", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RelativeForwardSpeedNormalized", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RelativeLateralSpeedNormalized", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RemappedForwardSpeedNormalized", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RemappedLateralSpeedNormalized", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("RemappedSpeedNormalized", AnimatorControllerParameterType.Float),
            new TopDownCharacterAnimationParameter("YRotationSpeed", AnimatorControllerParameterType.Float),
        };

        protected Animator _animator;
#if UNITY_EDITOR
        protected AnimatorController _controller;
#endif
        protected List<string> _parameters = new List<string>();
        public virtual void AddAnimationParameters()
        {
            _animator = this.gameObject.GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("You need to add the AnimationParameterInitializer class to a gameobject with an Animator.");
            }
#if UNITY_EDITOR
            _controller = _animator.runtimeAnimatorController as AnimatorController;
            if (_controller == null)
            {
                Debug.LogError("You need an animator controller on this Animator.");
            }
#endif
            _parameters.Clear();
            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                _parameters.Add(param.name);
            }
            foreach (TopDownCharacterAnimationParameter parameter in ParametersArray)
            {
                if (!_parameters.Contains(parameter.ParameterName))
                {
#if UNITY_EDITOR
                    _controller.AddParameter(parameter.ParameterName, parameter.ParameterType);
#endif
                }
            }
            if (AutoRemoveAfterInitialization)
            {
                DestroyImmediate(this);
            }
        }
    }
}
