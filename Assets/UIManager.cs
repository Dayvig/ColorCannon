using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public List<Sprite> EnemySprites = new List<Sprite>();
    public GameObject UpcomingPanel;
    public Transform PreviewPanel;
    public GameObject ChunkPreview;
    public GameObject UpgradePreview;
    public GameObject PreviewImage;
    public GameObject UpgradePanel;
    public Transform UpgradePreviewPanel;
    
    public GameModel modelGame;
    public GameManager gameManager;

    void Start(){
        modelGame = GameObject.Find("GameManager").GetComponent<GameModel>(); 
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void WaveUIUpdate()
    {
        
    }

    public void PostWaveUIUpdate()
    {
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
    
    public void SetupChunkPreview(WaveSpawningSystem.Chunk c)
    {
        GameObject newChunkPreview = Instantiate(ChunkPreview, PreviewPanel);
        for (int k = 0; k < c.colors.Length; k++)
        {
            GameObject newChunkPreviewImage = Instantiate(PreviewImage, newChunkPreview.transform);
            Image enemyImage = newChunkPreviewImage.GetComponent<Image>();
            enemyImage.sprite = c.image;
            enemyImage.color = modelGame.ColorToColor(c.colors[k]);
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
        for (int i = 0; i < PreviewPanel.childCount; i++)
        {
            PreviewPanel.GetChild(i).gameObject.SetActive(false);
        }
    }
}
