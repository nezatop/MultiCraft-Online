using System;
using UnityEngine;

namespace MultiCraft.Scripts.Utils
{
    [RequireComponent(typeof(AudioSource))]
    public class UISoundManager: MonoBehaviour
    {
        public static UISoundManager Instance { get; private set; }
        
        private AudioSource audioSource;
        
        public AudioClip ButtonClickSound;
        public AudioClip ButtonHoverSound;

        private void Awake()
        {
            Initialization();
        }

        public void Initialization()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        public void PlayButtonClickSound()
        {
            audioSource.PlayOneShot(ButtonClickSound);
        }

        public void PlayButtonHoverSound()
        {
            audioSource.PlayOneShot(ButtonHoverSound);
        }
        
    }
}