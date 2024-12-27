using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Multicraft.Scripts.Utils
{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusicPlayer : MonoBehaviour
    {
        public static BackgroundMusicPlayer Instance;

        private AudioSource audioSource;

        [SerializeField] private List<AudioClip> _backgroundMusics;
        [SerializeField] private float minInterval = 60f; // Minimum interval between music changes
        [SerializeField] private float maxInterval = 240f; // Maximum interval between music changes

        private void Awake()
        {
            Initialization();
        }

        public void Initialization()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            audioSource = gameObject.GetComponent<AudioSource>();

            StartCoroutine(PlayMusicCoroutine());
        }

        public IEnumerator PlayMusicCoroutine()
        {
            while (true)
            {
                if (_backgroundMusics.Count > 0)
                {
                    int randomIndex = Random.Range(0, _backgroundMusics.Count);
                    AudioClip clipToPlay = _backgroundMusics[randomIndex];
                    audioSource.clip = clipToPlay;
                    audioSource.Play();

                    float randomInterval = Random.Range(minInterval, maxInterval);
                    yield return
                        new WaitForSeconds(clipToPlay.length +
                                           randomInterval); // Wait until the clip finishes and then add a random interval
                }
                else
                {
                    Debug.LogError("No background music clips assigned to BackgroundMusicManager!");
                    yield return new WaitForSeconds(5f); // Wait before checking again
                }
            }
        }
    }
}