using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource deathSource;
    public AudioSource jumpAudioSource;
    public AudioSource pointAudioSource;
    public AudioSource[] clickSources;

    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static void PlayClick()
    {
        instance.clickSources[Random.Range(0, instance.clickSources.Length)].Play();
    }
}
