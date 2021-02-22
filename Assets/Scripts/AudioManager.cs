﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSound[] sounds;

    void Awake()
    {
        foreach (AudioSound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;            
        }
    }

    public void Play(string name) 
    {
        AudioSound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

}
