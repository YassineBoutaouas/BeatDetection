using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayerTemp : MonoBehaviour
{
    public SoundElement SoundFile;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.clip = SoundFile.AudioClip;
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;

        _audioSource.volume = 1.0f;

        _audioSource.Play();
    }

    public void Update()
    {
        if (!_audioSource.isPlaying) return;

        foreach (SoundEvent evt in SoundFile.SoundEvents)
        {
            if (evt.TimeStamp.NearlyEquals(_audioSource.time))
                SendMessage(evt.MethodName, evt, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnSoundEvent(SoundEvent evt)
    {
        Debug.Log("OnSoundEvent");
    }
}