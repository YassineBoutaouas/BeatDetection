using System.Collections;
using UnityEngine;

namespace SoundElements
{
    /// <summary>
    /// This class provides runtime functionality to invoke events associated to a SoundElement.asset and its audioclip
    /// </summary>
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundElement;
        public System.Action OnBeat;

        protected AudioSource _audioSource;

        /// <summary>
        /// Keeps track of the current event that shall be invoked
        /// </summary>
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

        /// <summary>
        /// Resets the current event index and plays the audio source
        /// </summary>
        public void Play()
        {
            CurrentEventIndex = 0;
            _audioSource.Play();
        }

        protected virtual void Update() { }

        /// <summary>
        /// Iterates through the events during the playtime of an audio source. 
        /// If the audiosource.time corresponds to the currenteventindex the event with that index is invoked. 
        /// </summary>
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

        /// <summary>
        /// Interpolates the animation curve provided by the soundelement and assigns a float value.
        /// That value can be used to widen the input window or to ease animation between beats
        /// </summary>
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