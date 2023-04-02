using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoundElements
{
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundElement;
        protected AudioSource _audioSource;
        public float CurrentEventIndex { get; private set; }

        protected virtual void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = SoundElement.AudioClip;
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
    }
}