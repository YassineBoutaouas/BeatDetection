using UnityEngine;

namespace SoundElements
{
    public class SamplePlayer : SoundPlayer
    {
        protected override void Start()
        {
            base.Start();

            Play();
        }

        protected override void Update()
        {
            UpdateEvents();
        }

        public void OnDrop()
        {
            Debug.Log("On Drop");
        }
    }
}