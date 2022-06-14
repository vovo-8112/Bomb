using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MoreMountains.Tools
{
    [Serializable]    
    public class Ratio
    {
        public bool DrawRatio = true;
        public Vector2 Size;
        public Color RatioColor;

        public Ratio(bool drawRatio, Vector2 size, Color ratioColor)
        {
            DrawRatio = drawRatio;
            Size = size;
            RatioColor = ratioColor;
        }
    }
    [AddComponentMenu("More Mountains/Tools/Camera/MMAspectRatioSafeZones")]
    public class MMAspectRatioSafeZones : MonoBehaviour
    {
        [Header("Center")]
        public bool DrawCenterCrosshair = true;
        public float CenterCrosshairSize = 1f;
        public Color CenterCrosshairColor = MMColors.Wheat;

        [Header("Ratios")]
        public bool DrawRatios = true;
        public float CameraSize = 5f;
        public float UnsafeZonesOpacity = 0.2f;
        public List<Ratio> Ratios;

        [MMInspectorButton("AutoSetup")]
        public bool AutoSetupButton;

        public virtual void AutoSetup()
        {
            Ratios.Clear();
            Ratios.Add(new Ratio(true, new Vector2(16, 9), MMColors.DeepSkyBlue));
            Ratios.Add(new Ratio(true, new Vector2(16, 10), MMColors.GreenYellow));
            Ratios.Add(new Ratio(true, new Vector2(4, 3), MMColors.HotPink));
        }
    }
}
