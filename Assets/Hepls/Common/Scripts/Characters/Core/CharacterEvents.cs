using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	public enum MMCharacterEventTypes
	{
		ButtonActivation,
		Jump
	}
	public struct MMCharacterEvent
	{
		public Character TargetCharacter;
		public MMCharacterEventTypes EventType;
		public MMCharacterEvent(Character character, MMCharacterEventTypes eventType)
		{
			TargetCharacter = character;
			EventType = eventType;
		}

        static MMCharacterEvent e;
        public static void Trigger(Character character, MMCharacterEventTypes eventType)
        {
            e.TargetCharacter = character;
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
	public struct MMDamageTakenEvent
	{
		public Character AffectedCharacter;
		public GameObject Instigator;
		public float CurrentHealth;
		public float DamageCaused;
		public float PreviousHealth;
		public MMDamageTakenEvent(Character affectedCharacter, GameObject instigator, float currentHealth, float damageCaused, float previousHealth)
		{
			AffectedCharacter = affectedCharacter;
			Instigator = instigator;
			CurrentHealth = currentHealth;
			DamageCaused = damageCaused;
			PreviousHealth = previousHealth;
		}

        static MMDamageTakenEvent e;
        public static void Trigger(Character affectedCharacter, GameObject instigator, float currentHealth, float damageCaused, float previousHealth)
        {
            e.AffectedCharacter = affectedCharacter;
            e.Instigator = instigator;
            e.CurrentHealth = currentHealth;
            e.DamageCaused = damageCaused;
            e.PreviousHealth = previousHealth;
            MMEventManager.TriggerEvent(e);
        }
    }
}