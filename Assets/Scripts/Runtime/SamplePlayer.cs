using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace SoundElements
{
    public class SamplePlayer : SoundPlayer
    {
        [ColorUsage(false, true)] public Color Standard;
        [ColorUsage(false, true)] public Color Drop;

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
            Debug.Log("Drop");
            StartCoroutine(DropBehavior());
        }

        private IEnumerator DropBehavior()
        {
            Material m = GetComponent<Renderer>().material;

            float t = 0;

            while(t < 0.05f)
            {
                yield return null;
                t += Time.deltaTime;

                m.SetColor("_EmissionColor", Color.Lerp(Standard, Drop, t / 0.05f));
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, t / 0.05f);
            }
        }
    }
}