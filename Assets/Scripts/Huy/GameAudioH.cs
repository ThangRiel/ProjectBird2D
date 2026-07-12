using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAudioH : MonoBehaviour
{
    const string AudioPath = "Audio/Huy/";

    static GameAudioH instance;

    AudioSource musicSource;
    AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] AudioClip gameplayMusic;
    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.45f;

    [Header("SFX")]
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip buttonClickClip;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 0.85f;

    readonly Dictionary<Component, int> lastHealthByComponent = new Dictionary<Component, int>();
    readonly HashSet<Button> wiredButtons = new HashSet<Button>();

    GameManager trackedGameManager;
    bool wasGameOver;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void PlayJump()
    {
        GameAudioH audio = EnsureInstance();
        audio.PlayOneShot(audio.jumpClip);
    }

    public static void PlayHit()
    {
        GameAudioH audio = EnsureInstance();
        audio.PlayOneShot(audio.hitClip);
    }

    public static void PlayDeath()
    {
        GameAudioH audio = EnsureInstance();
        audio.PlayOneShot(audio.deathClip);
    }

    public static void PlayButtonClick()
    {
        GameAudioH audio = EnsureInstance();
        audio.PlayOneShot(audio.buttonClickClip);
    }

    static GameAudioH EnsureInstance()
    {
        if (instance != null)
            return instance;

        GameObject audioObject = new GameObject("GameAudioH");
        instance = audioObject.AddComponent<GameAudioH>();
        DontDestroyOnLoad(audioObject);
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.ignoreListenerPause = true;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.ignoreListenerPause = true;
        sfxSource.volume = sfxVolume;

        LoadClips();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetupSceneAudio(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && IsGameplayScene(SceneManager.GetActiveScene()))
            PlayOneShot(jumpClip);

        WatchHealthDamage();
        WatchGameOver();
        WireButtonClickSounds();
    }

    void LoadClips()
    {
        if (jumpClip == null)
            jumpClip = Resources.Load<AudioClip>(AudioPath + "Space");

        if (hitClip == null)
            hitClip = Resources.Load<AudioClip>(AudioPath + "Hit");

        if (deathClip == null)
            deathClip = Resources.Load<AudioClip>(AudioPath + "Death");

        if (buttonClickClip == null)
            buttonClickClip = Resources.Load<AudioClip>(AudioPath + "ButonClick");

        if (gameplayMusic == null)
            gameplayMusic = Resources.Load<AudioClip>(AudioPath + "GamePlay_BG");

        if (mainMenuMusic == null)
            mainMenuMusic = Resources.Load<AudioClip>(AudioPath + "MainMenu_BG");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastHealthByComponent.Clear();
        wiredButtons.Clear();
        trackedGameManager = null;
        wasGameOver = false;
        SetupSceneAudio(scene);
    }

    void SetupSceneAudio(Scene scene)
    {
        AudioClip targetMusic = IsMenuScene(scene) ? mainMenuMusic : gameplayMusic;
        PlayMusic(targetMusic);
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    void WatchGameOver()
    {
        GameManager currentGameManager = FindAnyObjectByType<GameManager>();

        if (currentGameManager == null)
            return;

        if (currentGameManager != trackedGameManager)
        {
            trackedGameManager = currentGameManager;
            wasGameOver = false;
        }

        bool isGameOver = trackedGameManager.IsGameOver();
        if (isGameOver && !wasGameOver)
        {
            PlayOneShot(deathClip);
            PlayMusic(gameplayMusic);
        }

        wasGameOver = isGameOver;
    }

    void WatchHealthDamage()
    {
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName != "HealthManager" && typeName != "HealthManagerH")
                continue;

            if (!TryGetCurrentHealth(behaviour, out int currentHealth))
                continue;

            if (lastHealthByComponent.TryGetValue(behaviour, out int previousHealth) && currentHealth < previousHealth && currentHealth > 0)
                PlayOneShot(hitClip);

            lastHealthByComponent[behaviour] = currentHealth;
        }
    }

    void WireButtonClickSounds()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button == null || wiredButtons.Contains(button))
                continue;

            button.onClick.AddListener(PlayButtonClick);
            wiredButtons.Add(button);
        }
    }

    bool TryGetCurrentHealth(MonoBehaviour behaviour, out int currentHealth)
    {
        currentHealth = 0;
        FieldInfo field = behaviour.GetType().GetField("currentHealth", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (field == null || field.FieldType != typeof(int))
            return false;

        currentHealth = (int)field.GetValue(behaviour);
        return true;
    }

    bool IsMenuScene(Scene scene)
    {
        string sceneName = scene.name.ToLowerInvariant();
        return sceneName.Contains("menu") || sceneName == "ui" || sceneName.Contains("setting");
    }

    bool IsGameplayScene(Scene scene)
    {
        return !IsMenuScene(scene);
    }
}
