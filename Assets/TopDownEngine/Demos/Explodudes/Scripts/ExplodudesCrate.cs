using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class that modifies the y scale of crates in the Explodudes demo scene
    /// </summary>
    public class ExplodudesCrate : MonoBehaviour
    {
        protected const float MinHeight = 0.8f;
        protected const float MaxHeight = 1.1f;
        protected Vector3 _newScale = Vector3.one;

        public bool IsEnable = true;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            IsEnable = active;
        }

        private void OnDisable()
        {
            IsEnable = false;
        }

        /// <summary>
        /// On Start we randomize our y scale for aesthetic considerations only
        /// </summary>
        protected virtual void Start()
        {
            _newScale.y = Random.Range(MinHeight, MaxHeight);
            transform.localScale = _newScale;
        }
    }
}