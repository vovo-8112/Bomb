using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Sound/PersistentBackgroundMusic")]
    public class PersistentBackgroundMusic : MMPersistentSingleton<PersistentBackgroundMusic>
    {
        [Tooltip("the background music clip to use as persistent background music")]
        public AudioClip SoundClip;
        [Tooltip("whether or not the music should loop")]
        public bool Loop = true;
        
        protected AudioSource _source;
        protected PersistentBackgroundMusic _otherBackgroundMusic;

        protected virtual void OnEnable()
        {
            _otherBackgroundMusic = (PersistentBackgroundMusic)FindObjectOfType(typeof(PersistentBackgroundMusic));
            if ((_otherBackgroundMusic != null) && (_otherBackgroundMusic != this) )
            {
                _otherBackgroundMusic.enabled = false;
            }
        }
        protected virtual void Start()
        {
            MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
            options.Loop = Loop;
            options.Location = Vector3.zero;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            options.Persistent = true;
            
            MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
        }
    }
}