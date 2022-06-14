using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class MMDebugMenuChoiceEntry
    {
        public Button TargetButton;
        public Text ButtonText;
        public Image ButtonBg;
        public string ButtonEventName = "ButtonEvent";
    }
    public class MMDebugMenuItemChoices : MonoBehaviour
    {
        [Header("Bindings")]
        public Sprite SelectedSprite;
        public Sprite OffSprite;
        public Color OnColor = Color.white;
        public Color OffColor = Color.black;
        public Color AccentColor = MMColors.ReunoYellow;
        public List<MMDebugMenuChoiceEntry> Choices;
        public virtual void TriggerButtonEvent(int index)
        {
            MMDebugMenuButtonEvent.Trigger(Choices[index].ButtonEventName);
        }
        public virtual void Select(int index)
        {
            Deselect();
            Choices[index].ButtonBg.sprite = SelectedSprite;
            Choices[index].ButtonBg.color = AccentColor;
            Choices[index].ButtonText.color = OffColor;
        }
        public virtual void Deselect()
        {
            foreach(MMDebugMenuChoiceEntry entry in Choices)
            {
                entry.ButtonBg.sprite = OffSprite;
                entry.ButtonBg.color = OnColor;
                entry.ButtonText.color = OnColor;
            }
        }
    }
}
