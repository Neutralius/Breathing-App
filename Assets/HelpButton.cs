using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpButton : MonoBehaviour
{
    public Button helpButton;
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
                print(vocalBool);
            }
        });

        closeButton.onClick.AddListener(() =>
        {
            helpPanel.SetActive(false);
        });

    }

    // Update is called once per frame
    void Update()
    {
        vocalBool = BreathingApp.vocalBool;
    }
}
