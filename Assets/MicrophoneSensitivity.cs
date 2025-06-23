using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MicrophoneSensitivity : MonoBehaviour
{

    public Slider sensitivitySlider; // Drag the slider in the Unity editor
    public TMP_Text loudnessInfo;             // Shows the average Volume
    public static float boost = 0.1f;
    public TMP_Text boostInfo;
    private string micDevice;
    private AudioClip micClip;
    private int sampleWindow = 1024;
    private float[] samples;
    public Button startCalibrationButton;
    private bool isCalibrating = false;


    void Start()
    {
        if (sensitivitySlider != null)
        {
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
            loudnessInfo.text = "Kein Mikrofon gefunden.";
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
        boostInfo.text = boost.ToString();
    }

    IEnumerator StartCal()
{
    isCalibrating = true;
    micClip = Microphone.Start(micDevice, false, 3, 44100);  // Aufnahme startet
    loudnessInfo.text = "Bitte atmen...";
    
    yield return new WaitForSeconds(3.1f);  // Warte, bis Aufnahme vorbei ist
    GetLoudnessFromMicrophone();           // Jetzt analysieren
    
    isCalibrating = false;
}

    // Calculate the loudness from the microphone input
    private void GetLoudnessFromMicrophone()
    {
        if (micClip == null)
        {
            loudnessInfo.text = "Keine Aufnahme vorhanden.";
            return;
        }

        micClip.GetData(samples, 0);
        float min = float.MaxValue;
        float max = float.MinValue;
        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float val = samples[i];
            sum += Mathf.Abs(val);
            if (val > max) max = val;
            if (val < min) min = val;
        }

        float average = sum / samples.Length;

        loudnessInfo.text = $"⏺️ Max: {max:F3} | Min: {min:F3} | ∅: {average:F3}";

        if (average < 0.05f)
        {
            loudnessInfo.text += "\n⚠️ Mikrofon ist zu leise!";
        }
        else
        {
            loudnessInfo.text += "\n✅ Mikrofonpegel ist ausreichend.";
        }
    }    
}