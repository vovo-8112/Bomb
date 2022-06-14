using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMDebugOnScreenConsole : MonoBehaviour
    {
        [Header("Bindings")]
        public RectTransform Container;
        public Image BackgroundImage;
        public Text ConsoleText;

        [Header("Label")]
        public Color LabelColor = Color.white;

        [Header("Value")]
        public string ValueColor = "#FFC400";
        public float ValueSizeRatio = 1.35f;
        
        protected RectTransform _rectTransform;

        protected int _numberOfMessages = 0;
        protected bool _messageStackHasBeenDisplayed = false;
        protected bool _newMessageThisFrame = false;
        protected int _largestMessageLength = 0;
        protected StringBuilder _stringBuilder;

        protected string _valueTagStart;
        protected string _valueTagEnd;
        protected const string space = " ";

        protected Vector2 _closedSize = new Vector2(60, 80);
        protected Vector2 _openBackgroundWidth;
        protected int _last_append_at_frame = -1;

        public virtual void Toggle()
        {
            if (ConsoleText.enabled)
            {
                _openBackgroundWidth = BackgroundImage.rectTransform.sizeDelta;
                BackgroundImage.rectTransform.sizeDelta = _closedSize;
            }
            else
            {
                BackgroundImage.rectTransform.sizeDelta = _openBackgroundWidth;
            }
            ConsoleText.enabled = !ConsoleText.isActiveAndEnabled;
        }

        protected virtual void Awake()
        {
            Initialization();
        }
        
        protected virtual void Initialization()
        {
            ConsoleText.color = LabelColor;
            _stringBuilder = new StringBuilder();
            _rectTransform = this.gameObject.GetComponent<RectTransform>();

            _valueTagEnd = "</size></color>";
        }
        protected virtual void SetFontSize(int fontSize)
        {
            if (fontSize == ConsoleText.fontSize)
            {
                return;
            }
            ConsoleText.fontSize = fontSize;
            _valueTagStart = "<color=" + ValueColor + "><size=" + (ConsoleText.fontSize * ValueSizeRatio) + ">";
        }
        protected virtual void LateUpdate()
        {
            _messageStackHasBeenDisplayed = true;
            if (!_newMessageThisFrame && ConsoleText.isActiveAndEnabled)
            {
                this.gameObject.SetActive(false);
            }
            _newMessageThisFrame = false;
        }
        public virtual void SetScreenOffset(int top = 10, int left = 10)
        {
            Container.MMSetTop(top);
            Container.MMSetLeft(left);
        }
        public virtual void SetMessage(string newMessage)
        {
            AddMessage(newMessage, "", 30);
        }
        public virtual void AddMessage(string label, object value, int fontSize)
        {
            if (!this.gameObject.activeInHierarchy)
            {
                this.gameObject.SetActive(true);
            }

            int frame = Time.frameCount;

            if (!ConsoleText.isActiveAndEnabled)
            {
                return;
            }
            _newMessageThisFrame = true;
            SetFontSize(fontSize);
            if (_last_append_at_frame != frame)
            {
                _stringBuilder.Clear();
                _messageStackHasBeenDisplayed = false;
                _numberOfMessages = 0;
                _largestMessageLength = 0;
            }

            _last_append_at_frame = Time.frameCount;
            if (_stringBuilder.Length != 0)
            {
                _stringBuilder.Append(System.Environment.NewLine);
            }
            _stringBuilder.Append(label.ToUpper());
            _stringBuilder.Append(space);
            _stringBuilder.Append(_valueTagStart);
            _stringBuilder.Append(value);
            _stringBuilder.Append(_valueTagEnd);
            if (label.Length > _largestMessageLength)
            {
                _largestMessageLength = label.Length;
            }
            _numberOfMessages++;

            ConsoleText.text = _stringBuilder.ToString();
        }
    }
}

