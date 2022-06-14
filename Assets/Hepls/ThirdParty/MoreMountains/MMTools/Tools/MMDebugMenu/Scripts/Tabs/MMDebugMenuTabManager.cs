using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuTabManager : MonoBehaviour
    {
        public List<MMDebugMenuTab> Tabs;
        public List<MMDebugMenuTabContents> TabsContents;
        public virtual void Select(int selected)
        {
            foreach(MMDebugMenuTab tab in Tabs)
            {
                if (tab.Index != selected)
                {
                    tab.Deselect();
                }
            }
            foreach(MMDebugMenuTabContents contents in TabsContents)
            {
                if (contents.Index == selected)
                {
                    contents.gameObject.SetActive(true);
                }
                else
                {
                    contents.gameObject.SetActive(false);
                }
            }
        }
    }
}
