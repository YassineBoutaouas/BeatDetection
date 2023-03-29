using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;

namespace SoundElements
{
    public abstract class SoundPlayer : MonoBehaviour
    {
        public SoundElement SoundElement;
        protected AudioSource _audioSource;
        public float CurrentEventIndex { get; private set; }

        public int BPM;

        public BpmMatchData[] BPMMatches = new BpmMatchData[400 + 1];

        public struct BpmMatchData
        {
            public float Match;
            public int BPM;
        }

        public void CalculateBPM()
        {
            ///Create waveform
            int samplesSize = SoundElement.AudioClip.samples * SoundElement.AudioClip.channels;
            float[] allSamples = new float[samplesSize];
            SoundElement.AudioClip.GetData(allSamples, 0);

            ///divide into samples
            float sampleDuration = 0.1f;


            ///amount of samples in 0.1s
            int segmentSampleAmount = Mathf.FloorToInt(SoundElement.AudioClip.frequency * sampleDuration / SoundElement.AudioClip.channels);

            ///create peak array/ Volume array
            float[] amplitudeArray = new float[Mathf.CeilToInt((float)allSamples.Length / (float)segmentSampleAmount)];
            int powerIndex = 0;

            for (int segmentIndex = 0; segmentIndex < allSamples.Length; segmentIndex += segmentSampleAmount)
            {
                float sum = 0f;

                //for(int s = 0; s < sampleLength; s++)
                //sapmle = segmentIndex * segmentLength
                for (int sample = segmentIndex; sample < segmentIndex + segmentSampleAmount; sample++)
                {
                    if (allSamples.Length <= sample) break;

                    float absValue = Mathf.Abs(allSamples[sample]);

                    if (absValue > 1) continue;

                    sum += (absValue * absValue);
                }

                // Set volume value
                amplitudeArray[powerIndex] = Mathf.Sqrt(sum / segmentSampleAmount);
                powerIndex++;
            }

            ///Average amplitudes
            float maxAmplitude = amplitudeArray.Max();
            for (int i = 0; i < amplitudeArray.Length; i++)
            {
                amplitudeArray[i] = amplitudeArray[i] / maxAmplitude;
                //Debug.Log(amplitudeArray[i] / maxAmplitude);
            }
            ///-------------------------------------------------------------------------------------

            ///Search BPM for segments / Correlation list
            List<float> amplitudeDifferences = new List<float>();
            for (int i = 1; i < amplitudeArray.Length; i++)
            {
                amplitudeDifferences.Add(Mathf.Max(amplitudeArray[i] - amplitudeArray[i - 1], 0f));
            }

            ///Calculate the correlation
            float splitFrequency = (float)SoundElement.AudioClip.frequency / (float)segmentSampleAmount;
            Debug.Log(splitFrequency);

            int index = 0;
            for (int bpm = 60; bpm <= 400; bpm++)
            {
                float sinMatch = 0;
                float cosMatch = 0;
                float bps = (float)bpm / 60f;

                if (amplitudeDifferences.Count > 0)
                {
                    for (int i = 0; i < amplitudeDifferences.Count; i++)
                    {
                        sinMatch += (amplitudeDifferences[i] * Mathf.Cos(i * 2 * Mathf.PI * bps / splitFrequency));
                        cosMatch += (amplitudeDifferences[i] * Mathf.Sin(i * 2 * Mathf.PI * bps / splitFrequency));
                    }

                    sinMatch *= (1 / (float)amplitudeDifferences.Count); //average
                    cosMatch *= (1 / (float)amplitudeDifferences.Count); //average
                }

                float match = Mathf.Sqrt((sinMatch * sinMatch) + (cosMatch * cosMatch));
                BPMMatches[index].BPM = bpm;
                BPMMatches[index].Match = match;
                index++;
            }

            int assumedBPMIndex = Array.FindIndex(BPMMatches, x => x.Match == BPMMatches.Max(y => y.Match));
            Debug.Log($"BPM: {BPMMatches[assumedBPMIndex].BPM}");
            /////---------------------------------------------------------------------------------------------------------

            BPM = 0;
        }

        protected virtual void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = SoundElement.AudioClip;

            UniBpmAnalyzer.AnalyzeBpm(SoundElement.AudioClip);
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