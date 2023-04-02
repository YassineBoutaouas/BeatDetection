using System.Collections;
using UnityEngine;

namespace SoundElements
{
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundElement;
        public System.Action OnBeat;

        protected AudioSource _audioSource;
        public float CurrentEventIndex { get; private set; }

        public float BPS { get; private set; }
        private float _beatTimer;
        public float CurrentBeatValue { get; private set; }

        protected virtual void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = SoundElement.AudioClip;

            if (SoundElement.BPM > 0) 
                BPS = 60f / (float)SoundElement.BPM;
        }

        public void Play()
        {
            CurrentEventIndex = 0;
            _audioSource.Play();
        }

        protected virtual void Update() { }

        protected virtual void UpdateEvents()
        {
            if (!_audioSource.isPlaying) return;

            for (int i = 0; i < SoundElement.SoundEvents.Count; i++)
            {
                if (CurrentEventIndex != i) continue;

                SoundEvent e = SoundElement.SoundEvents[i];

                if (e.TimeStamp < _audioSource.time)
                {
                    SendMessage(e.MethodName, e, SendMessageOptions.DontRequireReceiver);
                    CurrentEventIndex++;
                }
            }
        }

        protected virtual void UpdateBeatTimer()
        {
            if (_beatTimer < BPS)
            {
                _beatTimer += Time.deltaTime;
                CurrentBeatValue = SoundElement.InterpolationCurve.Evaluate(_beatTimer / BPS);

                return;
            }

            OnBeat?.Invoke();
            _beatTimer = 0;
        }

        protected virtual void OnDestroy()
        {
            OnBeat = null;
        }
    }
}