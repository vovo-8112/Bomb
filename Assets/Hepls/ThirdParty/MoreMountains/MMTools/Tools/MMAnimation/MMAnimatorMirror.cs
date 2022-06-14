using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMAnimatorMirror : MonoBehaviour
    {
        public struct MMAnimatorMirrorBind
        {
            public int ParameterHash;
            public AnimatorControllerParameterType ParameterType;
        }

        [Header("Bindings")]
        public Animator SourceAnimator;
        public Animator TargetAnimator;

        protected AnimatorControllerParameter[] _sourceParameters;
        protected AnimatorControllerParameter[] _targetParameters;
        protected List<MMAnimatorMirrorBind> _updateParameters;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (TargetAnimator == null)
            {
                TargetAnimator = this.gameObject.GetComponent<Animator>();
            }

            if ((TargetAnimator == null) || (SourceAnimator == null))
            {
                return;
            }
            int numberOfParameters = SourceAnimator.parameterCount;
            _sourceParameters = new AnimatorControllerParameter[numberOfParameters];
            for (int i = 0; i < numberOfParameters; i++)
            {
                _sourceParameters[i] = SourceAnimator.GetParameter(i);
            }
            numberOfParameters = TargetAnimator.parameterCount;
            _targetParameters = new AnimatorControllerParameter[numberOfParameters];
            for (int i = 0; i < numberOfParameters; i++)
            {
                _targetParameters[i] = TargetAnimator.GetParameter(i);
            }
            _updateParameters = new List<MMAnimatorMirrorBind>();

            foreach (AnimatorControllerParameter sourceParam in _sourceParameters)
            {
                foreach (AnimatorControllerParameter targetParam in _targetParameters)
                {
                    if (sourceParam.name == targetParam.name)
                    {
                        MMAnimatorMirrorBind bind = new MMAnimatorMirrorBind();
                        bind.ParameterHash = sourceParam.nameHash;
                        bind.ParameterType = sourceParam.type;
                        _updateParameters.Add(bind);
                    }
                }
            }
        }
        protected virtual void Update()
        {
            Mirror();
        }
        protected virtual void Mirror()
        {
            if ((TargetAnimator == null) || (SourceAnimator == null))
            {
                return;
            }

            foreach (MMAnimatorMirrorBind bind in _updateParameters)
            {
                switch (bind.ParameterType)
                {
                    case AnimatorControllerParameterType.Bool:
                        TargetAnimator.SetBool(bind.ParameterHash, SourceAnimator.GetBool(bind.ParameterHash));
                        break;
                    case AnimatorControllerParameterType.Float:
                        TargetAnimator.SetFloat(bind.ParameterHash, SourceAnimator.GetFloat(bind.ParameterHash));
                        break;
                    case AnimatorControllerParameterType.Int:
                        TargetAnimator.SetInteger(bind.ParameterHash, SourceAnimator.GetInteger(bind.ParameterHash));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (SourceAnimator.GetBool(bind.ParameterHash))
                        {
                            TargetAnimator.SetTrigger(bind.ParameterHash);
                        }
                        break;
                }
            }
        }
    }
}
