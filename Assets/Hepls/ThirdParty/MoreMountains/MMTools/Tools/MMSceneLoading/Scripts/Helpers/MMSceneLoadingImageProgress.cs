using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMSceneLoadingImageProgress : MonoBehaviour
    {
        protected Image _image;
        protected virtual void Awake()
        {
            _image = this.gameObject.GetComponent<Image>();
        }
        public virtual void SetProgress(float newValue)
        {
            _image.fillAmount = newValue;
        }
    }
}