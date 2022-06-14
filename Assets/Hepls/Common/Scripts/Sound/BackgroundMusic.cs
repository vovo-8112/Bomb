using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Sound/BackgroundMusic")]
    public class BackgroundMusic : MonoBehaviour
    {
        [Tooltip("the audio clip to use as background music")]
        public AudioClip SoundClip;
        [Tooltip("whether or not the music should loop")]
        public bool Loop = true;
        protected virtual void Start()
        {
            MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
            options.Loop = Loop;
            options.Location = Vector3.zero;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            
            MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
        }
    }
}