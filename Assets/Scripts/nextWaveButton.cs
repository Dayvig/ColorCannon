using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class nextWaveButton : MonoBehaviour, IPointerEnterHandler
{
    public Button nextWave;
    public GameManager manager;
    public AudioSource audioSource;
    
    void Start()
    {
        nextWave.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void TaskOnClick()
    {
        manager.SetState(GameManager.GameState.WAVE);
    }

    public void HoverSound()
    {
        SoundManager.instance.PlaySound(audioSource, GameModel.instance.uiSounds[0]);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.instance.PlaySound(audioSource, GameModel.instance.uiSounds[0]);
    }
}
