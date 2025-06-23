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
    private string micDevice;
    private AudioClip micClip;
    private int sampleWindow = 128;



    void Start()
    {
        calibPanel.SetActive(false);

        openPopupButton.onClick.AddListener(() =>
        {
            calibPanel.SetActive(true);
        });

        cancelButton.onClick.AddListener(() =>
        {
            calibPanel.SetActive(false);
        });
    }


    //old
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