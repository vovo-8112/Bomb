using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuTab : MonoBehaviour
    {
        public Text TabText;
        public Image TabBackground;
        public Color SelectedBackgroundColor;
        public Color DeselectedBackgroundColor;
        public Color SelectedTextColor;
        public Color DeselectedTextColor;
        public int Index;
        public MMDebugMenuTabManager Manager;
        public bool ForceScaleOne = true;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (ForceScaleOne)
            {
                this.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
        public virtual void Select()
        {
            Manager.Select(Index);
            TabText.color = SelectedTextColor;
            TabBackground.color = SelectedBackgroundColor;
        }
        public virtual void Deselect()
        {
            TabText.color = DeselectedTextColor;
            TabBackground.color = DeselectedBackgroundColor;
        }
    }
}
