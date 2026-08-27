using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zlipacket.Core.Audio
{
    public class AudioSlider : MonoBehaviour, IPointerUpHandler
    {
        private Slider slider;
             
        public MixerType mixerType = MixerType.Master;
        
        public AudioClip sliderUpSound;
        
        private AudioManager manager => AudioManager.Instance;
        
        private void Awake()
        {
            slider = GetComponent<Slider>();
        }

        private void Start()
        {
            if (slider == null)
                return;
            
            slider.value = manager.GetVolume(mixerType);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            manager.SetVolume(mixerType, value);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            /*if (sliderUpSound != null)
                SfxManager.Instance.PlaySoundFX(sliderUpSound);*/
        }
    }
}