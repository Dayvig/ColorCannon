using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DonateButton : MonoBehaviour
{
    public Button donate;
    public GameManager manager;
    public UIManager uiManager;
    public Image ren;
    public Player player;

    public int Price1;
    public int Price2;
    public int Price3;
    public int Return1;
    public int Return2;
    public int Return3;

    public List<RectTransform> DonateButtons;
    public List<Vector3> DonateButtonPositions;
    public Vector3 DonateButtonPos;

    public float donateButtonInterval = 1.2f;
    public float donateButtonTimer = 0.0f;
    public bool playDonateButtonAnimations = false;
    public bool showButtons = false;

    void Start()
    {
        donate.onClick.AddListener(TaskOnClick);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void Update()
    {
        if (playDonateButtonAnimations)
        {
            donateButtonTimer += showButtons ? Time.deltaTime : -Time.deltaTime;

            for (int i = 0; i < DonateButtons.Count; i++)
            {
                DonateButtons[i].anchoredPosition = Vector3.Lerp(DonateButtonPos, DonateButtonPositions[i], donateButtonTimer / donateButtonInterval);
                DonateButtons[i].localScale = Vector3.Lerp(Vector3.one * 0f, Vector3.one * 1f, donateButtonTimer / donateButtonInterval);
            }
            if (showButtons ? donateButtonTimer >= donateButtonInterval : donateButtonTimer < 0f)
            {
                if (!showButtons)
                {
                    for (int i = 0; i < DonateButtons.Count; i++)
                    {
                        DonateButtons[i].gameObject.SetActive(showButtons);
                    }
                }
                playDonateButtonAnimations = false;
            }
        }
    }

    void TaskOnClick()
    {
        playDonateButtonAnimations = true;
        showButtons = !showButtons;
        if (showButtons)
        {
            for (int i = 0; i < DonateButtons.Count; i++)
            {
                DonateButtons[i].gameObject.SetActive(showButtons);
            }
        }
    }
}
