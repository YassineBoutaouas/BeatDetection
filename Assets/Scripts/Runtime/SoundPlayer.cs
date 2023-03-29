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

        public int BPM;

        public BpmMatchData[] BPMMatches = new BpmMatchData[800 + 1];

        public struct BpmMatchData
        {
            public float Match;
            public int BPM;
        }

        public void CalculateBPM()
        {
            ///Get samples
            float[] allSamples = new float[SoundElement.AudioClip.samples * SoundElement.AudioClip.channels];
            SoundElement.AudioClip.GetData(allSamples, 0);

            ///divide into samples
            float sampleDuration = 0.1f;

            ///amount of samples in 0.1s
            int segmentSampleAmount = Mathf.FloorToInt((SoundElement.AudioClip.frequency * sampleDuration) / SoundElement.AudioClip.channels);

            int totalSegments = Mathf.CeilToInt((float)allSamples.Length / (float)segmentSampleAmount);
            
            ///create peak array/ Volume array
            float[] similarityArray = new float[totalSegments];
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
                similarityArray[powerIndex] = Mathf.Sqrt(sum / segmentSampleAmount);
                powerIndex++;
            }

            ///Average amplitudes
            float maxAmplitude = similarityArray.Max();
            for (int i = 0; i < similarityArray.Length; i++)
            {
                similarityArray[i] = similarityArray[i] / maxAmplitude;
                //Debug.Log(amplitudeArray[i] / maxAmplitude);
            }
            ///-------------------------------------------------------------------------------------

            ///Search BPM for segments / Correlation list
            List<float> amplitudeDifferences = new List<float>();
            for (int i = 1; i < similarityArray.Length; i++)
            {
                amplitudeDifferences.Add(Mathf.Max(similarityArray[i] - similarityArray[i - 1], 0f));
            }

            ///Calculate the correlation
            float splitFrequency = (float)SoundElement.AudioClip.frequency / (float)segmentSampleAmount;

            int index = 0;
            for (int bpm = 1; bpm <= 800; bpm++)
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

        protected float Mean(float[] floatValues)
        {
            float sum = 0;
            for (int i = 0; i < floatValues.Length; i++)
                sum += floatValues[i];

            return sum / floatValues.Length;
        }

        public float[] Autocorrelation(float[] x)
        {
            float mean = Mean(x);
            float[] autocorrelation = new float[x.Length / 2];
            for (int t = 0; t < autocorrelation.Length; t++)
            {
                float n = 0; // Numerator
                float d = 0; // Denominator
                for (int i = 0; i < x.Length; i++)
                {
                    float xim = x[i] - mean;
                    n += xim * (x[(i + t) % x.Length] - mean);
                    d += xim * xim;
                }
                autocorrelation[t] = n / d;
            }
            return autocorrelation;
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