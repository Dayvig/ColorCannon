using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockData
{
    public int rainbowInk;
    public List<int> arenas;
    public int currentArena;
    public int maxProMode;

    public UnlockData()
    {
        maxProMode = 0;
        rainbowInk = 0;
        arenas = new List<int> { 0 };
        currentArena = 0;
    }
}
