using UnityEngine;

namespace SoundElements
{
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundFile;
        protected AudioSource _audioSource;

        private int _currentEventIndex;

        protected virtual void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = SoundFile.AudioClip;
            _currentEventIndex = 0;
        }

        protected virtual void Update() { }

        protected virtual void UpdateEvents()
        {
            if (!_audioSource.isPlaying) return;

            for(int i = 0; i < SoundFile.SoundEvents.Count; i++)
            {
                if (SoundFile.SoundEvents[i].TimeStamp.NearlyEquals(_audioSource.time) && _currentEventIndex == i)
                {
                    SendMessage(SoundFile.SoundEvents[i].MethodName, SoundFile.SoundEvents[i], SendMessageOptions.DontRequireReceiver);
                    _currentEventIndex++;
                }
            }
        }
    }
}