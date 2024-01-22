using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    public Button muteButton;
    public Image ren;
    public bool muted = false;
    public Sprite mutedSprite;
    public Sprite unmutedSprite;

    void Start()
    {
        muteButton.onClick.AddListener(TaskOnClick);
        muted = SoundManager.instance.isMuted;
        ren.sprite = muted ? mutedSprite : unmutedSprite;
    }

    public void SetState()
    {
        muted = SoundManager.instance.isMuted;
        ren.sprite = muted ? mutedSprite : unmutedSprite;
    }

    void TaskOnClick()
    {
        if (muted)
        {
            SoundManager.instance.UnMuteAudio();
        }
        else
        {
            SoundManager.instance.MuteAudio();
        }
        muted = !muted;
        ren.sprite = muted ? mutedSprite : unmutedSprite;
    }
}
