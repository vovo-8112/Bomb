using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/GUI/MMGetFocusOnEnable")]
    public class MMGetFocusOnEnable : MonoBehaviour
	{
		protected virtual void OnEnable()
		{
			EventSystem.current.SetSelectedGameObject(this.gameObject, null);
		}
	}
}