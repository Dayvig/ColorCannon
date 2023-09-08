using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public List<Sprite> EnemySprites = new List<Sprite>();
    public List<WaveModifier> WaveMods = new List<WaveModifier>();
    public GameObject UpcomingPanel;
    public GameObject WaveModPanel;
    public Transform PreviewPanelRow1;
    public Transform PreviewPanelRow2;
    public Transform PreviewPanelRow3;
    public Transform WaveModPanelRow;
    public GameObject ChunkPreview;
    public GameObject WaveModPreview;
    public GameObject UpgradePreview;
    public GameObject PreviewImage;
    public GameObject UpgradePanel;
    public Transform UpgradePreviewPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    
    public GameModel modelGame;
    public GameManager gameManager;

    public GameObject toolTip;
    public TextMeshProUGUI toolTipText;
    public RectTransform toolTipRect;
    public RectTransform toolTipTextRect;
    public bool toolTipActive = false;

    //Note: put in game model
    private float xBounds = 3;
    private float yBounds = 5;
    public float toolTipTimer;
    private float toolTipDissappearInterval = 0.05f;

    public int[] baseToolTipOffset = { 0, 0 };
    public int[] baseToolTipSize = { 350, 150 };
    public int[] extendedToolTipSize = { 350, 230 };

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
    }

    public void WaveUIUpdate()
    {
        
    }

    public void PostWaveUIUpdate()
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
        UpcomingPanel.SetActive(true);
        UpgradePanel.SetActive(true);
        WaveModPreview.SetActive(true);
    }

    public void deactivatePostWaveUI()
    {
        UpcomingPanel.SetActive(false);
        UpgradePanel.SetActive(false);
        WaveModPreview.SetActive(false);
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

    public void SetupUpgradePreview(GameManager.Upgrade u)
    {
        GameObject newUpgradePreview = Instantiate(UpgradePreview, UpgradePreviewPanel);
        newUpgradePreview.GetComponent<UpgradeButton>().upp = u;
        Image upgradeImage = newUpgradePreview.transform.GetChild(0).GetComponent<Image>();
        upgradeImage.sprite = modelGame.UpgradeImageFromType(u.type);
        upgradeImage.color = modelGame.ColorToColor(u.color);
        newUpgradePreview.GetComponent<UpgradeButton>().initialize(u);
    }

    void SetColor(Image i, Color c)
    {
        i.color = c;
    }

    public void WipePreviewImages()
    {
        WipeWave();
        WipeUpgrades();
        WipeWaveMods();
    }

    public void WipeUpgrades()
    {
        for (int i = 0; i < UpgradePreviewPanel.childCount; i++)
        {
            UpgradePreviewPanel.GetChild(i).gameObject.SetActive(false);
        }
    }
    
    public void WipeWave()
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
    }

    public void WipeWaveMods()
    {
        for (int i = 0; i < WaveModPanelRow.childCount; i++)
        {
            WaveModPanelRow.GetChild(i).gameObject.SetActive(false);
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
    }
}
