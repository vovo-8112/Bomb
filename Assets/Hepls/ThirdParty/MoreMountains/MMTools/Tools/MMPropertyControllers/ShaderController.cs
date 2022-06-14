using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [MMRequiresConstantRepaint]
    [AddComponentMenu("More Mountains/Tools/Property Controllers/ShaderController")]
    public class ShaderController : MMMonoBehaviour
    {
        public enum TargetTypes { Renderer, Image, RawImage, Text }
        public enum PropertyTypes { Bool, Float, Int, Vector, Keyword, Color }
        public enum ControlModes { PingPong, Random, OneTime, AudioAnalyzer, ToDestination, Driven }

        [Header("Target")]
        public TargetTypes TargetType = TargetTypes.Renderer;
        [MMEnumCondition("TargetType",(int)TargetTypes.Renderer)]
        public Renderer TargetRenderer;
        [MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
        public int TargetMaterialID = 0;
        [MMEnumCondition("TargetType", (int)TargetTypes.Image)]
        public Image TargetImage;
        [MMEnumCondition("TargetType", (int)TargetTypes.Image)]
        public bool UseMaterialForRendering = false;
        [MMEnumCondition("TargetType", (int)TargetTypes.RawImage)]
        public RawImage TargetRawImage;
        [MMEnumCondition("TargetType", (int)TargetTypes.Text)]
        public Text TargetText;
        public bool CacheMaterial = true;
        public bool CreateMaterialInstance = false;
        public string TargetPropertyName;
        public PropertyTypes PropertyType = PropertyTypes.Float;
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool X;
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool Y;
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool Z;
        [MMEnumCondition("PropertyType", (int)PropertyTypes.Vector)]
        public bool W;

        [Header("Color")]
        [ColorUsage(true, true)]
        public Color FromColor = Color.black;
        [ColorUsage(true, true)]
        public Color ToColor = Color.white;

        [Header("Global Settings")]
        public ControlModes ControlMode;
        public bool AddToInitialValue = false;
        public bool UseUnscaledTime = true;
        public bool RevertToInitialValueAfterEnd = true;
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
        [MMEnumCondition("TargetType", (int)TargetTypes.Renderer)]
        public bool UseMaterialPropertyBlocks = false;
        public bool SafeMode = false;
        [Header("Ping Pong")]
        public MMTweenType Curve;
        public float MinValue = 0f;
        public float MaxValue = 5f;
        public float Duration = 1f;
        public float PingPongPauseDuration = 1f;

        [Header("Driven")]
        public float DrivenLevel = 0f;

        [Header("Random")]
        [MMVector("Min", "Max")]
        public Vector2 Amplitude = new Vector2(0f,5f);
        [MMVector("Min", "Max")]
        public Vector2 Frequency = new Vector2(1f, 1f);
        [MMVector("Min", "Max")]
        public Vector2 Shift = new Vector2(0f, 1f);
        public bool RemapNoiseValues = false;
        [MMCondition("RemapNoiseValues", true)]
        public float RemapNoiseZero = 0f;
        [MMCondition("RemapNoiseValues", true)]
        public float RemapNoiseOne = 1f;
        
        [Header("OneTime")]
        public float OneTimeDuration = 1f;
        public float OneTimeAmplitude = 1f;
        public float OneTimeRemapMin = 0f;
        public float OneTimeRemapMax = 1f;
        public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [MMInspectorButton("OneTime")]
        public bool OneTimeButton;
        public bool DisableAfterOneTime = false;
        public bool DisableGameObjectAfterOneTime = false;

        [Header("AudioAnalyzer")]
        public MMAudioAnalyzer AudioAnalyzer;
        public int BeatID;
        public float AudioAnalyzerMultiplier = 1f;
        public float AudioAnalyzerOffset = 0f;
        public float AudioAnalyzerLerp = 60f;

        [Header("ToDestination")]
        public float ToDestinationValue = 1f;
        public float ToDestinationDuration = 1f;
        public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.6f), new Keyframe(1f, 1f));
        [MMInspectorButton("ToDestination")]
        public bool ToDestinationButton;
        public bool DisableAfterToDestination = false;

        [Header("Debug")]
        [MMReadOnly]
        public float InitialValue;
        [MMReadOnly]
        public float CurrentValue;
        [MMReadOnly]
        public float CurrentValueNormalized = 0f;
        [MMReadOnly]
        public Color InitialColor;

        [MMReadOnly]
        public int PropertyID;
        [MMReadOnly]
        public bool PropertyFound = false;
        [MMReadOnly]
        public Material TargetMaterial;
        [HideInInspector]
        public float PingPong;
        
        protected float _randomAmplitude;
        protected float _randomFrequency;
        protected float _randomShift;
        protected float _elapsedTime = 0f;

        protected bool _shaking = false;
        protected float _startedTimestamp = 0f;
        protected float _remappedTimeSinceStart = 0f;
        protected Color _currentColor;

        protected Vector4 _vectorValue;

        protected float _pingPongDirection = 1f;
        protected float _lastPingPongPauseAt = 0f;
        protected float _initialValue = 0f;
        protected Color _fromColorStorage;
        protected bool _activeLastFrame = false;
        protected MaterialPropertyBlock _propertyBlock;
        public virtual bool FindShaderProperty(string propertyName)
        {
            if (TargetType == TargetTypes.Renderer)
            {
                if (CreateMaterialInstance)
                {
                    TargetRenderer.materials[TargetMaterialID] = new Material(TargetRenderer.materials[TargetMaterialID]);
                }
                TargetMaterial = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterials[TargetMaterialID] : TargetRenderer.materials[TargetMaterialID];
            }
            else if (TargetType == TargetTypes.Image)
            {
                if (CreateMaterialInstance)
                {
                    TargetImage.material = new Material(TargetImage.material);
                }
                TargetMaterial = TargetImage.material;
            }
            else if (TargetType == TargetTypes.RawImage)
            {
                if (CreateMaterialInstance)
                {
                    TargetRawImage.material = new Material(TargetRawImage.material);
                }
                TargetMaterial = TargetRawImage.material;
            }
            else if (TargetType == TargetTypes.Text)
            {
                if (CreateMaterialInstance)
                {
                    TargetText.material = new Material(TargetText.material);
                }
                TargetMaterial = TargetText.material;
            }

            if (PropertyType == PropertyTypes.Keyword)
            {
                PropertyFound = true;
                return true;
            }
            if (TargetMaterial.HasProperty(propertyName))
            {                
                PropertyID = Shader.PropertyToID(propertyName);
                PropertyFound = true;
                return true;
            }
            return false;
        }
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void OnEnable()
        {
            InitialValue = GetInitialValue();
            if (PropertyType == PropertyTypes.Color)
            {
                InitialColor = TargetMaterial.GetColor(PropertyID);
            }
        }
        protected virtual bool RendererIsNull()
        {
            if ((TargetType == TargetTypes.Renderer) && (TargetRenderer == null))
            {
                return true;
            }
            if ((TargetType == TargetTypes.Image) && (TargetImage == null))
            {
                return true;
            }
            if ((TargetType == TargetTypes.RawImage) && (TargetRawImage == null))
            {
                return true;
            }
            if ((TargetType == TargetTypes.Text) && (TargetText == null))
            {
                return true;
            }
            return false;
        }
        public virtual void Initialization()
        {
            if (RendererIsNull() || (string.IsNullOrEmpty(TargetPropertyName)))
            {
                return;
            }
            if (TargetType != TargetTypes.Renderer)
            {
                UseMaterialPropertyBlocks = false;
            }
            
            PropertyFound = FindShaderProperty(TargetPropertyName);
            if (!PropertyFound)
            {
                return;
            }

            _elapsedTime = 0f;
            _randomAmplitude = Random.Range(Amplitude.x, Amplitude.y);
            _randomFrequency = Random.Range(Frequency.x, Frequency.y);
            _randomShift = Random.Range(Shift.x, Shift.y);
            
            if ((TargetType == TargetTypes.Renderer) && UseMaterialPropertyBlocks)
            {
                _propertyBlock = new MaterialPropertyBlock();
                TargetRenderer.GetPropertyBlock(_propertyBlock, TargetMaterialID);
            }

            InitialValue = GetInitialValue();
            if (PropertyType == PropertyTypes.Color)
            {
                InitialColor = TargetMaterial.GetColor(PropertyID);
            }
                
            _shaking = false;
            if (ControlMode == ControlModes.OneTime)
            {
                this.enabled = false;
            }
        }
        public virtual void SetDrivenLevelAbsolute(float level)
        {
            DrivenLevel = level;
        }
        public virtual void SetDrivenLevelNormalized(float normalizedLevel, float remapZero, float remapOne)
        {
            DrivenLevel = MMMaths.Remap(normalizedLevel, 0f, 1f, remapZero, remapOne);
        }
        public virtual void OneTime()
        {
            if (!CacheMaterial)
            {
                Initialization();
            }

            if (RendererIsNull() || (!PropertyFound))
            {
                return;
            }
            else
            {
                this.gameObject.SetActive(true);
                this.enabled = true;
                ControlMode = ControlModes.OneTime;
                _startedTimestamp = GetTime();
                _shaking = true;
            }
        }
        public virtual void ToDestination()
        {
            if (!CacheMaterial)
            {
                Initialization();
            }
            if (RendererIsNull() || (!PropertyFound))
            {
                return;
            }
            else
            {
                this.enabled = true;
                if (PropertyType == PropertyTypes.Color)
                {
                    _fromColorStorage = FromColor;
                    FromColor = TargetMaterial.GetColor(PropertyID);
                }                
                ControlMode = ControlModes.ToDestination;
                _startedTimestamp = GetTime();
                _shaking = true;
                _initialValue = GetInitialValue();
            }
        }
        protected float GetDeltaTime()
        {
            return UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
        protected float GetTime()
        {
            return UseUnscaledTime ? Time.unscaledTime : Time.time;
        }
        protected virtual void Update()
        {
            UpdateValue();
        }

        protected virtual void OnDisable()
        {
            if (RevertToInitialValueAfterEnd)
            {
                CurrentValue = InitialValue;
                _currentColor = InitialColor;
                SetValue(CurrentValue);
            }
        }
        protected virtual void UpdateValue()
        {
            if (SafeMode)
            {
                if (RendererIsNull() || (!PropertyFound))
                {
                    return;
                }    
            }

            switch (ControlMode)
            {
                case ControlModes.PingPong:
                    if (GetTime() - _lastPingPongPauseAt < PingPongPauseDuration)
                    {
                        return;
                    }
                    PingPong += GetDeltaTime() * _pingPongDirection;
                    if (PingPong < 0f)
                    {
                        PingPong = 0f;
                        _pingPongDirection = -_pingPongDirection;
                        _lastPingPongPauseAt = GetTime();
                    }

                    if (PingPong > Duration)
                    {
                        PingPong = Duration;
                        _pingPongDirection = -_pingPongDirection;
                        _lastPingPongPauseAt = GetTime();
                    }
                    CurrentValue = MMTween.Tween(PingPong, 0f, Duration, MinValue, MaxValue, Curve);
                    CurrentValueNormalized = MMMaths.Remap(CurrentValue, MinValue, MaxValue, 0f, 1f);
                    break;
                case ControlModes.Random:
                    _elapsedTime += GetDeltaTime();
                    CurrentValueNormalized = Mathf.PerlinNoise(_randomFrequency * _elapsedTime, _randomShift); 
                    if (RemapNoiseValues)
                    {
                        CurrentValue = CurrentValueNormalized;
                        CurrentValue = MMMaths.Remap(CurrentValue, 0f, 1f, RemapNoiseZero, RemapNoiseOne);
                    }
                    else
                    {
                        CurrentValue = (CurrentValueNormalized * 2.0f - 1.0f) * _randomAmplitude;
                    }
                    break;
                case ControlModes.OneTime:
                    if (!_shaking)
                    {
                        return;
                    }
                    _remappedTimeSinceStart = MMMaths.Remap(GetTime() - _startedTimestamp, 0f, OneTimeDuration, 0f, 1f);
                    CurrentValueNormalized = OneTimeCurve.Evaluate(_remappedTimeSinceStart);
                    CurrentValue = MMMaths.Remap(CurrentValueNormalized, 0f, 1f, OneTimeRemapMin, OneTimeRemapMax);
                    CurrentValue *= OneTimeAmplitude;
                    break;
                case ControlModes.AudioAnalyzer:
                    CurrentValue = Mathf.Lerp(CurrentValue, AudioAnalyzer.Beats[BeatID].CurrentValue * AudioAnalyzerMultiplier + AudioAnalyzerOffset, AudioAnalyzerLerp * GetDeltaTime());
                    CurrentValueNormalized = Mathf.Clamp(AudioAnalyzer.Beats[BeatID].CurrentValue, 0f, 1f);
                    break;
                case ControlModes.Driven:
                    CurrentValue = DrivenLevel;
                    CurrentValueNormalized = Mathf.Clamp(CurrentValue, 0f, 1f);
                    break;
                case ControlModes.ToDestination:
                    if (!_shaking)
                    {
                        return;
                    }
                    _remappedTimeSinceStart = MMMaths.Remap(GetTime() - _startedTimestamp, 0f, ToDestinationDuration, 0f, 1f);
                    float time = ToDestinationCurve.Evaluate(_remappedTimeSinceStart);
                    CurrentValue = Mathf.LerpUnclamped(_initialValue, ToDestinationValue, time);
                    CurrentValueNormalized = MMMaths.Remap(CurrentValue, _initialValue, ToDestinationValue, 0f, 1f);
                    break;
            }

            if (PropertyType == PropertyTypes.Color)
            {
                _currentColor = Color.Lerp(FromColor, ToColor, CurrentValue);
            }

            if (AddToInitialValue)
            {
                CurrentValue += InitialValue;
            }

            if ((ControlMode == ControlModes.OneTime) && _shaking && (GetTime() - _startedTimestamp > OneTimeDuration))
            {
                _shaking = false;
                if (RevertToInitialValueAfterEnd)
                {
                    CurrentValue = InitialValue;
                    if (PropertyType == PropertyTypes.Color)
                    {
                        _currentColor = InitialColor;
                    }
                }
                else
                {
                    CurrentValue = OneTimeCurve.Evaluate(1f);
                    CurrentValue = MMMaths.Remap(CurrentValue, 0f, 1f, OneTimeRemapMin, OneTimeRemapMax);
                    CurrentValue *= OneTimeAmplitude;
                }
                SetValue(CurrentValue);
                if (DisableAfterOneTime)
                {
                    this.enabled = false;
                }     
                if (DisableGameObjectAfterOneTime)
                {
                    this.gameObject.SetActive(false);
                }
                return;
            }

            if ((ControlMode == ControlModes.ToDestination) && _shaking && (GetTime() - _startedTimestamp > ToDestinationDuration))
            {
                _shaking = false;
                FromColor = _fromColorStorage;
                if (RevertToInitialValueAfterEnd)
                {
                    CurrentValue = InitialValue;
                    if (PropertyType == PropertyTypes.Color)
                    {
                        _currentColor = InitialColor;
                    }
                }
                else
                {
                    CurrentValue = ToDestinationValue;
                }
                SetValue(CurrentValue);
                if (DisableAfterToDestination)
                {
                    this.enabled = false;
                }
                return;
            }

            SetValue(CurrentValue);
        }
        protected virtual float GetInitialValue()
        {
            if (TargetMaterial == null)
            {
                Debug.LogWarning("Material is null", this);
                return 0f;
            }

            switch (PropertyType)
            {
                case PropertyTypes.Bool:
                    return TargetMaterial.GetInt(PropertyID);                    

                case PropertyTypes.Int:
                    return TargetMaterial.GetInt(PropertyID);

                case PropertyTypes.Float:
                    return TargetMaterial.GetFloat(PropertyID);

                case PropertyTypes.Vector:
                    return TargetMaterial.GetVector(PropertyID).x;                    

                case PropertyTypes.Keyword:
                    return TargetMaterial.IsKeywordEnabled(TargetPropertyName) ? 1f : 0f;

                case PropertyTypes.Color:
                    if (ControlMode != ControlModes.ToDestination)
                    {
                        InitialColor = TargetMaterial.GetColor(PropertyID);
                    }                    
                    return 0f;

                default:
                    return 0f;
            }
        }
        protected virtual void SetValue(float newValue)
        {
            if (TargetType == TargetTypes.Image && UseMaterialForRendering)
            {
                if (SafeMode)
                {
                    if (TargetImage == null)
                    {
                        return;    
                    }
                }
                TargetMaterial = TargetImage.materialForRendering;
            }

            switch (PropertyType)
            {
                case PropertyTypes.Bool:
                    newValue = (newValue > 0f) ? 1f : 0f;
                    int newBool = Mathf.RoundToInt(newValue);
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetInt(PropertyID, newBool);
                        TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
                    }
                    else
                    {
                        TargetMaterial.SetInt(PropertyID, newBool);    
                    }
                    break;

                case PropertyTypes.Keyword:
                    newValue = (newValue > 0f) ? 1f : 0f;
                    if (newValue == 0f)
                    {
                        TargetMaterial.DisableKeyword(TargetPropertyName);
                    }
                    else
                    {
                        TargetMaterial.EnableKeyword(TargetPropertyName);
                    }
                    break;

                case PropertyTypes.Int:
                    int newInt = Mathf.RoundToInt(newValue);
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetInt(PropertyID, newInt);
                        TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
                    }
                    else
                    {
                        TargetMaterial.SetInt(PropertyID, newInt);    
                    }
                    break;

                case PropertyTypes.Float:
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetFloat(PropertyID, newValue);
                        TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
                    }
                    else
                    {
                        TargetMaterial.SetFloat(PropertyID, newValue);
                    }
                    break;

                case PropertyTypes.Vector:
                    _vectorValue = TargetMaterial.GetVector(PropertyID);
                    if (X)
                    {
                        _vectorValue.x = newValue;
                    }
                    if (Y)
                    {
                        _vectorValue.y = newValue;
                    }
                    if (Z)
                    {
                        _vectorValue.z = newValue;
                    }
                    if (W)
                    {
                        _vectorValue.w = newValue;
                    }
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetVector(PropertyID, _vectorValue);
                        TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
                    }
                    else
                    {
                        TargetMaterial.SetVector(PropertyID, _vectorValue);
                    }
                    break;
                    
                case PropertyTypes.Color:
                    if (UseMaterialPropertyBlocks)
                    {
                        TargetRenderer.GetPropertyBlock(_propertyBlock);
                        _propertyBlock.SetColor(PropertyID, _currentColor);
                        TargetRenderer.SetPropertyBlock(_propertyBlock, TargetMaterialID);    
                    }
                    else
                    {
                        TargetMaterial.SetColor(PropertyID, _currentColor);
                    }
                    break;
            }
        }
        public virtual void Stop()
        {
            _shaking = false;
            this.enabled = false;
        }
    }
}

