using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource introSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip introClip;
    [SerializeField] AudioClip musicClip;
    [SerializeField] float volume;

    private static MusicManager _instance;

    #region Singleton
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<MusicManager>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("MusicManager");
                    _instance = singletonObject.AddComponent<MusicManager>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        // If the instance is already set, destroy this duplicate object
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
    }
    #endregion

    private void Start()
    {
        StartMusic(volume);
    }
    public void StartMusic(float volume)
    {
        /*
        double startTime = AudioSettings.dspTime + 0.1f;

        introSource.clip = introClip;
        introSource.volume = volume;
        introSource.loop = false;
        introSource.PlayScheduled(startTime);

        double introEndTime = startTime + introClip.length;
        */

        musicSource.clip = musicClip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }
    public void StopMusic(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutMusic(fadeDuration));
    }
    public IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / duration;
            if (introSource != null)
            introSource.volume = musicSource.volume;
            yield return null;
        }

        musicSource.Stop();
        if (introSource != null)
        introSource.Stop();
    }
}