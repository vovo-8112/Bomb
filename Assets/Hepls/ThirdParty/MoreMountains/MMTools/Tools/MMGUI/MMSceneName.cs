using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMSceneName : MonoBehaviour
    {
        protected Text _text;
        protected virtual void Awake()
        {
            _text = this.gameObject.GetComponent<Text>();
        }
        protected virtual void Start()
        {
            SetLevelNameText();
        }
        public virtual void SetLevelNameText()
        {
            if (_text != null)
            {
                _text.text = SceneManager.GetActiveScene().name;
            }
        }
    }
}

