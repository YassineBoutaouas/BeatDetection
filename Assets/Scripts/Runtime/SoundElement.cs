using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoundElements
{
    /// <summary>
    /// This class contains additional information on a given audio clip and extends it - An instance can be created through the sound editor
    /// </summary>
    public class SoundElement : ScriptableObject
    {
        public string Name;
        public AudioClip AudioClip;
        public int BPM;

        public AnimationCurve InterpolationCurve = new AnimationCurve();

        [NonReorderable] public List<SoundEvent> SoundEvents = new List<SoundEvent>();
    }

    /// <summary>
    /// This class contains information on the method that is invoked through the event, the values as well as a time stamp in order to be invoked at a specific time of the audio clip
    /// </summary>
    [Serializable]
    public class SoundEvent
    {
        public string MethodName;

        public float TimeStamp;

        public float FloatValue;
        public int IntValue;
        public string StringValue;
        public bool BoolValue;

        public SoundEvent(float timeStamp)
        {
            TimeStamp = timeStamp;
        }
    }
}