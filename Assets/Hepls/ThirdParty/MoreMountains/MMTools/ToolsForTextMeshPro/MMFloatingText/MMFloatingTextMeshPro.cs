using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = System.Random;
using TMPro;

namespace MoreMountains.Tools
{
    public class MMFloatingTextMeshPro : MMFloatingText
    {
        [Header("TextMeshPro")]
        public TextMeshPro TargetTextMeshPro;
        protected override void Initialization()
        {
            base.Initialization();
            _initialTextColor = TargetTextMeshPro.color;
        }
        public override void SetText(string newValue)
        {
            TargetTextMeshPro.text = newValue;
        }
        public override void SetColor(Color newColor)
        {
            TargetTextMeshPro.color = newColor;
        }
        public override void SetOpacity(float newOpacity)
        {
            _newColor = TargetTextMeshPro.color;
            _newColor.a = newOpacity;
            TargetTextMeshPro.color = _newColor;
        }
    }
}
