using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    public class DialogueBox : MonoBehaviour
    {
        [Header("Dialogue Box")]
        [Tooltip("the text panel background")]
        public CanvasGroup TextPanelCanvasGroup;
        [Tooltip("the text to display")]
        public Text DialogueText;
        [Tooltip("the Button A prompt")]
        public CanvasGroup Prompt;
        [Tooltip("the list of images to colorize")]
        public List<Image> ColorImages;

        protected Color _backgroundColor;
        protected Color _textColor;
        public virtual void ChangeText(string newText)
        {
            DialogueText.text = newText;
        }
        public virtual void ButtonActive(bool state)
        {
            Prompt.gameObject.SetActive(state);
        }
        public virtual void ChangeColor(Color backgroundColor, Color textColor)
        {
            _backgroundColor = backgroundColor;
            _textColor = textColor;
            
            foreach(Image image in ColorImages)
            {
                image.color = _backgroundColor;
            }
            DialogueText.color = _textColor;
        }
        public virtual void FadeIn(float duration)
        {
            if (TextPanelCanvasGroup != null)
            {
                StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 1f));
            }
            if (DialogueText != null)
            {
                StartCoroutine(MMFade.FadeText(DialogueText, duration, _textColor));
            }
            if (Prompt != null)
            {
                StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 1f));
            }
        }
        public virtual void FadeOut(float duration)
        {
            Color newBackgroundColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
            Color newTextColor = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

            StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 0f));
            StartCoroutine(MMFade.FadeText(DialogueText, duration, newTextColor));
            StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 0f));
        }
    }
}