using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Runs the final boss sequence. Starting the reactor calls the Apex in, the three consoles then
/// become usable, and breaking the third one plays the ending and loads the credits.
/// </summary>
public class NuclearReactorInteract : MonoBehaviour, IInteractable<IInteractor>
{
    public static NuclearReactorInteract Instance { get; private set; }

    [SerializeField] private Image fadeOut;
    [SerializeField] private float fadeOutTime = 1.5f;

    [Header("Reactor start")]
    [SerializeField] private AudioLogObject startReactorLog;
    [SerializeField] private float waitAfterStartReactorLog = 5f;
    [SerializeField] private AudioLogObject apexSpawnsLog;
    [SerializeField] private float waitAfterApexSpawnsLog = 2f;

    [Header("Consoles, in the order they are broken")]
    [SerializeField] private AudioLogObject firstSwitchLog;
    [SerializeField] private AudioLogObject secondSwitchLog;
    [SerializeField] private AudioLogObject lastSwitchLog;

    [Header("Ending")]
    [Tooltip("Let the last switch line finish before the explosion, so the lines that follow do not cut it off")]
    [SerializeField] private bool waitForLastSwitchLog = true;
    [SerializeField] private float waitAfterExplosion = 2f;
    [SerializeField] private AudioLogObject apexDeathLog;
    [SerializeField] private float waitAfterApexDeathLog = 1f;
    [SerializeField] private AudioLogObject destroyOwnRadioLog;
    [SerializeField] private float waitAfterDestroyOwnRadioLog = 1f;
    [SerializeField] private float waitBeforeCredits = 2f;

    [Header("Music (FMOD music event names)")]
    [SerializeField] private string sceneMusicKey = "ForestAmbianceAfterVillageDestroyed";
    [SerializeField] private string bossMusicKey = "ApexFinalBoss";
    [SerializeField] private string endingMusicKey = "Monologue&Credits";
    [SerializeField] private float musicCrossfadeDuration = 2f;

    public static readonly string Explosion = "FinalBossExplosion";
    public static readonly string RadioDestroyed = "PlayerLandHeavy";
    private const string CreditsScene = "Credtis";
    // extra time allowed on top of a log's length before a wait gives up (logs pause for radio sounds)
    private const float LogTimeoutMargin = 10f;
    // longest a queued line waits for a line that is already playing
    private const float MaxLineWait = 25f;

    private ApexSpawnExternalEvent apexSpawner;
    private Apex spawnedApex;
    private bool reactorStarted;
    private bool apexSpawned;
    private bool endingStarted;
    private InteractableUI currentUi;
    private readonly System.Collections.Generic.Queue<AudioLogObject> lineQueue = new();
    private Coroutine lineRunner;
    private string pendingMusicKey;
    private Coroutine musicRunner;
    private bool musicRequested;

    /// <summary>The consoles can be broken from the moment the reactor is started until the ending starts.</summary>
    public bool ConsolesUsable => reactorStarted && !endingStarted;

    private void Awake()
    {
        Instance = this;
        // static counter survives scene reloads, so start every attempt at the boss from zero
        ConsoleInteract.consolesBroken = 0;
    }

    private void Start()
    {
        // The Apex only arrives through the reactor sequence here, so switch off the timer that would
        // otherwise spawn it a few seconds after the scene loads. The spawner itself stays usable.
        apexSpawner = FindFirstObjectByType<ApexSpawnExternalEvent>();
        if (apexSpawner != null)
        {
            foreach (var timer in apexSpawner.GetComponents<QuestEventBroadcaster>()) timer.enabled = false;
            foreach (var receiver in apexSpawner.GetComponents<QuestEventReceiver>()) receiver.enabled = false;
        }
        else
        {
            Debug.LogWarning("NuclearReactorInteract: no ApexSpawnExternalEvent in the scene, the Apex cannot be spawned.");
        }

        StartCoroutine(StartSceneMusic());
    }

