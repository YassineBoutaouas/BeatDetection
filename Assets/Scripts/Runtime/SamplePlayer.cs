

using UnityEngine;

namespace SoundElements
{
    public class SamplePlayer : SoundPlayer
    {
        protected override void Start()
        {
            base.Start();

            _audioSource.Play();
        }

        protected override void Update()
        {
            UpdateEvents();
        }

        protected void OnSoundEvent()
        {
            Debug.Log("Sound Event");
        }
    }
}