using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Weapons/Bomb")]
    public class Bomb : MonoBehaviour 
	{
		public enum DamageAreaShapes { Rectangle, Circle }

		[Header("Explosion")]
		[Tooltip("the delay before the bomb explodes")]
		public float TimeBeforeExplosion = 2f;
		[Tooltip("a vfx to instantiate when the bomb explodes")]
		public GameObject ExplosionEffect;
		[Tooltip("a sound to play when the bomb explodes")]
		public AudioClip ExplosionSfx;

		[Header("Flicker")]
		[Tooltip("whether or not the sprite should flicker before explosion")]
		public bool FlickerSprite = true;
		[Tooltip("the duration before the flicker starts")]
		public float TimeBeforeFlicker = 1f;
		[Tooltip("the name of the property that should flicker")]
		public string MaterialPropertyName = "_Color";

		[Header("Damage Area")]
		[Tooltip("the collider of the damage area")]
		public Collider2D DamageAreaCollider;
		[Tooltip("the duration of the damage area")]
		public float DamageAreaActiveDuration = 1f;

		protected float _timeSinceStart;
		protected Renderer _renderer;
		protected MMPoolableObject _poolableObject;
		protected bool _flickering;
		protected bool _damageAreaActive;
		protected Color _initialColor;
		protected Color _flickerColor = new Color32(255, 20, 20, 255);
		protected MaterialPropertyBlock _propertyBlock;
		protected virtual void OnEnable()
		{
			Initialization ();
		}
		protected virtual void Initialization()
		{
			if (DamageAreaCollider == null)
			{
				Debug.LogWarning ("There's no damage area associated to this bomb : " + this.name + ". You should set one via its inspector.");
				return;
			}
			DamageAreaCollider.isTrigger = true;
			DisableDamageArea ();

			_propertyBlock = new MaterialPropertyBlock();
			_renderer = gameObject.MMGetComponentNoAlloc<Renderer> ();
			if (_renderer != null)
			{
				if (_renderer.sharedMaterial.HasProperty(MaterialPropertyName))
				{
					_initialColor = _renderer.sharedMaterial.GetColor(MaterialPropertyName);    
				}
			}

			_poolableObject = gameObject.MMGetComponentNoAlloc<MMPoolableObject> ();
			if (_poolableObject != null)
			{
				_poolableObject.LifeTime = 0;
			}

			_timeSinceStart = 0;
			_flickering = false;
			_damageAreaActive = false;
		}
		protected virtual void Update()
		{
			_timeSinceStart += Time.deltaTime;
			if (_timeSinceStart >= TimeBeforeFlicker)
			{
				if (!_flickering && FlickerSprite)
				{
					if (_renderer != null)
					{
						StartCoroutine(MMImage.Flicker(_renderer,_initialColor,_flickerColor,0.05f,(TimeBeforeExplosion - TimeBeforeFlicker)));	
					}
				}
			}
			if (_timeSinceStart >= TimeBeforeExplosion && !_damageAreaActive)
			{
				EnableDamageArea ();
				_renderer.enabled = false;
				InstantiateExplosionEffect ();
				PlayExplosionSound ();
				_damageAreaActive = true;
			}

			if (_timeSinceStart >= TimeBeforeExplosion + DamageAreaActiveDuration)
			{
				DestroyBomb ();
			}
		}
		protected virtual void DestroyBomb()
		{
			_renderer.enabled = true;
			_renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor(MaterialPropertyName, _initialColor);
			_renderer.SetPropertyBlock(_propertyBlock);
			if (_poolableObject != null)
			{
				_poolableObject.Destroy ();	
			}
			else
			{
				Destroy (gameObject);
			}

		}
		protected virtual void InstantiateExplosionEffect()
		{
			if (ExplosionEffect!=null)
			{
				GameObject instantiatedEffect=(GameObject)Instantiate(ExplosionEffect,transform.position,transform.rotation);
				instantiatedEffect.transform.localScale = transform.localScale;
			}
		}
		protected virtual void PlayExplosionSound()
		{
			if (ExplosionSfx!=null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(ExplosionSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}
		protected virtual void EnableDamageArea()
		{
			DamageAreaCollider.enabled = true;
		}
		protected virtual void DisableDamageArea()
		{
			DamageAreaCollider.enabled = false;
		}
	}
}