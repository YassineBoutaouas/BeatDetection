using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundElement : ScriptableObject
{
    public string Name;
    public AudioClip AudioClip;
    List<SoundEvent> SoundEvents = new List<SoundEvent>();
}

public class SoundEvent
{
    public EventType EventType;
    public float TimeStamp;

    public float FloatValue;
    public int IntValue;
    public string StringValue;
    public bool BoolValue;
    public Object ObjectValue;
}

public enum EventType
{
    Float,
    Int,
    String,
    Bool,
    Object
}