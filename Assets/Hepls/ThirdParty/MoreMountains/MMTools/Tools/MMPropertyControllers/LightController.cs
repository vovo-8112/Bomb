using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Property Controllers/LightController")]
    public class LightController : MonoBehaviour
    {
        [Header("Binding")]
        [MMInformation("Use this component to control the properties of one or more lights at runtime. Plays well with a FloatController. " +
            "This component will try to auto set the TargetLight if there's a Light component on this object.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public Light TargetLight;
        public List<Light> TargetLights;

        [Header("Light Settings")]
        public float Intensity = 1f;
        public float Multiplier = 1f;
        public float Range = 1f;

        [Header("Color")]
        public Color LightColor;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (TargetLight == null)
            {
                TargetLight = this.gameObject.GetComponent<Light>();
            }

            if (TargetLight != null)
            {
                TargetLight.range = Range;
                TargetLight.color = LightColor;
            }

            if (TargetLights.Count > 0)
            {
                foreach (Light light in TargetLights)
                {
                    if (light != null)
                    {
                        light.range = Range;
                        light.color = LightColor;
                    }
                }
            }
        }
        protected virtual void Update()
        {
            ApplyLightSettings();           
        }
        protected virtual void ApplyLightSettings()
        {
            if (TargetLight != null)
            {
                TargetLight.intensity = Intensity * Multiplier;
            }

            if (TargetLights.Count > 0)
            {
                foreach (Light light in TargetLights)
                {
                    if (light != null)
                    {
                        light.intensity = Intensity * Multiplier;
                    }
                }
            }
        }
    }
}
