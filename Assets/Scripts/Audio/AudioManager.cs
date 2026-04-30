using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clip);
    }


    //randomize the pitch so it's not identical for repeating sfx
    public void PlaySFXRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        float randomPitch = Random.Range(minPitch, maxPitch);
        sfxSource.pitch = randomPitch;

        sfxSource.PlayOneShot(clip);

        sfxSource.pitch = 1f;
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}