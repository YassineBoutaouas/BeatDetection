using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoundElements
{
    public static class RhythmReader
    {
        public static BpmMatchData[] BPMMatches = new BpmMatchData[800 + 1];

        public struct BpmMatchData
        {
            public float Match;
            public int BPM;
        }

        /// <summary>
        /// Calculates the BPM of a sound elements audio clip
        /// </summary>
        public static int CalculateBPM(AudioClip soundElement)
        {
            ///Get samples
            float[] allSamples = new float[soundElement.samples * soundElement.channels];
            soundElement.GetData(allSamples, 0);

            ///Set a sample duration
            float sampleDuration = 0.1f;

            ///amount of samples in 0.1s
            int segmentSampleAmount = Mathf.FloorToInt((soundElement.frequency * sampleDuration) / soundElement.channels);
            float splitFrequency = soundElement.frequency / segmentSampleAmount;

            ///amount of total segments in the file
            int totalSegments = Mathf.CeilToInt((float)allSamples.Length / (float)segmentSampleAmount);

            ///Create segments of samples
            List<float[]> segments = new List<float[]>();
            for (int segmentIndex = 0; segmentIndex < totalSegments; segmentIndex++)
            {
                float[] samplesInSegment = new float[segmentSampleAmount];

                for (int sampleIndex = 0; sampleIndex < segmentSampleAmount; sampleIndex++)
                {
                    int totalIndex = sampleIndex + segmentIndex * (segmentSampleAmount - 1);
                    samplesInSegment[sampleIndex] = allSamples[totalIndex];
                }

                segments.Add(samplesInSegment);
            }

            List<float> frequencyChanges = CalculateChanges(segments, segmentSampleAmount);

            return BPMMatches[MatchBPM(frequencyChanges, splitFrequency)].BPM;
        }

        /// <summary>
        /// Iterates through possible BPMs and tries to find the BPM that matches the amount of energy changes the most
        /// </summary>
        public static int MatchBPM(List<float> frequencyChanges, float splitFrequency)
        {
            int index = 0;
            for (int bpm = 1; bpm <= 800; bpm++)
            {
                float sinMatch = 0;
                float cosMatch = 0;
                float bps = (float)bpm / 60f;

                if (frequencyChanges.Count > 0)
                {
                    for (int i = 0; i < frequencyChanges.Count; i++)
                    {
                        sinMatch += (frequencyChanges[i] * Mathf.Cos(i * 2 * Mathf.PI * bps / splitFrequency));
                        cosMatch += (frequencyChanges[i] * Mathf.Sin(i * 2 * Mathf.PI * bps / splitFrequency));
                    }
                }

                float match = (sinMatch) + (cosMatch);
                BPMMatches[index].BPM = bpm;
                BPMMatches[index].Match = match;

                index++;
            }

            return Array.FindIndex(BPMMatches, x => x.Match == BPMMatches.Max(y => y.Match));
        }

        /// <summary>
        /// Gets the average energy of each segment
        /// </summary>
        public static List<float> CalculateChanges(List<float[]> segments, int segmentSampleAmount)
        {
            ///Calculate average frequency for each segment!
            float[] averages = new float[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                float sum = 0f;
                for (int j = 0; j < segments[i].Length; j++)
                {
                    float abs = Mathf.Abs(segments[i][j]);

                    sum += abs * abs;
                }

                averages[i] = sum / segmentSampleAmount;
            }

            ///Calulates the frequency changes of the frenquency bands over a time lag
            List<float> frequencyChanges = new List<float>();
            for (int i = 1; i < averages.Length; i++)
            {
                frequencyChanges.Add(Mathf.Max(averages[i] - averages[i - 1], 0f));
            }

            return frequencyChanges;
        }
    }
}