using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CalibrationPopUp : MonoBehaviour
{
    public GameObject calibPanel;
    public Button openPopupButton;
    public Button cancelButton;


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
}