using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	public class MMConsole : MonoBehaviour 
	{
		protected string _messageStack;

		protected int _numberOfMessages=0;
		protected bool _messageStackHasBeenDisplayed=false;
		protected int _largestMessageLength=0;

		protected int _marginTop = 10;
		protected int _marginLeft = 10;
		protected int _padding = 10;

		protected int _fontSize = 10;
		protected int _characterHeight = 16;
		protected int _characterWidth = 6;
		protected virtual void OnGUI()
		{
	        GUIStyle style = GUI.skin.GetStyle ("label");
			style.fontSize = _fontSize;
			int boxHeight = _numberOfMessages*_characterHeight;
			int boxWidth = _largestMessageLength * _characterWidth;
			GUI.Box (new Rect (_marginLeft,_marginTop,boxWidth+_padding*2,boxHeight+_padding*2), "");
			GUI.Label(new Rect(_marginLeft+_padding, _marginTop+_padding, boxWidth, boxHeight), _messageStack);
			_messageStackHasBeenDisplayed=true;
		}
		public virtual void SetFontSize(int fontSize)
		{
			_fontSize = fontSize;
			_characterHeight = (int)Mathf.Round(1.6f * fontSize + 0.49f);
			_characterWidth = (int)Mathf.Round(0.6f * fontSize + 0.49f);
		}
		public virtual void SetScreenOffset(int top = 10, int left = 10)
		{
			_marginTop = top;
			_marginLeft = left;
		}
		public virtual void SetMessage(string newMessage)
		{
			_messageStack=newMessage;
			_numberOfMessages=1;
		}
		public virtual void AddMessage(string newMessage)
		{
			if (_messageStackHasBeenDisplayed)
			{
				_messageStack="";
				_messageStackHasBeenDisplayed=false;
				_numberOfMessages=0;
				_largestMessageLength=0;
			}
			_messageStack += newMessage+"\n";
			if (newMessage.Length > _largestMessageLength)
			{
				_largestMessageLength = newMessage.Length;
			}
			_numberOfMessages++;
		}
	}
}
