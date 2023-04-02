using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoundElements
{
    public class SoundElement : ScriptableObject
    {
        public string Name;
        public AudioClip AudioClip;
        public int BPM;

        [NonReorderable] public List<SoundEvent> SoundEvents = new List<SoundEvent>();
    }

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