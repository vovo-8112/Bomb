using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnimationEvent
{
    public class WaitPanelAnim : MonoBehaviour
    {
        [SerializeField]
        private Animation m_Animation;

        [SerializeField]
        private Image m_Image;

        [SerializeField]
        private TMP_Text m_Text;

        public void SetUp()
        {
            m_Image.DOFade(1, 0.1f);
            m_Text.DOFade(1, 0.1f);
        }

        public void StartAnim()
        {
            m_Animation.Play();
        }

        //Use in animation
        public void DisablePanel()
        {
            gameObject.SetActive(false);
        }
    }
}