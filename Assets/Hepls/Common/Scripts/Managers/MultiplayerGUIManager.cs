using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Managers/Multiplayer GUIManager")]
    public class MultiplayerGUIManager : GUIManager
    {
        [Header("Multiplayer GUI")]
        [Tooltip("the HUD to display when in split screen mode")]
        public GameObject SplitHUD;
        [Tooltip("the HUD to display when in group camera mode")]
        public GameObject GroupHUD;
        [Tooltip("a UI object used to display the splitters UI images")]
        public GameObject SplittersGUI;
    }
}
