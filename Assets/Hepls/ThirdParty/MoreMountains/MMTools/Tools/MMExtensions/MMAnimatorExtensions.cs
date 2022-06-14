using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class MMAnimatorExtensions
    {
		public static bool MMHasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
        {
            if (string.IsNullOrEmpty(name)) { return false; }
            AnimatorControllerParameter[] parameters = self.parameters;
            foreach (AnimatorControllerParameter currParam in parameters)
            {
                if (currParam.type == type && currParam.name == name)
                {
                    return true;
                }
            }
            return false;
        }
        public static void AddAnimatorParameterIfExists(Animator animator, string parameterName, out int parameter, AnimatorControllerParameterType type, HashSet<int> parameterList)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                parameter = -1;
                return;
            }

            parameter = Animator.StringToHash(parameterName);

            if (animator.MMHasParameterOfType(parameterName, type))
            {
                parameterList.Add(parameter);
            }
        }
        public static void AddAnimatorParameterIfExists(Animator animator, string parameterName, AnimatorControllerParameterType type, HashSet<string> parameterList)
        {
            if (animator.MMHasParameterOfType(parameterName, type))
            {
                parameterList.Add(parameterName);
            }
        }

        #region SimpleMethods
        public static void UpdateAnimatorBool(Animator animator, string parameterName, bool value)
        {
            animator.SetBool(parameterName, value);
        }
        public static void UpdateAnimatorInteger(Animator animator, string parameterName, int value)
        {
            animator.SetInteger(parameterName, value);
        }
        public static void UpdateAnimatorFloat(Animator animator, string parameterName, float value, bool performSanityCheck = true)
        {
            animator.SetFloat(parameterName, value);
        }
        
        #endregion
        public static bool UpdateAnimatorBool(Animator animator, int parameter, bool value, HashSet<int> parameterList, bool performSanityCheck = true)
        {
            if (performSanityCheck && !parameterList.Contains(parameter))
            {
                return false;
            }
            animator.SetBool(parameter, value);
            return true;
        }
        public static bool UpdateAnimatorTrigger(Animator animator, int parameter, HashSet<int> parameterList, bool performSanityCheck = true)
        {
            if (performSanityCheck && !parameterList.Contains(parameter))
            {
                return false;
            }
            animator.SetTrigger(parameter);
            return true;
        }
        public static bool SetAnimatorTrigger(Animator animator, int parameter, HashSet<int> parameterList, bool performSanityCheck = true)
        {
            if (performSanityCheck && !parameterList.Contains(parameter))
            {
                return false;
            }
            animator.SetTrigger(parameter);
            return true;
        }
        public static bool UpdateAnimatorFloat(Animator animator, int parameter, float value, HashSet<int> parameterList, bool performSanityCheck = true)
        {
            if (performSanityCheck && !parameterList.Contains(parameter))
            {
                return false;
            }
            animator.SetFloat(parameter, value);
            return true;
        }
        public static bool UpdateAnimatorInteger(Animator animator, int parameter, int value, HashSet<int> parameterList, bool performSanityCheck = true)
        {
            if (performSanityCheck && !parameterList.Contains(parameter))
            {
                return false;
            }
            animator.SetInteger(parameter, value);
            return true;
        }

        #region StringParameterMethods
        public static void UpdateAnimatorBool(Animator animator, string parameterName, bool value, HashSet<string> parameterList, bool performSanityCheck = true)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }
        public static void UpdateAnimatorTrigger(Animator animator, string parameterName, HashSet<string> parameterList, bool performSanityCheck = true)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetTrigger(parameterName);
            }
        }
		public static void SetAnimatorTrigger(Animator animator, string parameterName, HashSet<string> parameterList, bool performSanityCheck = true)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetTrigger(parameterName);
            }
        }
		public static void UpdateAnimatorFloat(Animator animator, string parameterName, float value, HashSet<string> parameterList, bool performSanityCheck = true)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }
        public static void UpdateAnimatorInteger(Animator animator, string parameterName, int value, HashSet<string> parameterList, bool performSanityCheck = true)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetInteger(parameterName, value);
            }
        }
        public static void UpdateAnimatorBoolIfExists(Animator animator, string parameterName, bool value, bool performSanityCheck = true)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(parameterName, value);
            }
        }
        public static void UpdateAnimatorTriggerIfExists(Animator animator, string parameterName, bool performSanityCheck = true)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(parameterName);
            }
        }
        public static void SetAnimatorTriggerIfExists(Animator animator, string parameterName, bool performSanityCheck = true)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(parameterName);
            }
        }
        public static void UpdateAnimatorFloatIfExists(Animator animator, string parameterName, float value, bool performSanityCheck = true)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(parameterName, value);
            }
        }
        public static void UpdateAnimatorIntegerIfExists(Animator animator, string parameterName, int value, bool performSanityCheck = true)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Int))
            {
                animator.SetInteger(parameterName, value);
            }
        }

        #endregion
        
        
    }
}
