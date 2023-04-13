using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public List<Sprite> EnemySprites = new List<Sprite>();
    public GameObject UpcomingPanel;
    public Transform PreviewPanelRow1;
    public Transform PreviewPanelRow2;
    public Transform PreviewPanelRow3;
    public GameObject ChunkPreview;
    public GameObject UpgradePreview;
    public GameObject PreviewImage;
    public GameObject UpgradePanel;
    public Transform UpgradePreviewPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    
    public GameModel modelGame;
    public GameManager gameManager;

    void Start(){
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>(); 
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        deactivateWinLoseUI();
    }

    public void WaveUIUpdate()
    {
        
    }

    public void PostWaveUIUpdate()
    {
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
    }

    public void deactivatePostWaveUI()
    {
        UpcomingPanel.SetActive(false);
        UpgradePanel.SetActive(false);
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
}
