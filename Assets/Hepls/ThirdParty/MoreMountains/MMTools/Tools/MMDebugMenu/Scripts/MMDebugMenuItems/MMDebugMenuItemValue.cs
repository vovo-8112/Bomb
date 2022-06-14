using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugMenuItemValue : MonoBehaviour
    {
        [Header("Bindings")]
        public Text LabelText;
        public Text ValueText;
        public MMRadioReceiver RadioReceiver;
        public float Level { get { return _level;  } set { _level = value;  ValueText.text = value.ToString("F2"); } }

        protected float _level;
    }
}
