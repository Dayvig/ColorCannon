using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewScript : MonoBehaviour
{
    GameObject previewSprite;
    public BoxCollider2D thisCollider;
    private UIManager uiManager;

    void Start()
    {
        thisCollider = GetComponent<BoxCollider2D>();
        uiManager = GameObject.Find("GameManager").GetComponent<UIManager>();
    }
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (thisCollider.bounds.Contains(mousePos))
        {
            uiManager.renderToolTip("test");
        }
    }
}
