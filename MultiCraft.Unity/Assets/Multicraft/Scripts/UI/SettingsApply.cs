using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MultiCraft.Scripts.UI
{
    public class SettingsApply : MonoBehaviour
    {
        [Header("Слайдеры")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider soundVolumeSlider;
        public Slider environmentVolumeSlider;

        [Header("Аудио Микшер")]
        public AudioMixer audioMixer;
        
        private const string MasterVolumeParam = "Master";
        private const string MusicVolumeParam = "Music";
        private const string SoundVolumeParam = "Sounds";
        private const string EnvironmentVolumeParam = "Enviroment";
        private void Start()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = GetMixerVolume(MasterVolumeParam);
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = GetMixerVolume(MusicVolumeParam);
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            
            if (soundVolumeSlider != null)
            {
                soundVolumeSlider.value = GetMixerVolume(SoundVolumeParam);
                soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
            }

            if (environmentVolumeSlider != null)
            {
                environmentVolumeSlider.value = GetMixerVolume(EnvironmentVolumeParam);
                environmentVolumeSlider.onValueChanged.AddListener(SetEnvironmentVolume);
            }
            
            
            masterVolumeSlider.value = 0.7f;
            musicVolumeSlider.value = 0.7f;
            soundVolumeSlider.value = 0.7f;
            environmentVolumeSlider.value = 0.7f;
        }
        private void SetMasterVolume(float volume)
        {
            SetMixerVolume(MasterVolumeParam, volume);
        }

        private void SetMusicVolume(float volume)
        {
            SetMixerVolume(MusicVolumeParam, volume);
        }

        private void SetSoundVolume(float volume)
        {
            SetMixerVolume(SoundVolumeParam, volume);
        }

        private void SetEnvironmentVolume(float volume)
        {
            SetMixerVolume(EnvironmentVolumeParam, volume);
        }
        
        private void SetMixerVolume(string parameter, float sliderValue)
        {
            // Преобразование значения слайдера (0.0 - 1.0) в децибелы (-80 дБ до 0 дБ)
            float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
            audioMixer.SetFloat(parameter, dB);
        }

        private float GetMixerVolume(string parameter)
        {
            if (audioMixer.GetFloat(parameter, out float value))
            {
                // Преобразование значения из децибел обратно в линейное (0.0 - 1.0)
                return Mathf.Pow(10, value / 20);
            }
            return 1.0f; // Если параметр не найден, вернуть громкость по умолчанию
        }

        private void OnDestroy()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);

            if (soundVolumeSlider != null)
                soundVolumeSlider.onValueChanged.RemoveListener(SetSoundVolume);
            
            if (environmentVolumeSlider != null)
                environmentVolumeSlider.onValueChanged.RemoveListener(SetEnvironmentVolume);
        }
    }
}