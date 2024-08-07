using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class genericButtonScript : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource audioSource;
    public Button thisButton;
    public bool muteAudio;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        thisButton = gameObject.GetComponent<Button>();
        thisButton.onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick()
    {
        if (!muteAudio)
        {
            SoundManager.instance.PlaySFX(audioSource, GameModel.instance.uiSounds[1], -0.02f, 0.02f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.instance.PlaySFX(audioSource, GameModel.instance.uiSounds[0], -0.02f, 0.02f);
    }
}
