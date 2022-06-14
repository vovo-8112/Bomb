using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/VFX/PanningTexture")]
    public class MMPanningTexture : MonoBehaviour
    {
        [MMInformation("This script will let you pan a texture on an attached Renderer.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        public bool TextureShouldPan = true;
        public Vector2 Speed = new Vector2(10,10);
        public string SortingLayerName = "Above";
        [Tooltip("the property name, for example _MainTex")]
        public string MaterialPropertyName = "_MainTex_ST";
        [Tooltip("the index of the material")]
        public int MaterialIndex = 0;
        
        protected RawImage _rawImage;
        protected Renderer _renderer;
        protected Vector4 _position = Vector4.one;
        protected Vector4 _speed;
        protected MaterialPropertyBlock _propertyBlock;
        protected virtual void Start()
        {
            _renderer = GetComponent<Renderer>();
            if ((_renderer != null) && (!string.IsNullOrEmpty(SortingLayerName)))
            {
                _renderer.sortingLayerName = SortingLayerName;
                _propertyBlock = new MaterialPropertyBlock();
                _renderer.GetPropertyBlock(_propertyBlock);
            }            
            _position.x = _renderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
            _position.y = _renderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;
            _rawImage = GetComponent<RawImage>();

            _speed = new Vector4(0f, 0f, Speed.x, Speed.y);
        }
        protected virtual void Update()
        {
            if (!TextureShouldPan)
            {
                return;
            }
            
            if ((_rawImage == null) && (_renderer == null))
            {
                return;
            }

            _speed.z = Speed.x;
            _speed.w = Speed.y;
            _position += (_speed / 300) * Time.deltaTime;
            if (_position.z > 1.0f)
            {
                _position.z -= 1.0f;
            }
            if (_position.w > 1.0f)
            {
                _position.w -= 1.0f;
            }
            
            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetVector(MaterialPropertyName, _position);
                _renderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
            }
            if (_rawImage != null)
            {
                _rawImage.material.mainTextureOffset = _position;
            }

        }
    }
}