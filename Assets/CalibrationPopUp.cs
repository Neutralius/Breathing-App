using UnityEngine;
using UnityEngine.UI;

public class MicCalibrationUI : MonoBehaviour
{
    public GameObject calibPanel;
    public Button openPopupButton;
    public Button startCalibrationButton;
    public Button cancelButton;
    public GameObject micNormalizer;

    void Start()
    {
        calibPanel.SetActive(false);

        openPopupButton.onClick.AddListener(() => {
            calibPanel.SetActive(true);
        });

        startCalibrationButton.onClick.AddListener(() => {
            //StartCoroutine(micNormalizer.CalibrateMicrophone());
        });

        cancelButton.onClick.AddListener(() => {
            calibPanel.SetActive(false);
        });
    }
}
