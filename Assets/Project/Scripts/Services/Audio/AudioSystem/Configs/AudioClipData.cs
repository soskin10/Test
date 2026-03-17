using System;
using UnityEngine;

namespace Project.Scripts.Services.Audio.AudioSystem.Configs
{
    [Serializable]
    public class AudioClipData
    {
        public AudioClip Clip;
        public string Tag;
        [Range(0f, 1f)] public float DefaultVolume = 1.0f;
    }
}