using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Android; // For microphone permissions

public class MicrophoneSensitivity : MonoBehaviour
{

    public Slider sensitivitySlider; // Drag the slider in the Unity editor
    public TMP_Text loudnessInfo;             // Shows the average Volume
    public static float boost = 1.0f;
    public TMP_Text boostInfo;
    private string micDevice;
    private AudioClip micClip;
    private int sampleWindow = 1024;
    private float[] samples;
    public Button startCalibrationButton;
    private bool isCalibrating = false;
    public AudioSource backgroundMusic;


    void Start()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 1.0f;
            sensitivitySlider.maxValue = 100.0f;
            sensitivitySlider.value = 1.0f; // Startwert
            sensitivitySlider.value = boost; // Default sensitivity
            sensitivitySlider.onValueChanged.AddListener(v => boost = v); // ads a listener to the slider to increase the boost
        }
        else
        {
            Debug.LogWarning("No sensitivity slider assigned.  Microphone Sensitivity will not work.");
            enabled = false; // disable script since slider is not assigned
        }

        samples = new float[sampleWindow];
        micDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (micDevice == null)
        {
            loudnessInfo.text = "No microphone found.";
            enabled = false;
            return;
        }
         startCalibrationButton.onClick.AddListener(() =>
        {
            if (!isCalibrating)
                StartCoroutine(StartCal());
        });
    }

    void Update()
    {
        boostInfo.text = $"Boost: {boost:F2}";
    }

    IEnumerator StartCal()
    {
        if (backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }
        isCalibrating = true;
        micClip = Microphone.Start(micDevice, false, 3, 44100);
        loudnessInfo.text = "Please exhale...";
        
        yield return new WaitForSeconds(3.1f);
        GetLoudnessFromMicrophone();
        
        isCalibrating = false;
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    // Calculate the loudness from the microphone input
    private void GetLoudnessFromMicrophone()
    {
        if (micClip == null)
        {
            loudnessInfo.text = "No recording found.";
            return;
        }

        micClip.GetData(samples, 0);
        float min = float.MaxValue;
        float max = float.MinValue;
        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float val = samples[i] * boost; // Apply the boost factor
            sum += Mathf.Abs(val);
            if (val > max) max = val;
            if (val < min) min = val;
        }

        float average = sum / samples.Length;

        loudnessInfo.text = $"Average Loudness: {average:F3}";

        if (average < 0.05f)
        {
            loudnessInfo.text += "\n⚠️ microphone is too silent!";
        }
        else
        {
            loudnessInfo.text += "\n✅ microphone adjusted!";
        }
    }    
}