    // Same steps MusicSceneLink uses to start a scene's music; this scene has no MusicSceneLink of its own
    private IEnumerator StartSceneMusic()
    {
        yield return new WaitUntil(() => FMODEvents.Instance != null && FMODEvents.Instance.Initialized);
        yield return null;
        if (MusicManager.Instance == null || string.IsNullOrEmpty(sceneMusicKey) || musicRequested) yield break;

        Debug.Log($"[BossSequence] Starting scene music '{sceneMusicKey}'.");
        MusicManager.Instance.SetCurTrack(sceneMusicKey);
        // a track that was crossfaded away earlier in the session is left at volume 0
        MusicManager.Instance.curTrack.setVolume(1f);
        MusicManager.Instance.PlayMusic(false);
    }

    // Crossfades to a music track. MusicManager ignores a crossfade asked for while another is running, so
    // the latest request is held until it can be started.
    private void RequestMusic(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        musicRequested = true;
        pendingMusicKey = key;
        if (musicRunner == null) musicRunner = StartCoroutine(RunMusicRequests());
    }

    private IEnumerator RunMusicRequests()
    {
        while (pendingMusicKey != null)
        {
            while (MusicManager.Instance != null && MusicManager.Instance.IsCrossFading) yield return null;
            if (MusicManager.Instance == null) break;

            string key = pendingMusicKey;
            pendingMusicKey = null;
            Debug.Log($"[BossSequence] Music changes to '{key}'.");
            MusicManager.Instance.CrossFadeMusic(key, musicCrossfadeDuration);
            yield return null;
        }
        musicRunner = null;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void OnHoverEnter(InteractableUI ui) {
        currentUi = ui;
        if (!reactorStarted)
            ui.ActivateUI(this);
    }
    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
        currentUi = null;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (reactorStarted) return;

        reactorStarted = true;
        // the reactor can only be started once, so take the prompt away as soon as it has been used
        if (currentUi) currentUi.DeactivateUI();
        Debug.Log("[BossSequence] Reactor started, consoles are now usable.");
        StartCoroutine(StartReactorSequence());
    }

    private IEnumerator StartReactorSequence()
    {
        // The consoles are usable from now on, so the player can break them while this is still running.
        // Lines are queued so they never cut each other off, and the ending may already have started
        // before the Apex is due (then the Apex is no longer needed).
        yield return PlayLogAndWait(startReactorLog);
        Debug.Log($"[BossSequence] Waiting {waitAfterStartReactorLog}s before the Apex spawns line.");
        yield return new WaitForSeconds(waitAfterStartReactorLog);
        if (endingStarted)
        {
            Debug.Log("[BossSequence] Ending already started, the rest of the reactor sequence is skipped.");
            yield break;
        }
        yield return PlayLogAndWait(apexSpawnsLog);
        Debug.Log($"[BossSequence] Waiting {waitAfterApexSpawnsLog}s before spawning the Apex.");
        yield return new WaitForSeconds(waitAfterApexSpawnsLog);
        if (endingStarted)
        {
            Debug.Log("[BossSequence] Ending already started, the Apex is not spawned.");
            yield break;
        }

        // A failure inside the spawner must not stop the sequence here, or the consoles would stay locked
        try
        {
            if (apexSpawner != null) apexSpawner.TriggerExternalEvent();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        spawnedApex = FindFirstObjectByType<Apex>();
        if (spawnedApex == null) Debug.LogWarning("[BossSequence] The Apex did not spawn.");
        apexSpawned = true;
        Debug.Log("[BossSequence] Apex spawned.");
    }

    /// <summary>Called by a console when it is broken. <paramref name="brokenCount"/> includes that console.</summary>
    public void OnConsoleBroken(int brokenCount)
    {
        Debug.Log($"[BossSequence] Console {brokenCount} of 3 broken.");
        AudioLogObject log = brokenCount == 1 ? firstSwitchLog : brokenCount == 2 ? secondSwitchLog : lastSwitchLog;
        QueueLine(log);

        if (brokenCount >= 3 && !endingStarted)
        {
            endingStarted = true;
            Debug.Log("[BossSequence] All consoles broken, ending started.");
            StartCoroutine(EndingSequence());
        }
    }

    private IEnumerator EndingSequence()
    {
        // nothing may kill the player once the ending has started
        if (PlayerID.Instance != null && PlayerID.Instance.playerHealth != null)
            PlayerID.Instance.playerHealth.SetInvincible(true);

        if (spawnedApex == null) spawnedApex = FindFirstObjectByType<Apex>();

        Vector3 apexStart = transform.position;
        if (spawnedApex != null)
        {
            // Stop the Apex thinking and moving by itself, then take over its position
            spawnedApex.enabled = false;
            var agent = spawnedApex.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false; // otherwise it pulls the Apex back onto its NavMesh
            var rb = spawnedApex.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
            }
            apexStart = spawnedApex.transform.position;
        }

        // Fade to black while the Apex is drawn to the reactor. The screen stays black until the credits.
        KeepSubtitlesAboveFade();
        // the ending track starts as the fade begins and carries on into the credits
        RequestMusic(endingMusicKey);
        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = fadeOutTime > 0f ? Mathf.Clamp01(elapsed / fadeOutTime) : 1f;
            if (fadeOut != null) fadeOut.color = new Color(0, 0, 0, t);
            if (spawnedApex != null) spawnedApex.transform.position = Vector3.Lerp(apexStart, transform.position, t);
            yield return null;
        }
        if (fadeOut != null) fadeOut.color = new Color(0, 0, 0, 1);
        if (spawnedApex != null) spawnedApex.transform.position = transform.position;

