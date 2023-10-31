using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class previewScript2 : MonoBehaviour
{
    GameObject previewSprite;
    public BoxCollider2D thisCollider;
    private UIManager uiManager;
    public string modText;
    public Sprite chevronSprite;
    public TextMeshProUGUI text;

    void Start()
    {
        thisCollider = GetComponent<BoxCollider2D>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    public void setupCollider()
    {
        RectTransform rect = this.GetComponent<RectTransform>();
        thisCollider.size = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);
        thisCollider.isTrigger = true;
    }

    public void SetText(string mText)
    {
        modText = mText;
    }
    void Update()
    {
        //TODO: Implement touch control mode in gameModel
        Vector3 mousePos = (Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        if (thisCollider.bounds.Contains(mousePos))
        {
            uiManager.renderToolTip(modText);
        }
        setupCollider();
    }
}
