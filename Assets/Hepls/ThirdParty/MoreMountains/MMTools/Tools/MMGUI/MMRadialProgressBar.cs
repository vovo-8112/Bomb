using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/GUI/MMRadialProgressBar")]
    public class MMRadialProgressBar : MonoBehaviour 
	{
		public float StartValue = 1f;
		public float EndValue = 0f;
		public float Tolerance = 0.01f;
        public string PlayerID;

        protected Image _radialImage;
		protected float _newPercent;
		protected virtual void Awake()
		{
			_radialImage = GetComponent<Image>();
		}
		public virtual void UpdateBar(float currentValue,float minValue,float maxValue)
		{
			_newPercent = MMMaths.Remap(currentValue,minValue,maxValue,StartValue,EndValue);
			if (_radialImage == null) { return; }
			_radialImage.fillAmount = _newPercent;
			if (_radialImage.fillAmount > 1 - Tolerance)
			{
				_radialImage.fillAmount = 1;
			}
			if (_radialImage.fillAmount < Tolerance)
			{
				_radialImage.fillAmount = 0;
			}

		}
	}
}
