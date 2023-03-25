using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MusicPlayerTemp : MonoBehaviour
{
    public SoundElement SoundFile;
    public Dictionary<SoundEvent, MethodInfo> Events = new Dictionary<SoundEvent, MethodInfo>();
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = SoundFile.AudioClip;
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;

        foreach (SoundEvent evt in SoundFile.SoundEvents)
        {
            MethodInfo info = GetType().GetMethod(evt.MethodName);

            Events.Add(evt, info);
        }
    }

    public void Update()
    {
        if (!_audioSource.isPlaying) return;

        foreach (SoundEvent evt in SoundFile.SoundEvents)
        {
            if (evt.TimeStamp == _audioSource.time)
            {
                Events[evt].Invoke(evt, null);
            }
        }
    }
}
