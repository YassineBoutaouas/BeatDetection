using System.Collections;
using UnityEngine;

namespace SoundElements
{
    public class SamplePlayer : SoundPlayer
    {
        [ColorUsage(false, true)] public Color Standard;
        [ColorUsage(false, true)] public Color Drop;
        [ColorUsage(false, true)] public Color Low;

        private float _scaleMultiplierMax = 0.7f;
        private float _scaleMultiplierMin = 0.4f;

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

            _scaleMultiplierMax = 1.2f;
            _scaleMultiplierMin = 0.4f;
        }

        public void OnLow()
        {
            Material m = GetComponent<Renderer>().material;
            m.SetColor("_EmissionColor", Low);

            _scaleMultiplierMax = 0.7f;
            _scaleMultiplierMin = 0.4f;
        }

        public void OnEnd()
        {
            Material m = GetComponent<Renderer>().material;
            m.SetColor("_EmissionColor", Standard);

            _scaleMultiplierMax = 1f;
            _scaleMultiplierMin = 1f;
        }

        private void Beat()
        {
            StartCoroutine(OnBeatRoutine());
        }

        private IEnumerator OnBeatRoutine()
        {
            float t = 0;
            transform.localScale = Vector3.one * _scaleMultiplierMax;

            while (t < BPS)
            {
                yield return null;
                t += Time.deltaTime;

                transform.localScale = Vector3.Lerp(Vector3.one * _scaleMultiplierMax, _scaleMultiplierMin * Vector3.one, t / BPS);
            }
        }
    }
}