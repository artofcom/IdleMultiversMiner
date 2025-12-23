using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAudioPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    Button targetButton;


    void Start()
    {
        targetButton = GetComponent<Button>();

        if(targetButton != null)
            targetButton.onClick.AddListener(playClickSound);
    }

    void playClickSound()
    {
        if(audioSource != null && audioSource.clip != null) 
            audioSource.PlayOneShot(audioSource.clip);
    }
}
