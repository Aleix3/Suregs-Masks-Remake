using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    public AudioMixer masterMixer;

    [Header("Audio Sources")]
    public AudioSource musicSourceA;
    public AudioSource musicSourceB;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip townMusic;
    public AudioClip dungeonMusic;
    public AudioClip goodEndingMusic;
    public AudioClip badEndingMusic;
    public AudioClip creditsMusic;

    [Header("SFX")]
        [Header("UI")]
    //UI
    public AudioClip buttonClip;
    public AudioClip buyItemClip;
    public AudioClip changeInventoryPageClip;
    public AudioClip changeInventoryWindowClip;
    public AudioClip dialogClip;
    public AudioClip dialogWriteClip;
    public AudioClip openInventoryClip;
    public AudioClip menuClip;
    public AudioClip selectItemClip;
    public AudioClip selectClip;
    public AudioClip sellItemClip;

    [Header("Player")]
    public AudioClip attackCombo1;
    public AudioClip attackCombo2;
    public AudioClip attackCombo3;
    public AudioClip dash;
    public AudioClip lowHealth;
    public AudioClip death;
    public AudioClip getDamage;
    public List<AudioClip> footStepsSFX;
    public AudioClip switchMask;
    public AudioClip usePotion;
    public AudioClip useMaskAbility;

    [Header("Extra")]
    public AudioClip chestOpen;
    public AudioClip getItem;
    public AudioClip pressPuzlleButton;


    public float musicMaxVolume;
    private AudioSource currentSource;
    private AudioSource nextSource;

    private Coroutine musicCoroutine;
    private int lastFootstepIndex = -1;

    private Dictionary<AudioClip, float> musicPositions = new Dictionary<AudioClip, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentSource = musicSourceA;
            nextSource = musicSourceB;

            currentSource.volume = 1f;
            nextSource.volume = 0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (currentSource.isPlaying && currentSource.clip == clip)
            return;

        if (currentSource.clip != null)
        {
            musicPositions[currentSource.clip] = currentSource.time;
        }

        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(CrossFadeMusic(clip, 1.5f));
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        float originalPitch = sfxSource.pitch;

        sfxSource.pitch = Random.Range(0.8f, 1.2f);
        sfxSource.PlayOneShot(clip);

        sfxSource.pitch = originalPitch;
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("[AudioManager] masterMixer no está asignado en el Inspector.");
            return;
        }

        float db = volume <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        masterMixer.SetFloat("MusicVolume", db);
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer == null)
        {
            Debug.LogWarning("[AudioManager] masterMixer no está asignado en el Inspector.");
            return;
        }

        float db = volume <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        masterMixer.SetFloat("SFXVolume", db);
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip, float duration)
    {
        nextSource.clip = newClip;

        if (musicPositions.TryGetValue(newClip, out float savedTime))
        {
            nextSource.time = Mathf.Clamp(savedTime, 0f, newClip.length - 0.1f);
        }
        else
        {
            nextSource.time = 0f;
        }

        nextSource.loop = true;
        nextSource.volume = 0f;
        nextSource.Play();

        float startVolume = currentSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, musicMaxVolume, t);

            yield return null;
        }

        currentSource.Stop();
        currentSource.volume = musicMaxVolume;
        nextSource.volume = musicMaxVolume;

        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        musicCoroutine = null;
    }

    public void PlayRandomFootstep()
    {
        if (footStepsSFX == null || footStepsSFX.Count == 0)
            return;

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, footStepsSFX.Count);
        }
        while (footStepsSFX.Count > 1 && randomIndex == lastFootstepIndex);

        lastFootstepIndex = randomIndex;

        float originalPitch = sfxSource.pitch;
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(footStepsSFX[randomIndex]);
        sfxSource.pitch = originalPitch;
    }

    public void StopMusic(float fadeDuration = 1.5f)
    {
        if (currentSource.clip == null)
            return;

        musicPositions[currentSource.clip] = currentSource.time;

        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(FadeOutMusic(fadeDuration));
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = currentSource.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            currentSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        currentSource.Stop();
        currentSource.clip = null;
        currentSource.volume = musicMaxVolume; // o el volumen por defecto que uses

        musicCoroutine = null;
    }
}