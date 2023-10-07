using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static GameManager;
using Image = UnityEngine.UI.Image;

public class UIManager : MonoBehaviour, IDataPersistence
{

    public static UIManager instance { get; set; }

    public List<Sprite> EnemySprites = new List<Sprite>();
    public List<WaveModifier> WaveMods = new List<WaveModifier>();
    public GameObject previousUpgrades;
    public GameObject PostWaveUIPanel;
    public GameObject WaveModPanel;
    public GameObject currentUpgradePanel;
    public Transform PreviewPanelRow1;
    public Transform PreviewPanelRow2;
    public Transform PreviewPanelRow3;
    public List<Transform> currentUpgradeRows = new List<Transform>();
    public Transform WaveModPanelRow;
    public GameObject ChunkPreview;
    public GameObject WaveModPreview;
    public GameObject UpgradePreview;
    public GameObject PreviewImage;
    public GameObject UpgradePanel;
    public Transform UpgradePreviewPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    public GameObject RefreshUpgradesButton;
    public TextMeshProUGUI WaveText;
    
    public GameModel modelGame;
    public GameManager gameManager;

    public GameObject toolTip;
    public TextMeshProUGUI toolTipText;
    public RectTransform toolTipRect;
    public RectTransform toolTipTextRect;
    public bool toolTipActive = false;
    public bool refreshActive = true;

    //Note: put in game model
    private float xBounds = 3;
    private float yBounds = 5;
    public float toolTipTimer;
    private float toolTipDissappearInterval = 0.05f;

    public int[] baseToolTipOffset = { 0, 0 };
    public int[] baseToolTipSize = { 350, 150 };
    public int[] extendedToolTipSize = { 350, 230 };

    public static string[] WaveUIText = new string[] { "Wave: ", " / 15" };

    public float postWaveUITimer = 0.0f;
    public float postWaveUISwingInterval1;
    public float postWaveUISwingInterval2;
    private RectTransform PostUIRect;
    public Vector3 swingStart = new Vector3(-1076.0f, -7.2f, 0f);
    public Vector3 swingMid = new Vector3(-130f, 60f, 0f);
    public Vector3 swingEnd = new Vector3(-36f, -7.2f, 0f);
    private Vector3 swingRotStart = new Vector3(0f, 0f, 14f);
    private Vector3 swingRotMid = new Vector3(0f, 0f, -1.4f);
    private Vector3 swingRotEnd = new Vector3(0f, 0f, -3.4f);
    private bool swingIn = true;
    private Vector3 start;
    private Vector3 end;
    private Vector3 rotStart;
    private Vector3 rotEnd;
    private float interval1;
    private float interval2;

    public enum WaveModifier
    {
        NUMEROUS,
        FASTER,
        BIGGER_WAVE,
        DIFFICULT
    }

    void Start(){
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>(); 
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        toolTipRect = toolTip.GetComponent<RectTransform>();
        toolTipTextRect = toolTipText.gameObject.GetComponent<RectTransform>();
        deactivateWinLoseUI();
        PostUIRect = PostWaveUIPanel.GetComponent<RectTransform>();
    }

