using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Services.Audio.AudioSystem.Configs
{
    [CreateAssetMenu(menuName = "Configs/AudioSFXConfig", fileName = "AudioSFXConfig")]
    public class AudioSFXConfig : ScriptableObject
    {
        public List<AudioGroupData> Groups = new();
    }
}