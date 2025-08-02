using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpButton : MonoBehaviour
{
    public Button helpButton;
    public Button helpButton2;
    public Button helpButton3;
    public Button helpButton4;
    public Button closeButton;
    public AudioSource audioSource;
    public AudioClip helpAudio;
    public GameObject helpPanel;
    public bool vocalBool = BreathingApp.vocalBool; // Use the static variable from BreathingApp

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        helpPanel.SetActive(false); // Ensure the help panel is hidden at start
        helpButton.onClick.AddListener(() =>
        {
            if (vocalBool)
            {
                audioSource.clip = helpAudio;
                audioSource.Play();
                print(vocalBool);
            }
            else
            {
                helpPanel.SetActive(true);
                // because of weird bug: Other Help-Buttons are shining through the HelpPanel
                helpButton2.gameObject.SetActive(false);
                helpButton3.gameObject.SetActive(false);
                helpButton4.gameObject.SetActive(false);
                print(vocalBool);
            }
        });

        closeButton.onClick.AddListener(() =>
        {
            helpPanel.SetActive(false);
            helpButton2.gameObject.SetActive(true);
            helpButton3.gameObject.SetActive(true);
            helpButton4.gameObject.SetActive(true);
        });

    }

    // Update is called once per frame
    void Update()
    {
        vocalBool = BreathingApp.vocalBool;
    }
}
