using UnityEngine;

namespace SoundElements
{
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundFile;
        protected AudioSource _audioSource;
        public float CurrentEventIndex { get; private set; }
        public float CurrentTime { get { return _audioSource.time; } private set { CurrentTime = value; } }

        protected virtual void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = SoundFile.AudioClip;
        }

        public void Play()
        {
            CurrentEventIndex = 0;
            _audioSource.Play();
        }

        protected virtual void Update() { }

        protected virtual void UpdateEvents()
        {
            if (!_audioSource.isPlaying)
            {
                CurrentTime = 0;
                return;
            }

            for (int i = 0; i < SoundFile.SoundEvents.Count; i++)
            {
                if (CurrentEventIndex != i) continue;

                SoundEvent e = SoundFile.SoundEvents[i];

                if (e.TimeStamp < CurrentTime)
                {
                    SendMessage(e.MethodName, e, SendMessageOptions.DontRequireReceiver);
                    CurrentEventIndex++;
                }
            }
        }
    }
}