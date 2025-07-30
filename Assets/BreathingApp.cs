using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Android; // For microphone permissions

public class BreathingApp : MonoBehaviour
{
    // ---------- UI Elements ----------

    public GameObject welcomePanel;           // Panel shown at app launch (welcome screen)
    public GameObject exerciseSelectionPanel; // Panel with 4 exercise options (selection menu)
    public GameObject breathingPanel;         // Main breathing panel (circle, feedback, etc.)

    public TMP_Text titleText;                    // Displays the selected exercise name
    public TMP_Text instructionText;              // Inhale / Exhale guidance text
    public TMP_Text feedbackText;                 // Final message after session completion
    public TMP_Text scoreText;                    // Final score shown after session
    public TMP_Text soundText;                     // Text for sound toggle button

    public Button startButton;                // Start button to begin the exercise
    public Button pauseButton;                // Pause button to pause/resume
    public Button stopButton;                 // Stop button to end session early
    public Button restartButton;              // Restart button to redo the session
    public Button soundButton;                // Button to toggle vocal instructions

    public RectTransform breathingCircle;     // Animated breathing circle
    public Slider volumeSlider;               // Slider showing microphone input volume

    // ---------- Microphone Setup ----------

    private AudioClip micClip;
    private string micDevice;
    private int sampleWindow = 128;           // Size of audio sample to process

    // ---------- Session Settings ----------

    private float sessionDuration = 20f;     // Total session time (20 seconds)
    private float remainingTime;              // Countdown timer

    private enum Phase { Inhale, Exhale, Hold }
    private Phase currentPhase = Phase.Inhale;
    private Phase lastPlayedPhase = Phase.Hold; // Last played phase for vocal instructions
    private float phaseTimer = 0f;
    private float phaseDuration;         // Duration of each breathing phase
    private float InhalePhaseDuration;        // Default duration per phase (can change based on selected exercise)
    private float ExhalePhaseDuration;
    private float HoldPhaseDuration;  
    private bool isSessionActive = false;
    private bool isPaused = false;

    private int successfulPhases = 0;         // Count of correctly completed breathing phases
    private int totalPhases = 0;              // Total number of attempted phases

    private string selectedExercise = "";     // Stores which breathing exercise was chosen

    float gainFactor = MicrophoneSensitivity.boost; // Gain factor for microphone sensitivity
    public static bool vocalBool = false; // Toggle for vocal instructions
    public AudioSource backgroundMusic;
    public AudioSource vocalInstructions; // Optional audio source for vocal instructions
    public AudioClip breathIn;
    public AudioClip breathOut;
    public AudioClip holdBreath;
    public AudioClip welcomeAudio; 


    // ---------- Initialization ----------