        Debug.Log("[BossSequence] Screen is black and the Apex is at the reactor.");

        FreezePlayer();

        if (waitForLastSwitchLog)
        {
            Debug.Log("[BossSequence] Waiting for the switch lines to finish.");
            while (lineRunner != null) yield return null;
        }

        // Played at the player: they can be far from the reactor at this point and the sound is positional
        Debug.Log("[BossSequence] Playing the explosion.");
        Vector3 listenerPos = PlayerID.Instance != null ? PlayerID.Instance.transform.position : transform.position;
        yield return PlaySoundAndWait(Explosion, listenerPos);
        Debug.Log($"[BossSequence] Explosion finished, waiting {waitAfterExplosion}s.");
        yield return new WaitForSeconds(waitAfterExplosion);

        yield return PlayLogAndWait(apexDeathLog);
        Debug.Log($"[BossSequence] Waiting {waitAfterApexDeathLog}s.");
        yield return new WaitForSeconds(waitAfterApexDeathLog);
        yield return PlayLogAndWait(destroyOwnRadioLog);
        Debug.Log($"[BossSequence] Waiting {waitAfterDestroyOwnRadioLog}s.");
        yield return new WaitForSeconds(waitAfterDestroyOwnRadioLog);

        Debug.Log($"[BossSequence] Playing {RadioDestroyed}, then waiting {waitBeforeCredits}s.");
        listenerPos = PlayerID.Instance != null ? PlayerID.Instance.transform.position : transform.position;
        AudioManager.Instance.PlayOneShotNoAsync(RadioDestroyed, listenerPos);
        yield return new WaitForSeconds(waitBeforeCredits);

        Debug.Log("[BossSequence] Loading the credits.");
        // keep the ending track playing through the scene change, in place of the credits scene's own music
        if (MusicManager.Instance != null) MusicManager.Instance.KeepTrackThroughNextSceneLoad();

