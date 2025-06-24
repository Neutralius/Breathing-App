using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BreathingApp : MonoBehaviour
{
    // ---------- UI Elements ----------

    public GameObject welcomePanel;           // Panel shown at app launch (welcome screen)
    public GameObject exerciseSelectionPanel; // Panel with 4 exercise options (selection menu)
    public GameObject breathingPanel;         // Main breathing panel (circle, feedback, etc.)

    public TMP_Text  titleText;                    // Displays the selected exercise name
    public TMP_Text  instructionText;              // Inhale / Exhale guidance text
    public TMP_Text  feedbackText;                 // Final message after session completion
    public TMP_Text  timerText;                    // Displays remaining session time
    public TMP_Text  scoreText;                    // Final score shown after session

    public Button startButton;                // Start button to begin the exercise
    public Button pauseButton;                // Pause button to pause/resume
    public Button stopButton;                 // Stop button to end session early
    public Button restartButton;              // Restart button to redo the session

    public RectTransform breathingCircle;     // Animated breathing circle
    public Slider volumeSlider;               // Slider showing microphone input volume

    // ---------- Microphone Setup ----------

    private AudioClip micClip;
    private string micDevice;
    private int sampleWindow = 128;           // Size of audio sample to process

    // ---------- Session Settings ----------

    private float sessionDuration = 20f;     // Total session time (20 seconds)
    private float remainingTime;              // Countdown timer

    private enum Phase { Inhale, Exhale }
    private Phase currentPhase = Phase.Inhale;
    private float phaseTimer = 0f;
    private float phaseDuration = 2f;        // Default duration per phase (can change based on selected exercise)

    private bool isSessionActive = false;
    private bool isPaused = false;

    private int successfulPhases = 0;         // Count of correctly completed breathing phases
    private int totalPhases = 0;              // Total number of attempted phases

    private string selectedExercise = "";     // Stores which breathing exercise was chosen

    // ---------- Initialization ----------

    void Start()
    {
        // Start with welcome panel visible
        ShowPanel(welcomePanel);
        HideAllExcept(welcomePanel);
        SetupButtons();
        feedbackText.text = "";
    }

    // ---------- Main Update Loop ----------

    void Update()
    {
        if (!isSessionActive || isPaused || micClip == null) return;

        // Countdown timer
        remainingTime -= Time.deltaTime;
        timerText.text = "Time Left: " + Mathf.CeilToInt(remainingTime) + "s";

        // End session if time runs out
        if (remainingTime <= 0f)
        {
            EndSession();
            return;
        }

        // Get microphone volume level
        float volume = GetMicVolume();
        volumeSlider.value = volume;

        // Handle breathing phase
        phaseTimer += Time.deltaTime;
        instructionText.text = currentPhase == Phase.Inhale ? "Inhale..." : "Exhale...";

        // Animate breathing circle size
        float baseScale = currentPhase == Phase.Inhale ? 1f : 2f;
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
            currentPhase = currentPhase == Phase.Inhale ? Phase.Exhale : Phase.Inhale;
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
            float abs = Mathf.Abs(sample);
            if (abs > maxVolume) maxVolume = abs;
        }
        return maxVolume;
    }

    // ---------- Session Control ----------

    void StartSession()
    {
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
    }

    void PauseSession()
    {
        isPaused = true;
        instructionText.text = "Paused";
    }

    void ResumeSession()
    {
        isPaused = false;
    }

    void StopSession()
    {
        isSessionActive = false;
        feedbackText.text = "Session Stopped.";
        restartButton.gameObject.SetActive(true);
    }

    void RestartSession()
    {
        breathingCircle.localScale = Vector3.one;
        StartSession();
    }

    // ---------- Exercise Selection Logic ----------

    public void SelectExercise(string exerciseName)
    {
        // Store the selected exercise and update UI
        selectedExercise = exerciseName;
        titleText.text = selectedExercise;

        // Set specific phase durations based on the selected exercise
        switch (selectedExercise)
        {
            case "Box Breathing":
                phaseDuration = 3f; break;
            case "4-7-8 Method":
                phaseDuration = 6f; break;
            case "Equal Breathing":
                phaseDuration = 1f; break;
            case "Relaxing Breath":
                phaseDuration = 5f; break;
            default:
                phaseDuration = 2f; break;
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
        ShowPanel(exerciseSelectionPanel); // Navigate from welcome to exercise choice
    }

    public void GoToWelcome()
    {
        ShowPanel(welcomePanel); // Optional back navigation
    }
}

