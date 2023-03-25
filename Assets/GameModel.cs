using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{

    public enum GameColor
    {
        RED, YELLOW, BLUE
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Color ColorToColor(GameColor col)
    {
        switch (col)
        {
            case GameColor.RED:
                return Color.red;
            case GameColor.BLUE:
                return Color.blue;
            case GameColor.YELLOW:
                return Color.yellow;
        }
        return Color.gray;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
