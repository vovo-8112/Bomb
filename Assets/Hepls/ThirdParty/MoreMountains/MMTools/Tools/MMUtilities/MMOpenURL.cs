using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Utilities/MMOpenURL")]
    public class MMOpenURL : MonoBehaviour 
	{
		public string DestinationURL;
		public virtual void OpenURL()
		{
			Application.OpenURL(DestinationURL);
		}		
	}
}
