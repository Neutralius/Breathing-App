using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BreathingManager : MonoBehaviour
{
    // UI elements
    public RectTransform breathingCircle;    // Circle that grows/shrinks with breathing
    public TMP_Text instructionText;             // Shows "Inhale"/"Exhale"
    public Slider volumeSlider;              // Shows microphone volume
    public TMP_Text timerText;                   // Shows remaining time
    public TMP_Text feedbackText;                // Shows end result
    public TMP_Text scoreText;                // // Displays the final score percentage

    // Buttons
    public Button startButton;               // Starts the breathing session
    public Button pauseButton;               // Pauses/resumes session
    public Button stopButton;                // Ends session manually
    public Button restartButton;             // Restarts the whole session

    private AudioClip micClip;               // Microphone audio clip
    private string micDevice;                // Microphone device name
    private int sampleWindow = 128;          // Size of audio sample for volume check

    private float totalSessionTime = 20f;   // Total session time in seconds (2 minutes)
    private float remainingTime;             // Countdown timer
    private enum Phase { Inhale, Exhale }    // Breathing phases
    private Phase currentPhase = Phase.Inhale;
    private float phaseTime = 0f;            // Time passed in current phase
    private float phaseDuration = 2f;       // Duration per breathing phase
    private bool isSessionActive = false;    // Session state
    private bool isPaused = false;           // Pause state
    private int successfulPhases = 0;        // Tracks successful inhale/exhale cycles
    private int totalPhases = 0;             // Tracks all attempted cycles
    float gainFactor = MicrophoneSensitivity.boost;

    void Start()
    {
        feedbackText.text = "";
        SetupButtons();           // Setup button behavior
        HideSessionUI();          // Hide everything at start except Start button
    }

    void Update()
    {
        if (!isSessionActive || isPaused || micClip == null) return;

        // Update timer
        remainingTime -= Time.deltaTime;
        timerText.text = "Time left: " + Mathf.CeilToInt(remainingTime) + "s";

        if (remainingTime <= 0f)
        {
            EndSession();         // End when time runs out
            return;
        }

        // Microphone volume reading
        float volume = GetMaxVolume() * gainFactor;
        print(gainFactor);
        if (volumeSlider != null)
            volumeSlider.value = volume;

        // Update breathing phase
        phaseTime += Time.deltaTime;
        instructionText.text = currentPhase == Phase.Inhale ? "Inhale..." : "Exhale...";

        // Animate the circle based on volume
        float baseScale = currentPhase == Phase.Inhale ? 0.5f : 1f;
        float targetScale = baseScale + volume * 3f;
        breathingCircle.localScale = Vector3.Lerp(
            breathingCircle.localScale,
            new Vector3(targetScale, targetScale, 1f),
            Time.deltaTime * 3f
        );

        // Switch phase if time is up
        if (phaseTime >= phaseDuration)
        {
            totalPhases++;
            if (volume > 0.01f)
                successfulPhases++;

            phaseTime = 0f;
            currentPhase = currentPhase == Phase.Inhale ? Phase.Exhale : Phase.Inhale;
        }
    }

    // Get the highest volume in the audio sample
    float GetMaxVolume()
    {
        float maxLevel = 0f;
        float[] samples = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(micDevice) - sampleWindow + 1;
        if (micPosition < 0) return 0f;

        micClip.GetData(samples, micPosition);
        for (int i = 0; i < sampleWindow; ++i)
        {
            float wavePeak = Mathf.Abs(samples[i]);
            if (wavePeak > maxLevel)
                maxLevel = wavePeak;
        }
        return maxLevel;
    }

    // Starts the breathing session
    void StartSession()
    {
        micDevice = Microphone.devices[0];
        micClip = Microphone.Start(micDevice, true, 10, 44100);
        while (!(Microphone.GetPosition(micDevice) > 0)) { }  // wait for mic
        remainingTime = totalSessionTime;
        isSessionActive = true;
        isPaused = false;
        successfulPhases = 0;
        totalPhases = 0;
        feedbackText.text = "";
        ShowSessionUI();
    }

    // Pause/resume logic
    void PauseSession()
    {
        isPaused = true;
        instructionText.text = "Paused";
    }

    void ResumeSession()
    {
        isPaused = false;
    }

    // Manual stop
    void StopSession()
    {
        isSessionActive = false;
        instructionText.text = "Session stopped.";
        ShowRestartButton();
    }

    // Final evaluation after timer ends
    void EndSession()
    {
        isSessionActive = false;
        breathingCircle.localScale = Vector3.one;
        float score = (float)successfulPhases / totalPhases;
        int scorePercent = Mathf.RoundToInt(score * 100);


        if(scoreText != null)
        scoreText.text = "Your score : " + scorePercent + "%";


        if (score >= 0.5f)
            feedbackText.text = "Well done! ";
        else
            feedbackText.text = "Try again ";

        ShowRestartButton();
    }

    // Reset session from the beginning
    void RestartSession()
    {
        StopAllCoroutines();
        breathingCircle.localScale = Vector3.one;
        StartSession();
    }

    // Hook up button events
    void SetupButtons()
    {
        startButton.onClick.AddListener(StartSession);
        pauseButton.onClick.AddListener(() => {
            if (!isPaused) PauseSession(); else ResumeSession();
        });
        stopButton.onClick.AddListener(StopSession);
        restartButton.onClick.AddListener(RestartSession);
    }

    // Hide all elements at beginning
    void HideSessionUI()
    {
        breathingCircle.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(false);
        volumeSlider.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        // Start button remains visible
       // startButton.gameObject.SetActive(true);
        // Clear the score text at start
        if(scoreText != null)
            scoreText.text = "";
    
    }

    // Show UI after session starts
    void ShowSessionUI()
    {
        breathingCircle.gameObject.SetActive(true);
        instructionText.gameObject.SetActive(true);
        volumeSlider.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    // Show restart button at end or on stop
    void ShowRestartButton()
    {
        restartButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(false);
    }
}
