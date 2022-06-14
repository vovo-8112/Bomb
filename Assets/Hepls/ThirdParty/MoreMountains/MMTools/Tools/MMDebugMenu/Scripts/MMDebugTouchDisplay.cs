using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMDebugTouchDisplay : MonoBehaviour
    {
        [Header("Bindings")]
        public Canvas TargetCanvas;

        [Header("Touches")]
        public RectTransform TouchPrefab;
        public int TouchProvision = 6;

        protected List<RectTransform> _touchDisplays;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _touchDisplays = new List<RectTransform>();

            for (int i = 0; i < TouchProvision; i++)
            {
                RectTransform touchDisplay = Instantiate(TouchPrefab);
                touchDisplay.transform.SetParent(TargetCanvas.transform);
                touchDisplay.name = "MMDebugTouchDisplay_" + i;
                touchDisplay.gameObject.SetActive(false);
                _touchDisplays.Add(touchDisplay);
            }

            this.enabled = false;
        }
        protected virtual void Update()
        {
            DisableAllDisplays();
            DetectTouches();
        }
        protected virtual void DetectTouches()
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                _touchDisplays[i].gameObject.SetActive(true);
                _touchDisplays[i].position = Input.GetTouch(i).position;
            }
        }
        protected virtual void DisableAllDisplays()
        {
            foreach(RectTransform display in _touchDisplays)
            {
                display.gameObject.SetActive(false);
            }
        }
        protected virtual void OnDisable()
        {
            DisableAllDisplays();
        }
    }
}
