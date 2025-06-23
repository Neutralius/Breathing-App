using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CalibrationPopUp : MonoBehaviour
{
    public GameObject calibPanel;
    public Button openPopupButton;
    public Button startCalibrationButton;
    public Button cancelButton;
    public TMP_Text CalibrationText;
    private string micDevice;
    private AudioClip micClip;
    private int sampleWindow = 128;
    private float gain;
    public static float gainFactor = 1f;
    private bool calibrating = false;

    void Start()
    {
        calibPanel.SetActive(false);

        openPopupButton.onClick.AddListener(() =>
        {
            calibPanel.SetActive(true);
        });

        startCalibrationButton.onClick.AddListener(() =>
        {
            if (!calibrating)
                StartCoroutine(CalibrateMic());
        });

        cancelButton.onClick.AddListener(() =>
        {
            calibPanel.SetActive(false);
        });
    }

    void Update()
    {
        //TODO
    }

    // old
    void StartCalibration()
    {
        micDevice = Microphone.devices[0];
        if (micDevice == null)
        {
            CalibrationText.text = "No microphone found!";
            return;
        }
        micClip = Microphone.Start(micDevice, true, 10, 44100);
        if (micClip == null)
        {
            CalibrationText.text = "Can't use microphone!";
            return;
        }
    }

    IEnumerator CalibrateMic()
    {
        calibrating = true;
        CalibrationText.text = "Calibrating...";

        micDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (micDevice == null)
        {
            CalibrationText.text = "No microphone found!";
            calibrating = false;
            yield break;
        }

        micClip = Microphone.Start(micDevice, true, 10, 44100);
        while (!(Microphone.GetPosition(micDevice) > 0)) { }  // wait for mic

        float sum = 0f;
        int frames = 20;

        for (int i = 0; i < frames; i++)
        {
            sum += GetMicAverage();
            print(sum);
            yield return new WaitForSeconds(0.1f);
        }

        float avg = sum / frames;

        // aiming for best average value of 0.15f (normal speaking)
        float targetLevel = 0.15f;
        if (avg > 0f)
        {
            gainFactor = targetLevel / avg;
            gainFactor = Mathf.Clamp(gainFactor, 1f, 10f);
            print(gainFactor);
            CalibrationText.text = $"Calibrated!\nGain: {gainFactor:F2}";
        }
        else
        {
            CalibrationText.text = $"Mic too quiet: {avg} – try again.";
            gainFactor += 0.5f;
        }

        calibrating = false;
    }

    float GetMicAverage()
    {
        float[] samples = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(micDevice) - sampleWindow + 1;
        if (micPosition < 0) return 0f;

        micClip.GetData(samples, micPosition);
        float sum = 0f;
        for (int i = 0; i < sampleWindow; ++i)
        {
            sum += Mathf.Abs(samples[i]);
        }
        return sum / sampleWindow;
    }


}