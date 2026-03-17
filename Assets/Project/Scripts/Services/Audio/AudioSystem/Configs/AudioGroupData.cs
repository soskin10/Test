using System;
using System.Collections.Generic;

namespace Project.Scripts.Services.Audio.AudioSystem.Configs
{
    [Serializable]
    public class AudioGroupData
    {
        public string GroupTag;
        public bool Loop;
        public List<AudioClipData> Clips = new();
    }
}