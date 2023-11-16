using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class mousetoolscript : MonoBehaviour
{

    TextMeshPro textf;
    // Start is called before the first frame update
    void Start()
    {
        textf = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        this.transform.position = newMousePos;
        textf.text = "(" + mousePos.x + ", " + mousePos.y + ")";
    }
}
