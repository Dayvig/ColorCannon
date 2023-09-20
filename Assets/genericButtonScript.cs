using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class genericButtonScript : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource audioSource;
    public Button thisButton;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        thisButton = gameObject.GetComponent<Button>();
        thisButton.onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick()
    {
        SoundManager.instance.PlaySound(GameManager.instance.gameAudio, GameModel.instance.uiSounds[1]);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.instance.PlaySound(GameManager.instance.gameAudio, GameModel.instance.uiSounds[0]);
    }
}