    void Start()
    {
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
        
        volumeSlider.minValue = 1.0f;
        volumeSlider.maxValue = 100.0f;
        volumeSlider.value = 1.0f; // Startwert
}
        // Start with welcome panel visible
        vocalInstructions.clip = welcomeAudio;
        ShowPanel(welcomePanel);
        HideAllExcept(welcomePanel);
        SetupButtons();
        feedbackText.text = "";
    }

    // ---------- Main Update Loop ----------

    void Update()
    {
        if (vocalBool)
        {
            soundText.text = "Sound: ON";
        }
        else
        {
            soundText.text = "Sound: OFF";
        }
        gainFactor = MicrophoneSensitivity.boost;
        if (!isSessionActive || isPaused || micClip == null) return;

        // Countdown timer
        remainingTime -= Time.deltaTime;
        //timerText.text = "Time Left: " + Mathf.CeilToInt(remainingTime) + "s";

        // End session if time runs out
        if (remainingTime <= 0f)
        {
            EndSession();
            return;
        }

        // Get microphone volume level
        float volume = GetMicVolume();
        volumeSlider.value = volume * gainFactor;

        // Handle breathing phase
        phaseTimer += Time.deltaTime;
        switch (currentPhase){
            case Phase.Inhale:
                if (vocalBool && lastPlayedPhase != Phase.Inhale)
                {
                    vocalInstructions.clip = breathIn;
                    vocalInstructions.Play();
                    lastPlayedPhase = Phase.Inhale;
                }
                instructionText.text = "Inhale..." + (InhalePhaseDuration - phaseTimer).ToString("F2");
                phaseDuration = InhalePhaseDuration;
                break;

            case Phase.Hold:
                if (vocalBool && lastPlayedPhase != Phase.Hold)
                {
                    vocalInstructions.clip = holdBreath;
                    vocalInstructions.Play();
                    lastPlayedPhase = Phase.Hold;
                }
                instructionText.text = "Hold..." + (HoldPhaseDuration - phaseTimer).ToString("F2");
                phaseDuration = HoldPhaseDuration;
                break;

            case Phase.Exhale:
                if (vocalBool && lastPlayedPhase != Phase.Exhale)
                {
                    vocalInstructions.clip = breathOut;
                    vocalInstructions.Play();
                    lastPlayedPhase = Phase.Exhale;
                }
                instructionText.text = "Exhale..." + (ExhalePhaseDuration - phaseTimer).ToString("F2");
                phaseDuration = ExhalePhaseDuration;
                break;
        }

        // Animate breathing circle size
        float baseScale = currentPhase == Phase.Inhale ? 2f : 1f;
        float targetScale = baseScale + volume * 3f;

        breathingCircle.localScale = Vector3.Lerp(
            breathingCircle.localScale,
            new Vector3(targetScale, targetScale, 1f),
            Time.deltaTime * 4f
        );
    
        // Check if phase duration is completed
        if (phaseTimer >= phaseDuration)
        {
            totalPhases++;
            if (volume > 0.02f) successfulPhases++; // Acceptable volume threshold
            switch (currentPhase)
            {
                case Phase.Inhale:
                    currentPhase = Phase.Hold; // Switch to Hold after Inhale
                    break;
                case Phase.Hold:
                    currentPhase = Phase.Exhale; // Switch to Exhale after Hold
                    break;
                case Phase.Exhale:
                    currentPhase = Phase.Inhale; // Switch back to Inhale after Exhale
                    break;
            }
            phaseTimer = 0f;
        }
    }

    // ---------- Microphone Volume Detection ----------

    float GetMicVolume()
    {
        float maxVolume = 0f;
        float[] samples = new float[sampleWindow];
        int micPos = Microphone.GetPosition(micDevice) - sampleWindow + 1;
        if (micPos < 0) return 0f;
        micClip.GetData(samples, micPos);

        foreach (float sample in samples)
        {
            float abs = Mathf.Abs(sample) * gainFactor;
            if (abs > maxVolume) maxVolume = abs;
        }       
        return maxVolume;
    }

    // ---------- Session Control ----------

    void StartSession()
    {
        if (backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }
        // Start microphone input
        micDevice = Microphone.devices[0];
        micClip = Microphone.Start(micDevice, true, 10, 44100);
        while (!(Microphone.GetPosition(micDevice) > 0)) { }

        // Set session state
        remainingTime = sessionDuration;
        isSessionActive = true;
        isPaused = false;
        successfulPhases = 0;
        totalPhases = 0;

        feedbackText.text = "";

        ShowPanel(breathingPanel);
        startButton.gameObject.SetActive(false);
    }

    void EndSession()
    {
        // Stop breathing session and calculate score
        isSessionActive = false;
        breathingCircle.localScale = Vector3.one;

        float ratio = totalPhases > 0 ? (float)successfulPhases / totalPhases : 0f;
        int score = Mathf.RoundToInt(ratio * 100);

        scoreText.text = "Score: " + score + "%";
        feedbackText.text = score >= 50 ? "Well done! " : "Try again ";

        restartButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    void PauseSession()
    {
        isPaused = true;
        instructionText.text = "Paused";
        startButton.gameObject.SetActive(true);
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    void ResumeSession()
    {
        if (backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }
        isPaused = false;
        startButton.gameObject.SetActive(false);
    }

    void StopSession()
    {
        isSessionActive = false;
        breathingCircle.localScale = Vector3.one;
        volumeSlider.value = 1f; // Reset volume slider
        currentPhase = Phase.Inhale;
        lastPlayedPhase = Phase.Hold; // Last played phase for vocal instructions
        phaseTimer = 0f;
        feedbackText.text = "Session Stopped.";
        scoreText.text = "";
        instructionText.text = "Press Start to begin again.";
        restartButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    void RestartSession()
    {
        StopSession();
        StartSession();
    }

    // ---------- Exercise Selection Logic ----------

    public void SelectExercise(string exerciseName)
    {
        // Store the selected exercise and update UI
        selectedExercise = exerciseName;
        titleText.text = selectedExercise;

        // Set specific phase durations based on the selected exercise
        // TODO: ausbauen und anpassen!
        switch (selectedExercise)
        {
            case "Box Breathing":
                StopSession(); // Reset session state
                InhalePhaseDuration = 4f;
                HoldPhaseDuration = 0f;
                ExhalePhaseDuration = 4f;
                break;
            case "4-7-8 Method":
                StopSession(); // Reset session state
                InhalePhaseDuration = 4f;
                HoldPhaseDuration = 7f;
                ExhalePhaseDuration = 8f;
                break;
            case "Coherent Breathing": 
                StopSession(); // Reset session state
                InhalePhaseDuration = 5f;
                HoldPhaseDuration = 0f;
                ExhalePhaseDuration = 5f;
                break;
            case "Deep Breathing":
                StopSession(); // Reset session state
                InhalePhaseDuration = 4f;
                HoldPhaseDuration = 4f;
                ExhalePhaseDuration = 4f;
                break;
            default:
                InhalePhaseDuration = 3f;
                HoldPhaseDuration = 2f;
                ExhalePhaseDuration = 4f;
                break;
        }

        ShowPanel(breathingPanel);
    }

    // ---------- Button Setup ----------

    void SetupButtons()
    {
        // Assign listeners to each button at runtime
        startButton.onClick.AddListener(StartSession);
        pauseButton.onClick.AddListener(() => { if (!isPaused) PauseSession(); else ResumeSession(); });
        stopButton.onClick.AddListener(StopSession);
        restartButton.onClick.AddListener(RestartSession);
    }

    // ---------- Panel Visibility Control ----------

    void ShowPanel(GameObject panel)
    {
        // Show only the selected panel
        welcomePanel.SetActive(false);
        exerciseSelectionPanel.SetActive(false);
        breathingPanel.SetActive(false);
        panel.SetActive(true);
    }

    void HideAllExcept(GameObject panelToShow)
    {
        // Hide all panels except one
        foreach (GameObject go in new GameObject[] { welcomePanel, exerciseSelectionPanel, breathingPanel })
        {
            if (go != null)
                go.SetActive(go == panelToShow);
        }
    }

    // ---------- UI Navigation Buttons ----------

    public void GoToExerciseSelection()
    {
        StopSession();
        ShowPanel(exerciseSelectionPanel); // Navigate from welcome to exercise choice
    }

    public void GoToWelcome()
    {
        StopSession();
        ShowPanel(welcomePanel); // Optional back navigation
    }
    
    public void ToggleVocalInstructions()
    {
        if (vocalBool)
        {
            vocalBool = false; // Toggle off
        }
        else
        {
            vocalBool = true;
        }
    }
}