        // Credits has no player, so it cannot go through FadeToScene (which waits for the player to load).
        // The screen is already black from our own fade, so hand over to the fader and let it fade in the credits.
        SceneFader.Instance.SetBlackImmediate();
        SceneFader.Instance.LoadSceneAndFadeFromBlack(CreditsScene);
    }

    // Once the screen is black the player stays where they are until the credits load.
    private void FreezePlayer()
    {
        // input off, the same way the death menu and the prologue exit do it
        if (PlayerInput.Instance != null) PlayerInput.Instance.DebugToggleInput(true);

        // A key that was held when input went off would otherwise keep its last value, so also pin the body
        if (PlayerID.Instance != null && PlayerID.Instance.rb != null)
        {
            Rigidbody playerRb = PlayerID.Instance.rb;
            if (!playerRb.isKinematic)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
            playerRb.isKinematic = true;
        }
        Debug.Log("[BossSequence] Player movement disabled.");
    }

    // The ending lines play over the black screen, so the subtitles have to draw on top of the fade image.
    // Giving the subtitle text its own canvas layer above the fade's works whichever canvases they are on.
    private void KeepSubtitlesAboveFade()
    {
        if (fadeOut == null || AudioLogManager.Instance == null || AudioLogManager.Instance.Subtitles == null) return;

        Canvas fadeCanvas = fadeOut.canvas != null ? fadeOut.canvas.rootCanvas : null;
        int fadeOrder = fadeCanvas != null ? fadeCanvas.sortingOrder : 0;

        GameObject subtitleObject = AudioLogManager.Instance.Subtitles.gameObject;
        Canvas subtitleCanvas = subtitleObject.GetComponent<Canvas>();
        if (subtitleCanvas == null) subtitleCanvas = subtitleObject.AddComponent<Canvas>();
        subtitleCanvas.overrideSorting = true;
        if (fadeCanvas != null) subtitleCanvas.sortingLayerID = fadeCanvas.sortingLayerID;
        subtitleCanvas.sortingOrder = fadeOrder + 1;
        Debug.Log($"[BossSequence] Subtitles raised above the fade (order {subtitleCanvas.sortingOrder}).");
    }

    private void PlayLog(AudioLogObject log)
    {
        if (log == null)
        {
            Debug.LogWarning("NuclearReactorInteract: an audio log is not assigned, skipping it.");
            return;
        }
        if (AudioLogManager.Instance == null || PlayerID.Instance == null) return;

        AudioLogManager.Instance.PlayAudioLog(log, PlayerID.Instance.gameObject);
    }

    // Boss lines go through one queue so a console line and a sequence line never cut each other off:
    // each waits for whatever is playing, then plays in the order it was asked for.
    private void QueueLine(AudioLogObject log)
    {
        if (log == null)
        {
            Debug.LogWarning("[BossSequence] An audio log is not assigned, skipping it.");
            return;
        }

        lineQueue.Enqueue(log);
        if (lineRunner == null) lineRunner = StartCoroutine(RunLineQueue());
    }

    private IEnumerator RunLineQueue()
    {
        while (lineQueue.Count > 0)
        {
            // something else may be talking (another boss line, or an unrelated audio log)
            yield return WaitForLogToFinish(MaxLineWait);

            AudioLogObject log = lineQueue.Dequeue();
            Debug.Log($"[BossSequence] Playing audio log '{log.audioName}'.");
            // the boss music comes in with the line that announces the Apex
            if (log == apexSpawnsLog && !endingStarted) RequestMusic(bossMusicKey);
            PlayLog(log);
            yield return WaitForLogToFinish(LogLength(log) + LogTimeoutMargin);
        }
        lineRunner = null;
    }

    // Queues the line and waits until it, and anything queued around it, has finished
    private IEnumerator PlayLogAndWait(AudioLogObject log)
    {
        QueueLine(log);
        yield return null;
        while (lineRunner != null) yield return null;
    }

    // Length of a log taken from its last subtitle timestamp
    private static float LogLength(AudioLogObject log)
    {
        float length = 0f;
        if (log != null && log.subtitles != null)
        {
            foreach (var line in log.subtitles) length = Mathf.Max(length, line.seconds);
        }
        return length;
    }

    // Waits until no audio log is playing. Gives up after the timeout so a log that never reports that it
    // finished cannot hang the sequence.
    private IEnumerator WaitForLogToFinish(float timeout)
    {
        yield return null;
        float waited = 0f;
        while (AudioLogManager.Instance != null && AudioLogManager.Instance.IsPlaying)
        {
            waited += Time.deltaTime;
            if (waited >= timeout)
            {
                Debug.LogWarning($"[BossSequence] Audio log still playing after {timeout:F0}s, moving on.");
                break;
            }
            yield return null;
        }
    }

    private IEnumerator PlaySoundAndWait(string soundName, Vector3 position)
    {
        EventInstance sound = AudioManager.Instance.PlayOneShotStoppableNoAsync(soundName, position);
        if (!sound.isValid()) yield break;

        // length of the event, as a cap in case it never reports that it stopped
        float maxWait = 30f;
        if (sound.getDescription(out EventDescription description) == FMOD.RESULT.OK
            && description.getLength(out int lengthMs) == FMOD.RESULT.OK && lengthMs > 0)
        {
            maxWait = lengthMs / 1000f + 1f;
        }

        float waited = 0f;
        while (waited < maxWait)
        {
            sound.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED) break;
            waited += Time.deltaTime;
            yield return null;
        }
        sound.release();
    }
}
