using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockData
{
    public int rainbowInk;
    public List<int> arenas;
    public int currentArena;

    public UnlockData()
    {
        rainbowInk = 0;
        arenas = new List<int>();
        arenas.Add(0);
        currentArena = 0;
    }
}
