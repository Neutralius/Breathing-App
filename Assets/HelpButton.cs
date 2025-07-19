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
        helpButton.onClick.AddListener(() =>
        {
            if (vocalBool)
            {
                audioSource.clip = helpAudio;
                audioSource.Play();
            }
            else
            {
                helpPanel.SetActive(true);
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
        
    }
}
