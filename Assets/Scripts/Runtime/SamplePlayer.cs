using System.Collections;
using UnityEngine;

namespace SoundElements
{
    public class SamplePlayer : SoundPlayer
    {
        [ColorUsage(false, true)] public Color Standard;
        [ColorUsage(false, true)] public Color Drop;

        private float _scaleMultiplierMax = 1f;
        private float _scaleMultiplierMin = 0.8f;

        protected override void Start()
        {
            base.Start();

            Play();
            OnBeat += Beat;
        }

        protected override void Update() 
        { 
            UpdateEvents(); 
            UpdateBeatTimer();
        }

        public void OnDrop() 
        {
            Material m = GetComponent<Renderer>().material;
            m.SetColor("_EmissionColor", Drop);

            _scaleMultiplierMax = 0.7f;
            _scaleMultiplierMin = 0.4f;
        }

        private void Beat()
        {
            StartCoroutine(OnBeatRoutine());
        }

        private IEnumerator OnBeatRoutine()
        {
            float t = 0;
            while(t < BPS)
            {
                yield return null;
                t += Time.deltaTime;

                transform.localScale = Vector3.Lerp(Vector3.one * _scaleMultiplierMax, _scaleMultiplierMin * Vector3.one, SoundElement.InterpolationCurve.Evaluate(t / BPS));
            }
        }
    }
}