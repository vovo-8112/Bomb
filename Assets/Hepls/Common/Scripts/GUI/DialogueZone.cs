using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    [Serializable]
    public class DialogueElement
    {
        [Multiline]
        public string DialogueLine;
    }
    [AddComponentMenu("TopDown Engine/GUI/Dialogue Zone")]
    public class DialogueZone : ButtonActivated
    {
        [Header("Dialogue Look")]
        [Tooltip("the prefab to use to display the dialogue")]
        public DialogueBox DialogueBoxPrefab;
        [Tooltip("the color of the text background.")]
        public Color TextBackgroundColor = Color.black;
        [Tooltip("the color of the text")]
        public Color TextColor = Color.white;
        [Tooltip("the font that should be used to display the text")]
        public Font TextFont;
        [Tooltip("the size of the font")]
        public int TextSize = 40;
        [Tooltip("the text alignment in the box used to display the text")]
        public TextAnchor Alignment = TextAnchor.MiddleCenter;
        
        [Header("Dialogue Speed (in seconds)")]
        [Tooltip("the duration of the in and out fades")]
        public float FadeDuration = 0.2f;
        [Tooltip("the time between two dialogues ")]
        public float TransitionTime = 0.2f;

        [Header("Dialogue Position")]
        [Tooltip("the distance from the top of the box collider the dialogue box should appear at")]
        public Vector3 Offset = Vector3.zero;
        [Tooltip("if this is true, the dialogue boxes will follow the zone's position")]
        public bool BoxesFollowZone = false;

        [Header("Player Movement")]
        [Tooltip("if this is set to true, the character will be able to move while dialogue is in progress")]
        public bool CanMoveWhileTalking = true;

        [Header("Press button to go from one message to the next ?")]
        [Tooltip("whether or not this zone is handled by a button or not")]
        public bool ButtonHandled = true;
        [Header("Only if the dialogue is not button handled :")]
        [Range(1, 100)]
        [Tooltip("The duration for which the message should be displayed, in seconds. only considered if the box is not button handled")]
        public float MessageDuration = 3f;
        
        [Header("Activations")]
        [Tooltip("true if can be activated more than once")]
        public bool ActivableMoreThanOnce = true;
        [Range(1, 100)]
        [Tooltip("if the zone is activable more than once, how long should it remain inactive between up times?")]
        public float InactiveTime = 2f;
        [Tooltip("the dialogue lines")]
        public DialogueElement[] Dialogue;
        protected DialogueBox _dialogueBox;
        protected bool _activated = false;
        protected bool _playing = false;
        protected int _currentIndex;
        protected bool _activable = true;
        protected WaitForSeconds _transitionTimeWFS;
        protected WaitForSeconds _messageDurationWFS;
        protected WaitForSeconds _inactiveTimeWFS;
        protected override void OnEnable()
        {
            base.OnEnable();
            _currentIndex = 0;
            _transitionTimeWFS = new WaitForSeconds(TransitionTime);
            _messageDurationWFS = new WaitForSeconds(MessageDuration);
            _inactiveTimeWFS = new WaitForSeconds(InactiveTime);
        }
        public override void TriggerButtonAction()
        {
            if (!CheckNumberOfUses())
            {
                return;
            }
            if (_playing && !ButtonHandled)
            {
                return;
            }
            base.TriggerButtonAction();
            StartDialogue();
            ActivateZone();
        }
        public virtual void StartDialogue()
        {
            if ((_collider == null) && (_collider2D == null))
            {
                return;
            }
            if (_activated && !ActivableMoreThanOnce)
            {
                return;
            }
            if (!_activable)
            {
                return;
            }
            if (!CanMoveWhileTalking)
            {
                LevelManager.Instance.FreezeCharacters();
                if (ShouldUpdateState && (_characterButtonActivation != null))
                {
                    _characterButtonActivation.GetComponentInParent<Character>().MovementState.ChangeState(CharacterStates.MovementStates.Idle);
                }
            }
            if (!_playing)
            {
                _dialogueBox = Instantiate(DialogueBoxPrefab);
                if (_collider2D != null)
                {
                    _dialogueBox.transform.position = _collider2D.bounds.center + Offset;
                }
                if (_collider != null)
                {
                    _dialogueBox.transform.position = _collider.bounds.center + Offset;
                }
                _dialogueBox.ChangeColor(TextBackgroundColor, TextColor);
                _dialogueBox.ButtonActive(ButtonHandled);
                if (BoxesFollowZone)
                {
                    _dialogueBox.transform.SetParent(this.gameObject.transform);
                }
                if (TextFont != null)
                {
                    _dialogueBox.DialogueText.font = TextFont;
                }
                if (TextSize != 0)
                {
                    _dialogueBox.DialogueText.fontSize = TextSize;
                }
                _dialogueBox.DialogueText.alignment = Alignment;
                _playing = true;
            }
            StartCoroutine(PlayNextDialogue());
        }
        protected virtual void EnableCollider(bool status)
        {
            if (_collider2D != null)
            {
                _collider2D.enabled = status;
            }
            if (_collider != null)
            {
                _collider.enabled = status;
            }
        }
        protected virtual IEnumerator PlayNextDialogue()
        {
            if (_dialogueBox == null)
            {
                yield break;
            }
            if (_currentIndex != 0)
            {
                _dialogueBox.FadeOut(FadeDuration);
                yield return _transitionTimeWFS;
            }
            if (_currentIndex >= Dialogue.Length)
            {
                _currentIndex = 0;
                Destroy(_dialogueBox.gameObject);
                EnableCollider(false);
                _activated = true;
                if (!CanMoveWhileTalking)
                {
                    LevelManager.Instance.UnFreezeCharacters();
                }
                if ((_characterButtonActivation != null))
                {
                    _characterButtonActivation.InButtonActivatedZone = false;
                    _characterButtonActivation.ButtonActivatedZone = null;
                }
                if (ActivableMoreThanOnce)
                {
                    _activable = false;
                    _playing = false;
                    StartCoroutine(Reactivate());
                }
                else
                {
                    gameObject.SetActive(false);
                }
                yield break;
            }
            if (_dialogueBox.DialogueText != null)
            {
                _dialogueBox.FadeIn(FadeDuration);
                _dialogueBox.DialogueText.text = Dialogue[_currentIndex].DialogueLine;
            }

            _currentIndex++;
            if (!ButtonHandled)
            {
                StartCoroutine(AutoNextDialogue());
            }
        }
        protected virtual IEnumerator AutoNextDialogue()
        {
            yield return _messageDurationWFS;
            StartCoroutine(PlayNextDialogue());
        }
        protected virtual IEnumerator Reactivate()
        {
            yield return _inactiveTimeWFS;
            EnableCollider(true);
            _activable = true;
            _playing = false;
            _currentIndex = 0;
            _promptHiddenForever = false;

            if (AlwaysShowPrompt)
            {
                ShowPrompt();
            }
        }
    }
}
