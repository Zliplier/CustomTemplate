using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.Audio
{
    public class AudioManager : PersistantSingleton<AudioManager>
    {
        public const float DEFAULT_VOLUME = 0.7f;
        
        [SerializeField] private AudioMixer audioMixer;
        
        public float GetVolume(MixerType type)
        {
            float volume = DEFAULT_VOLUME;  
            
            if (PlayerPrefs.HasKey(type.ToString()))
                volume = PlayerPrefs.GetFloat(type.ToString());
            else
            {
                //No Saved Volume Data
                SetVolume(type, volume);
            }
            
            return volume;
        }

        public void SetVolume(MixerType type, float volume)
        {
            PlayerPrefs.SetFloat(type.ToString(), volume);
            audioMixer.SetFloat(type.ToString(), Mathf.Log10(volume) * 20);
        }

        public AudioMixerGroup GetMixerGroup(MixerType type)
            => audioMixer.FindMatchingGroups(type.ToString()).FirstOrDefault();
    }
    
    public enum MixerType
    {
        Master,
        Music,
        Sfx,
        Voice
    }
}
