using UnityEngine;

namespace SoundElements
{
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundFile;
        protected AudioSource _audioSource;

        protected virtual void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = SoundFile.AudioClip;
        }

        protected virtual void Update() { }

        protected virtual void UpdateEvents()
        {
            if (!_audioSource.isPlaying) return;

            foreach (SoundEvent evt in SoundFile.SoundEvents)
            {
                if (evt.TimeStamp.NearlyEquals(_audioSource.time))
                    SendMessage(evt.MethodName, evt, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}