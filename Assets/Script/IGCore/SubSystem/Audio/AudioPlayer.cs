using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] bool playOnEnabled = true;
    [SerializeField] float delay = 0.1f;
    [SerializeField] AudioSource audioSource;
    [SerializeField] bool isLoop = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {}

    private void OnEnable()
    {
        if(playOnEnabled)   Play();
    }

    private void OnDisable()
    {
        if(audioSource != null && audioSource.clip != null)
        {
            if(audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    public void Play()
    {
        StartCoroutine(coPlay());
    }

    IEnumerator coPlay()
    {
        yield return new WaitForSeconds(delay);

        if(audioSource != null && audioSource.clip != null)
        {
            audioSource.loop = isLoop;
            audioSource.Play();
        }
    }
}
