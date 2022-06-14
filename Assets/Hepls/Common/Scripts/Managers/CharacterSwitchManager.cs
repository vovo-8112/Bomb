using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Managers/CharacterSwitchManager")]
    public class CharacterSwitchManager : MonoBehaviour
    {
        public enum NextCharacterChoices { Sequential, Random }

        [Header("Character Switch")]
        [MMInformation("Add this component to an empty object in your scene, and when you'll press the SwitchCharacter button (P by default, change that in Unity's InputManager settings), your main character will be replaced by one of the prefabs in the list set on this component. You can decide the order (sequential or random), and have as many as you want.", MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the list of possible characters prefabs to switch to")]
        public Character[] CharacterPrefabs;
        [Tooltip("the order in which to pick the next character")]
        public NextCharacterChoices NextCharacterChoice = NextCharacterChoices.Sequential;
        [Tooltip("the initial (and at runtime, current) index of the character prefab")]
        public int CurrentIndex = 0;
        [Tooltip("if this is true, current health value will be passed from character to character")]
        public bool CommonHealth;

        [Header("Visual Effects")]
        [Tooltip("a particle system to play when a character gets changed")]
        public ParticleSystem CharacterSwitchVFX;

        protected Character[] _instantiatedCharacters;
        protected ParticleSystem _instantiatedVFX;
        protected InputManager _inputManager;
        protected TopDownEngineEvent _switchEvent = new TopDownEngineEvent(TopDownEngineEventTypes.CharacterSwitch, null);
        protected virtual void Start()
        {
            _inputManager = FindObjectOfType(typeof(InputManager)) as InputManager;
            InstantiateCharacters();
            InstantiateVFX();
        }
        protected virtual void InstantiateCharacters()
        {
            _instantiatedCharacters = new Character[CharacterPrefabs.Length];

            for (int i = 0; i < CharacterPrefabs.Length; i++)
            {
                Character newCharacter = Instantiate(CharacterPrefabs[i]);
                newCharacter.name = "CharacterSwitch_" + i;
                newCharacter.gameObject.SetActive(false);
                _instantiatedCharacters[i] = newCharacter;
            }
        }
        protected virtual void InstantiateVFX()
        {
            if (CharacterSwitchVFX != null)
            {
                _instantiatedVFX = Instantiate(CharacterSwitchVFX);
                _instantiatedVFX.Stop();
                _instantiatedVFX.gameObject.SetActive(false);
            }
        }
        protected virtual void Update()
        {
            if (_inputManager == null)
            {
                return;
            }

            if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                SwitchCharacter();
            }
        }
        protected virtual void SwitchCharacter()
        {
            if (_instantiatedCharacters.Length <= 1)
            {
                return;
            }
            if (NextCharacterChoice == NextCharacterChoices.Random)
            {
                CurrentIndex = Random.Range(0, _instantiatedCharacters.Length);
            }
            else
            {
                CurrentIndex = CurrentIndex + 1;
                if (CurrentIndex >= _instantiatedCharacters.Length)
                {
                    CurrentIndex = 0;
                }
            }
            LevelManager.Instance.Players[0].gameObject.SetActive(false);
            _instantiatedCharacters[CurrentIndex].gameObject.SetActive(true);
            _instantiatedCharacters[CurrentIndex].transform.position = LevelManager.Instance.Players[0].transform.position;
            _instantiatedCharacters[CurrentIndex].transform.rotation = LevelManager.Instance.Players[0].transform.rotation;
            if (CommonHealth)
            {
                _instantiatedCharacters[CurrentIndex].gameObject.MMGetComponentNoAlloc<Health>().SetHealth(LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth);
            }
            _instantiatedCharacters[CurrentIndex].MovementState.ChangeState(LevelManager.Instance.Players[0].MovementState.CurrentState);
            _instantiatedCharacters[CurrentIndex].ConditionState.ChangeState(LevelManager.Instance.Players[0].ConditionState.CurrentState);
            LevelManager.Instance.Players[0] = _instantiatedCharacters[CurrentIndex];
            if (_instantiatedVFX != null)
            {
                _instantiatedVFX.gameObject.SetActive(true);
                _instantiatedVFX.transform.position = _instantiatedCharacters[CurrentIndex].transform.position;
                _instantiatedVFX.Play();
            }
            MMEventManager.TriggerEvent(_switchEvent);
        }
    }
}