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
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.1f;

    [Header("SFX")]
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip buttonClickClip;
    [SerializeField] AudioClip bossFireDeathClip;
    [SerializeField] AudioClip bossChargeClip;
    [SerializeField] AudioClip bossLaserFireClip;
    [SerializeField] AudioClip fireRainCastClip;
    [SerializeField] AudioClip fireRainDropClip;
    [SerializeField] AudioClip cannonFireClip;
    [SerializeField] AudioClip bombExplodeClip;
    [SerializeField] AudioClip skillDashClip;
    [SerializeField] AudioClip sceneMoveClip;
    [SerializeField] AudioClip iceBreakClip;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 0.85f;

    [Header("SFX Volumes")]
    [SerializeField, Range(0f, 1f)] float jumpVolume = 1f;
    [SerializeField, Range(0f, 1f)] float hitVolume = 1f;
    [SerializeField, Range(0f, 1f)] float deathVolume = 1f;
    [SerializeField, Range(0f, 1f)] float buttonClickVolume = 1f;
    [SerializeField, Range(0f, 1f)] float bossFireDeathVolume = 1f;
    [SerializeField, Range(0f, 1f)] float bossChargeVolume = 1f;
    [SerializeField, Range(0f, 1f)] float bossLaserFireVolume = 1f;
    [SerializeField, Range(0f, 1f)] float fireRainCastVolume = 1f;
    [SerializeField, Range(0f, 1f)] float fireRainDropVolume = 1f;
    [SerializeField, Range(0f, 1f)] float cannonFireVolume = 1f;
    [SerializeField, Range(0f, 1f)] float bombExplodeVolume = 1f;
    [SerializeField, Range(0f, 1f)] float skillDashVolume = 1f;
    [SerializeField, Range(0f, 1f)] float sceneMoveVolume = 1f;
    [SerializeField, Range(0f, 1f)] float iceBreakVolume = 1f;

    AudioSource musicSource; // Plays looping background music.
    AudioSource sfxSource; // Plays short sounds like jump, hit, button click.

    // Remember the previous health number for each HealthManager object.
    // If current health becomes smaller than previous health, we know the player was hit.
    readonly Dictionary<Component, int> lastHealthByComponent = new Dictionary<Component, int>();

    // Remembers whether each ice break trap was already broken last frame.
    readonly Dictionary<Component, bool> iceTrapBrokenState = new Dictionary<Component, bool>();

    // Remembers the previous dash state for each LoiPlayer so dash SFX plays only when a dash starts.
    readonly Dictionary<Component, bool> lastDashStateByComponent = new Dictionary<Component, bool>();

    // Stores fireballs that were already seen so each falling fireball gets one drop sound.
    readonly HashSet<int> seenFireballIds = new HashSet<int>();

    // Stores cannon bullets that were already seen so each fired bullet gets one shot sound.
    readonly HashSet<int> seenCannonBulletIds = new HashSet<int>();

    // Remembers the previous explode state for each BombTrap so the explosion sound plays once.
    readonly Dictionary<Component, bool> lastBombExplodeState = new Dictionary<Component, bool>();

    // Stores buttons that already received the click-sound listener.
    // This stops the script from adding the same click sound many times.
    readonly HashSet<Button> wiredButtons = new HashSet<Button>();

    GameManager trackedGameManager; // The GameManager currently being watched for game over.
    bool wasGameOver; // Previous frame's game-over state.
    bool hasSeenBossInBossFireScene; // Becomes true after the boss exists at least once.
    bool bossFireDeathPlayed; // Stops the boss death sound from repeating every frame.
    bool wasBossPreviewLaserActive; // Previous frame's PreviewLaser state, used to play charge SFX once.
    bool wasBossMainLaserActive; // Previous frame's MainLaser state, used to play laser SFX once.
    float lastFireRainDropTime = -999f; // Used to know when a new Fire Rain sequence begins.

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
        WatchDashSkillStart();
        WatchCannonBullets();
        WatchBombExplosions();
        WatchIceBreakTraps();
        WatchGameOver();
        WatchBossFireLaser();
        WatchBossFireRain();
        WatchBossFireDeath();
        WireButtonClickSounds();
    }

    public static void PlayJump()
    {
        // Lets other scripts call GameAudioH.PlayJump() without needing a direct reference.
        PlaySharedOneShot(audio => audio.jumpClip, audio => audio.jumpVolume);
    }

    public static void PlayHit()
    {
        PlaySharedOneShot(audio => audio.hitClip, audio => audio.hitVolume);
    }

    public static void PlayDeath()
    {
        PlaySharedOneShot(audio => audio.deathClip, audio => audio.deathVolume);
    }

    public static void PlayButtonClick()
    {
        PlaySharedOneShot(audio => audio.buttonClickClip, audio => audio.buttonClickVolume);
    }

    public static void PlaySkillDash()
    {
        PlaySharedOneShot(audio => audio.skillDashClip, audio => audio.skillDashVolume);
    }

    public static void PlaySceneMove()
    {
        PlaySharedOneShot(audio => audio.sceneMoveClip, audio => audio.sceneMoveVolume);
    }

    public static void PlayIceBreak()
    {
        PlaySharedOneShot(audio => audio.iceBreakClip, audio => audio.iceBreakVolume);
    }

    public static float GetSceneMoveDuration(float fallbackDuration = 0.25f)
    {
        GameAudioH audio = FindInstance();

        if (audio == null || audio.sceneMoveClip == null)
            return fallbackDuration;

        return audio.sceneMoveClip.length;
    }

    static void PlaySharedOneShot(System.Func<GameAudioH, AudioClip> clipSelector, System.Func<GameAudioH, float> volumeSelector)
    {
        GameAudioH audio = FindInstance();

        // If the scene has no GameAudioH object, do nothing instead of throwing an error.
        if (audio != null)
            audio.PlayOneShot(clipSelector(audio), volumeSelector(audio));
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

        if (bossChargeClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/BossCharge.wav.
            bossChargeClip = Resources.Load<AudioClip>(AudioPath + "BossCharge");
        }

        if (bossLaserFireClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/BossLaserFire.wav.
            bossLaserFireClip = Resources.Load<AudioClip>(AudioPath + "BossLaserFire");
        }

        if (fireRainCastClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/FireRainCast.wav.
            fireRainCastClip = Resources.Load<AudioClip>(AudioPath + "FireRainCast");
        }

        if (fireRainDropClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/FireRainDrop.wav.
            fireRainDropClip = Resources.Load<AudioClip>(AudioPath + "FireRainDrop");
        }

        if (cannonFireClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/CannonFire.wav.
            cannonFireClip = Resources.Load<AudioClip>(AudioPath + "CannonFire");
        }

        if (bombExplodeClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/BombExplode.wav.
            bombExplodeClip = Resources.Load<AudioClip>(AudioPath + "BombExplode");
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

        if (iceBreakClip == null)
        {
            // Loads Assets/Resources/Audio/Huy/IceBreak.wav.
            iceBreakClip = Resources.Load<AudioClip>(AudioPath + "IceBreak");
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
        iceTrapBrokenState.Clear();
        lastDashStateByComponent.Clear();
        seenFireballIds.Clear();
        seenCannonBulletIds.Clear();
        lastBombExplodeState.Clear();
        wiredButtons.Clear();
        trackedGameManager = null;
        wasGameOver = false;
        hasSeenBossInBossFireScene = false;
        bossFireDeathPlayed = false;
        wasBossPreviewLaserActive = false;
        wasBossMainLaserActive = false;
        lastFireRainDropTime = -999f;
    }

    void SetupSceneAudio(Scene scene)
    {
        // Menu scenes use main menu music; every other scene uses the prefab's Gameplay Music clip.
        if (IsMenuScene(scene))
        {
            // Menu / UI scenes use menu music.
            PlayMusic(mainMenuMusic);
        }
        else
        {
            // Scene-specific prefabs can swap this one field to use a different background track.
            PlayMusic(gameplayMusic);
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        // Music Volume controls whichever background track this prefab is using.
        musicSource.volume = musicVolume;

        // Avoid restarting the same music every frame/scene check.
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        // Put the selected AudioClip into the music source and start playing it.
        musicSource.clip = clip;
        musicSource.Play();
    }

    void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, volumeScale);
    }

    void HandleInputSounds()
    {
        if (Keyboard.current == null || !IsGameplayScene(SceneManager.GetActiveScene()))
            return;

        // Space jump sound, only outside menu scenes.
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // wasPressedThisFrame means the sound plays once, not every frame while held.
            PlayOneShot(jumpClip, jumpVolume);
        }

        // Dash sound is handled by WatchDashSkillStart so cooldown key presses stay silent.
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
                PlayOneShot(hitClip, hitVolume);

            lastHealthByComponent[behaviour] = currentHealth;
        }
    }

    void WatchDashSkillStart()
    {
        // Watches LoiPlayer.isDashing from outside, so cooldown rules stay owned by the player script.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (!IsLoiPlayer(behaviour))
                continue;

            if (!TryGetBoolField(behaviour, "isDashing", out bool isDashing))
                continue;

            // Play only on the frame the player actually enters dash.
            if (lastDashStateByComponent.TryGetValue(behaviour, out bool wasDashing) && !wasDashing && isDashing)
                PlayOneShot(skillDashClip, skillDashVolume);

            lastDashStateByComponent[behaviour] = isDashing;
        }
    }

    void WatchCannonBullets()
    {
        // Watches cannon bullets after CannonShooter spawns them, without editing the trap script.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (!IsCannonBullet(behaviour))
                continue;

            int bulletId = behaviour.gameObject.GetInstanceID();
            if (seenCannonBulletIds.Add(bulletId))
                PlayOneShot(cannonFireClip, cannonFireVolume);
        }
    }

    void WatchBombExplosions()
    {
        // Watches BombTrap.hasExploded after player collision, without editing the bomb script.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (!IsBombTrap(behaviour))
                continue;

            if (!TryGetBoolField(behaviour, "hasExploded", out bool hasExploded))
                continue;

            // Play only on the frame the bomb changes from idle to exploded.
            bool wasTracked = lastBombExplodeState.TryGetValue(behaviour, out bool wasExploded);

            if (hasExploded && (!wasTracked || !wasExploded))
                PlayOneShot(bombExplodeClip, bombExplodeVolume);

            lastBombExplodeState[behaviour] = hasExploded;
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
            StopMusic();
            PlayOneShot(deathClip, deathVolume);
        }

        // Store this frame's value so next frame can compare against it.
        wasGameOver = isGameOver;
    }

    void WatchIceBreakTraps()
    {
        // Watches IceLayerHitTrap from outside, so the teammate trap script does not need audio code.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (!IsIceLayerHitTrap(behaviour))
                continue;

            BoxCollider2D iceCollider = behaviour.GetComponent<BoxCollider2D>();
            if (iceCollider == null)
                continue;

            bool isBroken = !iceCollider.enabled;

            if (iceTrapBrokenState.TryGetValue(behaviour, out bool wasBroken) && !wasBroken && isBroken)
                PlayOneShot(iceBreakClip, iceBreakVolume);

            iceTrapBrokenState[behaviour] = isBroken;
        }
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
            PlayOneShot(bossFireDeathClip, bossFireDeathVolume);
        }
    }

    void WatchBossFireLaser()
    {
        // Boss scenes with PreviewLaser/MainLaser use the same charge and fire sound timing.
        if (!IsBossSkillScene(SceneManager.GetActiveScene()))
            return;

        GameObject boss = FindBossObject();
        if (boss == null)
        {
            wasBossPreviewLaserActive = false;
            wasBossMainLaserActive = false;
            return;
        }

        // PreviewLaser turns on at the start of the 0.8 second charge.
        LineRenderer previewLaser = FindChildComponentByName<LineRenderer>(boss.transform, "PreviewLaser");
        bool isPreviewLaserActive = previewLaser != null && previewLaser.enabled && previewLaser.gameObject.activeInHierarchy;

        // Play only on the frame PreviewLaser changes from off to on.
        if (isPreviewLaserActive && !wasBossPreviewLaserActive)
            PlayOneShot(bossChargeClip, bossChargeVolume);

        wasBossPreviewLaserActive = isPreviewLaserActive;

        // MainLaser turns on after the 0.8 second preview charge in the boss skill coroutine.
        LineRenderer mainLaser = FindChildComponentByName<LineRenderer>(boss.transform, "MainLaser");
        bool isMainLaserActive = mainLaser != null && mainLaser.enabled && mainLaser.gameObject.activeInHierarchy;

        // Play only on the frame MainLaser changes from off to on.
        if (isMainLaserActive && !wasBossMainLaserActive)
            PlayOneShot(bossLaserFireClip, bossLaserFireVolume);

        wasBossMainLaserActive = isMainLaserActive;
    }

    void WatchBossFireRain()
    {
        // Boss scenes that spawn FireBall objects use the same cast/drop sound timing.
        if (!IsBossSkillScene(SceneManager.GetActiveScene()))
            return;

        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (!IsFireBall(behaviour))
                continue;

            int fireballId = behaviour.gameObject.GetInstanceID();
            if (!seenFireballIds.Add(fireballId))
                continue;

            // A new rain sequence starts when the next fireball appears after the previous sequence gap.
            if (Time.time - lastFireRainDropTime > 1f)
                PlayOneShot(fireRainCastClip, fireRainCastVolume);

            // Every new fireball gets a falling/drop sound.
            PlayOneShot(fireRainDropClip, fireRainDropVolume);
            lastFireRainDropTime = Time.time;
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
        // Prefer the root boss script because some child sprite parts also use the Boss tag.
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName == "BossAI" || typeName == "BossAI2")
                return behaviour.gameObject;
        }

        // Fallback to boss tags if the script cannot be found.
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

        return null;
    }

    T FindChildComponentByName<T>(Transform parent, string childName) where T : Component
    {
        if (parent == null)
            return null;

        // Check the current Transform first, then search every child recursively.
        if (parent.name == childName && parent.TryGetComponent(out T component))
            return component;

        foreach (Transform child in parent)
        {
            T foundComponent = FindChildComponentByName<T>(child, childName);
            if (foundComponent != null)
                return foundComponent;
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

    bool TryGetBoolField(MonoBehaviour behaviour, string fieldName, out bool value)
    {
        value = false;

        // isDashing is private in LoiPlayer, so reflection reads it without changing teammate code.
        FieldInfo field = behaviour.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (field == null || field.FieldType != typeof(bool))
            return false;

        value = (bool)field.GetValue(behaviour);
        return true;
    }

    bool IsHealthManager(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        string typeName = behaviour.GetType().Name;
        return typeName == "HealthManager" || typeName == "HealthManagerH";
    }

    bool IsIceLayerHitTrap(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        return behaviour.GetType().Name == "IceLayerHitTrap";
    }

    bool IsLoiPlayer(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        return behaviour.GetType().Name == "LoiPlayer";
    }

    bool IsFireBall(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        return behaviour.GetType().Name == "FireBall";
    }

    bool IsCannonBullet(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        return behaviour.GetType().Name == "CannonBullet";
    }

    bool IsBombTrap(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        return behaviour.GetType().Name == "BombTrap";
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
        return scene.name.Contains("BossFireScenes");
    }

    bool IsBossSkillScene(Scene scene)
    {
        return scene.name.Contains("BossFireScenes") || scene.name.Contains("BossGolemScene");
    }
}
