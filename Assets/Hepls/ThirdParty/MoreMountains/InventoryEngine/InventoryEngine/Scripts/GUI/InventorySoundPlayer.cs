using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(InventoryDisplay))]
	public class InventorySoundPlayer : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		public enum Modes { Direct, Event }

		[Header("Settings")]
		public Modes Mode = Modes.Direct;
		
		[Header("Sounds")]
		[MMInformation("Here you can define the default sounds that will get played when interacting with this inventory.",MMInformationAttribute.InformationType.Info,false)]
		public AudioClip OpenFx;
		public AudioClip CloseFx;
		public AudioClip SelectionChangeFx;
		public AudioClip ClickFX;
		public AudioClip MoveFX;
		public AudioClip ErrorFx;
		public AudioClip UseFx;
		public AudioClip DropFx;
		public AudioClip EquipFx;

		protected string _targetInventoryName;
		protected AudioSource _audioSource;
		protected virtual void Start()
		{
			SetupInventorySoundPlayer ();
			_audioSource = GetComponent<AudioSource> ();
			_targetInventoryName = this.gameObject.MMGetComponentNoAlloc<InventoryDisplay> ().TargetInventoryName;
		}
		public virtual void SetupInventorySoundPlayer()
		{
			AddAudioSource ();			
		}
		protected virtual void AddAudioSource()
		{
			if (GetComponent<AudioSource>() == null)
			{
				this.gameObject.AddComponent<AudioSource>();
			}
		}
		public virtual void PlaySound(string soundFx)
		{
			if (soundFx==null || soundFx=="")
			{
				return;
			}

			AudioClip soundToPlay=null;
			float volume=1f;

			switch (soundFx)
			{
				case "error":
					soundToPlay=ErrorFx;
					volume=1f;
					break;
				case "select":
					soundToPlay=SelectionChangeFx;
					volume=0.5f;
					break;
				case "click":
					soundToPlay=ClickFX;
					volume=0.5f;
					break;
				case "open":
					soundToPlay=OpenFx;
					volume=1f;
					break;
				case "close":
					soundToPlay=CloseFx;
					volume=1f;
					break;
				case "move":
					soundToPlay=MoveFX;
					volume=1f;
					break;
				case "use":
					soundToPlay=UseFx;
					volume=1f;
					break;
				case "drop":
					soundToPlay=DropFx;
					volume=1f;
					break;
				case "equip":
					soundToPlay=EquipFx;
					volume=1f;
					break;
			}

			if (soundToPlay!=null)
			{
				if (Mode == Modes.Direct)
				{
					_audioSource.PlayOneShot(soundToPlay,volume);	
				}
				else
				{
					MMSfxEvent.Trigger(soundToPlay, null, volume, 1);	
				}
			}
		}
		public virtual void PlaySound(AudioClip soundFx,float volume)
		{
			if (soundFx != null)
			{
				if (Mode == Modes.Direct)
				{
					_audioSource.PlayOneShot(soundFx, volume);
				}
				else
				{
					MMSfxEvent.Trigger(soundFx, null, volume, 1);
				}
			}
		}
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.TargetInventoryName != _targetInventoryName)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					this.PlaySound("select");
					break;
				case MMInventoryEventType.Click:
					this.PlaySound("click");
					break;
				case MMInventoryEventType.InventoryOpens:
					this.PlaySound("open");
					break;
				case MMInventoryEventType.InventoryCloses:
					this.PlaySound("close");
					break;
				case MMInventoryEventType.Error:
					this.PlaySound("error");
					break;
				case MMInventoryEventType.Move:
					if (inventoryEvent.EventItem.MovedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("move"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.MovedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemEquipped:
					if (inventoryEvent.EventItem.EquippedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("equip"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.EquippedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemUsed:
					if (inventoryEvent.EventItem.UsedSound == null)
					{
								if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("use"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.UsedSound, 1f);
					}
					break;
				case MMInventoryEventType.Drop:
					if (inventoryEvent.EventItem.DroppedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("drop"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.DroppedSound, 1f);
					}
					break;
			}
		}
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}
