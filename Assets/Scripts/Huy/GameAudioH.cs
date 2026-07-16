using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAudioH : MonoBehaviour
{
    // Folder path used by Resources.Load. Unity starts inside any folder named Resources.
    // Example: Resources.Load<AudioClip>("Audio/Huy/Space") loads Assets/Resources/Audio/Huy/Space.wav.
    const string AudioPath = "Audio/Huy/";

    // Stores the one active GameAudioH in the scene so static functions can find it.
    // This script does NOT create itself automatically; you must add this component to a GameObject.
    static GameAudioH instance;

    [Header("Music")]
    [SerializeField] AudioClip gameplayMusic;
    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField] AudioClip bossSceneMusic;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.1f;

    [Header("SFX")]
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip buttonClickClip;
    [SerializeField] AudioClip bossFireDeathClip;
    [SerializeField] AudioClip skillDashClip;
    [SerializeField] AudioClip sceneMoveClip;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 0.85f;

    AudioSource musicSource; // Plays looping background music.
    AudioSource sfxSource; // Plays short sounds like jump, hit, button click.

    // Remember the previous health number for each HealthManager object.
    // If current health becomes smaller than previous health, we know the player was hit.
    readonly Dictionary<Component, int> lastHealthByComponent = new Dictionary<Component, int>();

    // Stores buttons that already received the click-sound listener.
    // This stops the script from adding the same click sound many times.
    readonly HashSet<Button> wiredButtons = new HashSet<Button>();

    GameManager trackedGameManager; // The GameManager currently being watched for game over.
    bool wasGameOver; // Previous frame's game-over state.
    bool hasSeenBossInBossFireScene; // Becomes true after the boss exists at least once.
    bool bossFireDeathPlayed; // Stops the boss death sound from repeating every frame.

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // If the scene accidentally has two GameAudioH objects, keep the first and remove this one.
            Destroy(gameObject);
            return;
        }

        // Register this component as the active audio manager.
        instance = this;

        LoadClips();
        CreateAudioSources();

        // Run OnSceneLoaded whenever Unity loads another scene.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start the correct music immediately for the current scene.
        SetupSceneAudio(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        if (instance != this)
            return;

        // Unsubscribe from scene load events so Unity does not call a destroyed object.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    void Update()
    {
        HandleInputSounds();

        // These watchers keep sound working without editing teammate scripts.
        WatchHealthDamage();
        WatchGameOver();
        WatchBossFireDeath();
        WireButtonClickSounds();
    }

    public static void PlayJump()
    {
        // Lets other scripts call GameAudioH.PlayJump() without needing a direct reference.
        PlaySharedOneShot(audio => audio.jumpClip);
    }

    public static void PlayHit()
    {
        PlaySharedOneShot(audio => audio.hitClip);
    }

    public static void PlayDeath()
    {
        PlaySharedOneShot(audio => audio.deathClip);
    }

    public static void PlayButtonClick()
    {
        PlaySharedOneShot(audio => audio.buttonClickClip);
    }

    public static void PlaySkillDash()
    {
        PlaySharedOneShot(audio => audio.skillDashClip);
    }

    public static void PlaySceneMove()
    {
        PlaySharedOneShot(audio => audio.sceneMoveClip);
    }

    public static float GetSceneMoveDuration(float fallbackDuration = 0.25f)
    {
        GameAudioH audio = FindInstance();

        if (audio == null || audio.sceneMoveClip == null)
            return fallbackDuration;

        return audio.sceneMoveClip.length;
    }

    static void PlaySharedOneShot(System.Func<GameAudioH, AudioClip> clipSelector)
    {
        GameAudioH audio = FindInstance();

        // If the scene has no GameAudioH object, do nothing instead of throwing an error.
        if (audio != null)
            audio.PlayOneShot(clipSelector(audio));
    }

    static GameAudioH FindInstance()
    {
        if (instance != null)
            return instance;

        // Look for a manually placed GameAudioH component, including inactive objects.
        instance = FindAnyObjectByType<GameAudioH>(FindObjectsInactive.Include);
        return instance;
    }

    void LoadClips()
    {
        // Inspector-assigned clips are kept; missing clips are loaded by filename.
        if (gameplayMusic == null)
        {
            // Loads Assets/Resources/Audio/Huy/GamePlay_BG.wav.
            gameplayMusic = Resources.Load<AudioClip>(AudioPath + "GamePlay_BG");
        }

        if (mainMenuMusic == null)
        {
            // Loads Assets/Resources/Audio/Huy/MainMenu_BG.wav.
            mainMenuMusic = Resources.Load<AudioClip>(AudioPath + "MainMenu_BG");
        }

        if (bossSceneMusic == null)
        {
            // Loads Assets/Resources/Audio/Huy/BossScene.wav.
            bossSceneMusic = Resources.Load<AudioClip>(AudioPath + "BossScene");
        }

        if (jumpClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/Space.wav.
            jumpClip = Resources.Load<AudioClip>(AudioPath + "Space");
        }

        if (hitClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/Hit.wav.
            hitClip = Resources.Load<AudioClip>(AudioPath + "Hit");
        }

        if (deathClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/Death.wav.
            deathClip = Resources.Load<AudioClip>(AudioPath + "Death");
        }

        if (buttonClickClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/ButonClick.wav.
            buttonClickClip = Resources.Load<AudioClip>(AudioPath + "ButonClick");
        }

        if (bossFireDeathClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/BossFireDeath.wav.
            bossFireDeathClip = Resources.Load<AudioClip>(AudioPath + "BossFireDeath");
        }

        if (skillDashClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/SkillDash.wav.
            skillDashClip = Resources.Load<AudioClip>(AudioPath + "SkillDash");
        }

        if (sceneMoveClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/SceneMove.wav.
            sceneMoveClip = Resources.Load<AudioClip>(AudioPath + "SceneMove");
        }
    }

    void CreateAudioSources()
    {
        // Music uses its own AudioSource so it can loop separately from SFX.
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true; // Background music repeats forever.
        musicSource.playOnAwake = false; // Music starts only when PlayMusic chooses a clip.
        musicSource.ignoreListenerPause = true; // Audio still works if gameplay uses pause/time stop.
        musicSource.volume = musicVolume; // Uses the Music Volume slider in Inspector.

        // SFX uses a second AudioSource so short sounds can overlap the music.
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false; // SFX starts only when PlayOneShot is called.
        sfxSource.ignoreListenerPause = true; // Button/death sounds can still play while game is paused.
        sfxSource.volume = sfxVolume; // Uses the SFX Volume slider in Inspector.
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetWatcherState();
        SetupSceneAudio(scene);
    }

    void ResetWatcherState()
    {
        // Scene changes/retry need fresh watcher state so one-shot sounds can play again.
        lastHealthByComponent.Clear();
        wiredButtons.Clear();
        trackedGameManager = null;
        wasGameOver = false;
        hasSeenBossInBossFireScene = false;
        bossFireDeathPlayed = false;
    }

    void SetupSceneAudio(Scene scene)
    {
        // BossFireScenes gets boss music, menu scenes get menu music, all others get gameplay music.
        if (IsBossFireScene(scene))
        {
            // BossFireScenes has its own boss background music.
            PlayMusic(bossSceneMusic);
        }
        else if (IsMenuScene(scene))
        {
            // Menu / UI scenes use menu music.
            PlayMusic(mainMenuMusic);
        }
        else
        {
            // Every other non-menu scene uses normal gameplay music.
            PlayMusic(gameplayMusic);
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        // Avoid restarting the same music every frame/scene check.
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        // Put the selected AudioClip into the music source and start playing it.
        musicSource.clip = clip;
        musicSource.Play();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    void HandleInputSounds()
    {
        if (Keyboard.current == null || !IsGameplayScene(SceneManager.GetActiveScene()))
            return;

        // Space jump sound, only outside menu scenes.
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // wasPressedThisFrame means the sound plays once, not every frame while held.
            PlayOneShot(jumpClip);
        }

        // F skill sound, only outside menu scenes.
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            // Plays dash/skill sound when the player presses F.
            PlayOneShot(skillDashClip);
        }
    }

    void WatchHealthDamage()
    {
        // Watches HealthManager / HealthManagerH currentHealth with reflection.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            // Skip every script except HealthManager and HealthManagerH.
            if (!IsHealthManager(behaviour))
                continue;

            // If this health script does not have an int currentHealth field, skip it.
            if (!TryGetCurrentHealth(behaviour, out int currentHealth))
                continue;

            // Health decreased but player is still alive, so play hit sound.
            if (lastHealthByComponent.TryGetValue(behaviour, out int previousHealth) && currentHealth < previousHealth && currentHealth > 0)
                PlayOneShot(hitClip);

            lastHealthByComponent[behaviour] = currentHealth;
        }
    }

    void WatchGameOver()
    {
        // Detects GameManager game-over state and plays death SFX once per run.
        GameManager currentGameManager = FindAnyObjectByType<GameManager>();

        if (currentGameManager == null)
            return;

        if (currentGameManager != trackedGameManager)
        {
            // A new/reloaded scene can create a new GameManager, so reset the old game-over memory.
            trackedGameManager = currentGameManager;
            wasGameOver = false;
        }

        // Read the current game-over state from GameManager.
        bool isGameOver = trackedGameManager.IsGameOver();
        if (isGameOver && !wasGameOver)
        {
            // This block only runs on the frame when game over changes from false to true.
            PlayOneShot(deathClip);
            PlayMusic(gameplayMusic);
        }

        // Store this frame's value so next frame can compare against it.
        wasGameOver = isGameOver;
    }

    void WatchBossFireDeath()
    {
        // Only BossFireScenes should use the BossFireDeath sound rule.
        if (!IsBossFireScene(SceneManager.GetActiveScene()))
            return;

        bool bossAlive = FindBossObject() != null;

        if (bossAlive)
        {
            // Wait until a boss has existed before listening for its death.
            hasSeenBossInBossFireScene = true;
            return;
        }

        if (hasSeenBossInBossFireScene && !bossFireDeathPlayed)
        {
            bossFireDeathPlayed = true;
            PlayOneShot(bossFireDeathClip);
        }
    }

    void WireButtonClickSounds()
    {
        // Finds UI Buttons, including inactive panels, and adds click SFX once.
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button == null || wiredButtons.Contains(button))
                continue;

            // Add PlayButtonClick to the button's OnClick list.
            button.onClick.AddListener(PlayButtonClick);

            // Mark this button as done so the listener is not added again next Update.
            wiredButtons.Add(button);
        }
    }

    GameObject FindBossObject()
    {
        // Prefer boss tags because they are cheap and match the scene setup.
        string[] bossTags = { "Boss", "Boss2" };

        foreach (string bossTag in bossTags)
        {
            try
            {
                // FindGameObjectWithTag throws an error if the tag does not exist, so it is inside try/catch.
                GameObject boss = GameObject.FindGameObjectWithTag(bossTag);
                if (boss != null && boss.activeInHierarchy)
                    return boss;
            }
            catch (UnityException)
            {
            }
        }

        // Fallback for boss objects found by script name instead of tag.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName == "BossAI" || typeName == "BossAI2")
                return behaviour.gameObject;
        }

        return null;
    }

    bool TryGetCurrentHealth(MonoBehaviour behaviour, out int currentHealth)
    {
        currentHealth = 0;

        // currentHealth is private in the health scripts, so reflection reads it safely.
        FieldInfo field = behaviour.GetType().GetField("currentHealth", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // Only accept the field if it exists and is an integer.
        if (field == null || field.FieldType != typeof(int))
            return false;

        // Read the private currentHealth value from the HealthManager object.
        currentHealth = (int)field.GetValue(behaviour);
        return true;
    }

    bool IsHealthManager(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        string typeName = behaviour.GetType().Name;
        return typeName == "HealthManager" || typeName == "HealthManagerH";
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

    bool IsBossFireScene(Scene scene)
    {
        return scene.name == "BossFireScenes";
    }
}
