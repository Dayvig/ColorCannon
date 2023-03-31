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
    public GameObject PreviewImage;
    
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
    }

    public void deactivatePostWaveUI()
    {
        UpcomingPanel.SetActive(false);
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

    void SetColor(Image i, Color c)
    {
        i.color = c;
    }

    public void WipePreviewImages()
    {
        for (int i = 0; i < PreviewPanel.childCount; i++)
        {
            PreviewPanel.GetChild(i).gameObject.SetActive(false);
        }
    }
}
