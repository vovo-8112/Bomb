using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Performance/MMFPSUnlock")]
    public class MMFPSUnlock : MonoBehaviour
    {
        [MMInformation("Add this component to any object and it'll set the target frame rate and vsync count. Note that vsync count must be 0 for the target FPS to work.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public int TargetFPS;
        [Range(0,2)]
        public int VSyncCount = 0;
		protected virtual void Start()
		{
            UpdateSettings();
		}
        protected virtual void OnValidate()
        {
            UpdateSettings();
        }
        protected virtual void UpdateSettings()
        {
            QualitySettings.vSyncCount = VSyncCount;
            Application.targetFrameRate = TargetFPS;
        }
	}
}
