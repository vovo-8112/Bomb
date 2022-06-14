using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Persistence")]
    public class CharacterPersistence : CharacterAbility, MMEventListener<MMGameEvent>, MMEventListener<TopDownEngineEvent>
    {
        public bool Initialized { get; set; }
        protected override void Initialization()
        {
            base.Initialization();

            if (AbilityAuthorized)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            Initialized = true;
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            Initialized = false;
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == "Save")
            {
                SaveCharacter();
            }
        }
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            if (!AbilityAuthorized)
            {
                return;
            }

            switch (engineEvent.EventType)
            {
                case TopDownEngineEventTypes.LoadNextScene:
                    this.gameObject.SetActive(false);
                    break;
                case TopDownEngineEventTypes.SpawnCharacterStarts:
                    this.transform.position = LevelManager.Instance.InitialSpawnPoint.transform.position;
                    this.gameObject.SetActive(true);
                    Character character = this.gameObject.GetComponentInParent<Character>(); 
                    character.enabled = true;
                    character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
                    character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
                    character.SetInputManager();
                    break;
                case TopDownEngineEventTypes.LevelStart:
                    if (_health != null)
                    {
                        _health.StoreInitialPosition();    
                    }
                    break;
                case TopDownEngineEventTypes.RespawnComplete:
                    Initialized = true;
                    break;
            }
        }
        protected virtual void SaveCharacter()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            GameManager.Instance.PersistentCharacter = _character;
        }
        public virtual void ClearSavedCharacter()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            GameManager.Instance.PersistentCharacter = null;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<TopDownEngineEvent>();
        }
		protected virtual void OnDestroy()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}