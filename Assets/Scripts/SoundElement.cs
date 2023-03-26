using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundElement : ScriptableObject
{
    public string Name;
    public AudioClip AudioClip;

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