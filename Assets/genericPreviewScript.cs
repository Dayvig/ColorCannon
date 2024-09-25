using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class genericPreviewScript : MonoBehaviour
{
    public BoxCollider2D thisCollider;
    public string modText;

    void Start()
    {
        thisCollider = GetComponent<BoxCollider2D>();
        setupCollider();
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
            UIManager.instance.renderToolTip(modText);
        }
    }
}
