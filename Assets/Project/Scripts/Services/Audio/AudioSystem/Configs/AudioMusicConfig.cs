using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Services.Audio.AudioSystem.Configs
{
    [CreateAssetMenu(menuName = "Configs/AudioMusicConfig", fileName = "AudioMusicConfig")]
    public class AudioMusicConfig : ScriptableObject
    {
        public List<AudioGroupData> Groups = new();
    }
}