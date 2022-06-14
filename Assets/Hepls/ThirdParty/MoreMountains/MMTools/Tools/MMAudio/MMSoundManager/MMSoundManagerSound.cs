using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [Serializable]
    public struct MMSoundManagerSound
    {
        public int ID;
        public MMSoundManager.MMSoundManagerTracks Track;
        public AudioSource Source;
        public bool Persistent;
    }
}