    public void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Multiple UIManagers Detected.");
        }
        instance = this;
    }

    public void WaveUIUpdate()
    {
        
    }

    public void activatePostWaveAnimations(bool swingingIn)
    {
        swingIn = swingingIn;
        start = swingIn ? swingStart : swingEnd;
        end = swingIn ? swingEnd : swingStart;
        rotStart = swingIn ? swingRotStart : swingRotEnd;
        rotEnd = swingIn ? swingRotEnd : swingRotStart;
        interval1 = swingIn ? postWaveUISwingInterval1 : postWaveUISwingInterval2;
        interval2 = swingIn ? postWaveUISwingInterval2 : postWaveUISwingInterval1;
        postWaveUITimer = 0.0f;
        PostUIRect.anchoredPosition = start;

        SoundManager.instance.PlaySound(GameManager.instance.gameAudio, GameModel.instance.uiSounds[2], -0.5f, 0.5f);

    }

    public void PostWaveUIAndAnimationUpdate()
    {
        if (postWaveUITimer == -1f)
        {
            if (toolTipTimer < toolTipDissappearInterval)
            {
                toolTipTimer += Time.deltaTime;
                if (toolTipTimer >= toolTipDissappearInterval)
                {
                    SetToolTip(false);
                }
                else
                {
                    SetToolTip(true);
                }
            }
        }
        else
        {
            postWaveUITimer += Time.deltaTime;
            if (postWaveUITimer < interval1)
            {
                PostUIRect.anchoredPosition = Vector3.Lerp(start, swingMid, postWaveUITimer / interval1);
                PostUIRect.transform.rotation = Quaternion.Euler(Vector3.Lerp(rotStart, swingRotMid, postWaveUITimer / interval1));
            }
            else if (postWaveUITimer < interval2 + interval1)
            {
                PostUIRect.anchoredPosition = Vector3.Lerp(swingMid, end, (postWaveUITimer - interval1) / interval2);
                PostUIRect.transform.rotation = Quaternion.Euler(Vector3.Lerp(swingRotMid, rotEnd, (postWaveUITimer - interval1) / interval2));
            }   
            else 
            {
                postWaveUITimer = -1f;
                if (!swingIn)
                {
                    deactivatePostWaveUI();
                    GameManager.instance.SetState(GameState.WAVE);
                }
            }
        }
    }

    public void renderToolTip(String inputText)
    {
        if (inputText.ToCharArray().Length < modelGame.CHARACTERMAX)
        {
            toolTipRect.sizeDelta = new Vector2(baseToolTipSize[0], baseToolTipSize[1]);
            toolTipTextRect.sizeDelta = new Vector2(baseToolTipSize[0]-20, baseToolTipSize[1]-20);
        }
        else
        {
            toolTipRect.sizeDelta = new Vector2(extendedToolTipSize[0], extendedToolTipSize[1]);
            toolTipTextRect.sizeDelta = new Vector2(extendedToolTipSize[0] - 20, extendedToolTipSize[1] - 20);
        }
        Vector3 mousePos = Input.mousePosition;
        
        Vector3 toolTipPos = new Vector3(mousePos.x + (mousePos.x >= 150 ? -baseToolTipOffset[0] : baseToolTipOffset[0]), mousePos.y - baseToolTipOffset[1], 0);
        toolTip.transform.position = toolTipPos;
        toolTipText.text = inputText;
        toolTipTimer = 0.0f;
    }

    public void SetToolTip(bool isActive)
    {
        toolTip.SetActive(isActive);
    }

    public void AddWaveMod(WaveModifier waveMod)
    {
        WaveMods.Add(waveMod);
    }

    public void activateWinScreen()
    {
        WinPanel.SetActive(true);   
    }
    
    public void activateLoseScreen()
    {
        LosePanel.SetActive(true);
    }

    public void deactivateWinLoseUI()
    {
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
    }

    public void activatePostWaveUI()
    {
        PostWaveUIPanel.SetActive(true);
        UpgradePanel.SetActive(true);
        WaveModPreview.SetActive(true);
        currentUpgradePanel.SetActive(true);
        SetCurrentPlayerUpgradePreviews(true);
        SetWaveModPreviews(true);
        SetUpgradesVisible(true);
        RefreshUpgradesButton.SetActive(refreshActive);
        WaveText.text = WaveUIText[0] + WaveSpawningSystem.instance.Level + WaveUIText[1];
        refreshActive = true;
    }

    public void deactivatePostWaveUI()
    {
        PostWaveUIPanel.SetActive(false);
        UpgradePanel.SetActive(false);
        WaveModPreview.SetActive(false);
        currentUpgradePanel.SetActive(false);
        RefreshUpgradesButton.SetActive(false);
    }

    public void SetCurrentPlayerUpgradePreviews(bool isActive)
    {
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                currentUpgradeRows[i].GetChild(k).gameObject.SetActive(isActive);
            }
        }
    }

    public void SetWaveModPreviews(bool isActive)
    {
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            WaveModPanelRow.GetChild(i).gameObject.SetActive(isActive);
        }

    }

    public void SetupChunkPreview(WaveSpawningSystem.Chunk c, int row)
    {
        GameObject newChunkPreview;
        if (row == 1)
        {
            newChunkPreview = Instantiate(ChunkPreview, PreviewPanelRow1);
        }
        else if (row == 2)
        {
            newChunkPreview = Instantiate(ChunkPreview, PreviewPanelRow2);
        }
        else
        {
            newChunkPreview = Instantiate(ChunkPreview, PreviewPanelRow3);
        }
        for (int k = 0; k < c.colors.Length; k++)
        {
            GameObject newChunkPreviewImage = Instantiate(PreviewImage, newChunkPreview.transform);
            Image enemyImage = newChunkPreviewImage.GetComponent<Image>();
            enemyImage.sprite = c.image;
            Color newColor = modelGame.ColorToColor(c.colors[k]);
            if (!c.isDarkened)
            {
                enemyImage.color = newColor;
            }
            else
            {
                float div = modelGame.darkenedColorDivisor;
                enemyImage.color = new Color(newColor.r / div, newColor.g / div, newColor.b / div, newColor.a);
            }
        }
        previewScript2 thisPreview = newChunkPreview.GetComponent<previewScript2>();
        thisPreview.modText = modelGame.GetChunkTextFromType(c);
        thisPreview.setupCollider();
    }

    public void SetupWaveModUI()
    {
        foreach (WaveModifier mod in WaveMods)
        {
            GameObject newWaveModPreview = Instantiate(WaveModPreview, WaveModPanelRow.transform);
            GameObject newWaveModPreviewImage = Instantiate(PreviewImage, newWaveModPreview.transform);
            Image modImage = newWaveModPreviewImage.GetComponent<Image>();
            previewScript2 previewScript2 = newWaveModPreview.GetComponent<previewScript2>();
            previewScript2.SetText(modelGame.GetWaveModTextFromType(mod));
            previewScript2.setupCollider();
            modImage.sprite = modelGame.WaveModImageFromType(mod);
        }
    }

    public void SetupNextUpgradePreview(GameManager.Upgrade u)
    {
        GameObject newUpgradePreview = Instantiate(UpgradePreview, UpgradePreviewPanel);
        newUpgradePreview.GetComponent<UpgradeButton>().upp = u;
        Image upgradeImage = newUpgradePreview.transform.GetChild(0).GetComponent<Image>();
        upgradeImage.sprite = modelGame.UpgradeImageFromType(u.type);
        upgradeImage.color = modelGame.ColorToColor(u.color);
        newUpgradePreview.GetComponent<UpgradeButton>().initialize(u);
        newUpgradePreview.GetComponent<previewScript2>().modText = modelGame.GetUpgradeTextFromType(u.type);
        newUpgradePreview.SetActive(true);
    }

    public void AddNewPlayerUpgradeToPreview(GameManager.Upgrade u)
    {
        Transform rowToShowIn;
        switch (u.color)
        {
            case GameModel.GameColor.RED:
                rowToShowIn = currentUpgradeRows[0];
                break;
            case GameModel.GameColor.BLUE:
                rowToShowIn = currentUpgradeRows[1];
                break;
            case GameModel.GameColor.YELLOW:
                rowToShowIn = currentUpgradeRows[2];
                break;
            default:
                rowToShowIn = currentUpgradeRows[0];
                break;
        }
        GameObject newUpgradePreview = Instantiate(WaveModPreview, rowToShowIn);
        GameObject newUpgradePreviewImage = Instantiate(PreviewImage, newUpgradePreview.transform);
        Image upImage = newUpgradePreviewImage.GetComponent<Image>();
        upImage.sprite = modelGame.UpgradeImageFromType(u.type);
        upImage.color = modelGame.ColorToColor(u.color);
        newUpgradePreview.GetComponent<previewScript2>().modText = modelGame.GetUpgradeTextFromType(u.type);
    }

    void SetColor(Image i, Color c)
    {
        i.color = c;
    }

    public void WipePreviewImages()
    {
        HideWave();
        WipeUpgrades();
        HideWaveMods();
    }

    public void WipeUpgrades()
    {
        Transform found;
        List<Transform> toMove = new List<Transform>();
        for (int i = 0; i < UpgradePreviewPanel.transform.childCount; i++)
        {
            toMove.Add(UpgradePreviewPanel.GetChild(i));
        }
        foreach (Transform t in toMove)
        {
            t.SetParent(previousUpgrades.transform);
            t.gameObject.SetActive(false);
        }
    }

    public void SetUpgradesVisible(bool visible)
    {
        foreach (Transform g in UpgradePreviewPanel.GetComponentsInChildren<Transform>())
        {
            g.gameObject.SetActive(visible);
        }
    }

    public void HideWave()
    {
        for (int i = 0; i < PreviewPanelRow1.childCount; i++)
        {
            PreviewPanelRow1.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < PreviewPanelRow2.childCount; i++)
        {
            PreviewPanelRow2.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < PreviewPanelRow3.childCount; i++)
        {
            PreviewPanelRow3.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                currentUpgradeRows[i].GetChild(k).gameObject.SetActive(false);
            }
        }
    }

    public void HideWaveMods()
    {
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            Destroy(WaveModPanelRow.GetChild(i).gameObject);
        }
    }

    public void ClearWaveMods()
    {
        WaveMods.Clear();
    }

    public void DestroyAll()
    {
        for (int i = 0; i < PreviewPanelRow1.childCount; i++)
        {
            Destroy(PreviewPanelRow1.GetChild(i).gameObject);
        }
        for (int i = 0; i < PreviewPanelRow2.childCount; i++)
        {
            Destroy(PreviewPanelRow2.GetChild(i).gameObject);
        }
        for (int i = 0; i < PreviewPanelRow3.childCount; i++)
        {
            Destroy(PreviewPanelRow3.GetChild(i).gameObject);
        }
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            Destroy(WaveModPanelRow.GetChild(i).gameObject);
        }
        for (int i = 0; i < UpgradePreviewPanel.childCount; i++)
        {
            Destroy(UpgradePreviewPanel.GetChild(i).gameObject);
        }
        for (int i = 0; i < currentUpgradeRows.Count; i++)
        {
            for (int k = 0; k < currentUpgradeRows[i].childCount; k++)
            {
                Destroy(currentUpgradeRows[i].GetChild(k).gameObject);
            }
        }
    }

    public void LoadData(GameData data)
    {
        WaveMods = data.waveUpgrades;
        SetupWaveModUI();
        refreshActive = data.refreshActive;
        RefreshUpgradesButton.gameObject.SetActive(refreshActive);
    }

    public void SaveData(ref GameData data)
    {
        data.waveUpgrades = WaveMods;
        data.refreshActive = refreshActive;
        data.waveUpgrades = WaveMods;
    }
}
