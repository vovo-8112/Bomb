using UnityEngine;

namespace AnimationEvent
{
    public class WaitPanelAnim : MonoBehaviour
    {
        [SerializeField]
        private Animation m_Animation;

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