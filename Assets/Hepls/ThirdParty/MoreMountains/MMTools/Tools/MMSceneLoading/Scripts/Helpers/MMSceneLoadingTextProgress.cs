using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMSceneLoadingTextProgress : MonoBehaviour
    {
        [Tooltip("the value to which the progress' zero value should be remapped to")]
        public float RemapMin = 0f;
        [Tooltip("the value to which the progress' one value should be remapped to")]
        public float RemapMax = 100f;
        [Tooltip("the amount of decimals to display")]
        public int NumberOfDecimals = 0;

        protected Text _text;
        protected virtual void Awake()
        {
            _text = this.gameObject.GetComponent<Text>();
        }
        public virtual void SetProgress(float newValue)
        {
            float remappedValue = MMMaths.Remap(newValue, 0f, 1f, RemapMin, RemapMax);
            float displayValue = MMMaths.RoundToDecimal(remappedValue, NumberOfDecimals);
            _text.text = displayValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}
