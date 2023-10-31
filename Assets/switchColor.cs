using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class switchColor : MonoBehaviour
{

    public Button switcher;
    public Player player;
    public GameModel.GameColor switcherColor;

    // Start is called before the first frame update
    void Start()
    {
        switcher.onClick.AddListener(TaskOnClick);
        player = GameObject.Find("Player").GetComponent<Player>();
    }
    void TaskOnClick()
    {
        player.setColor((int)switcherColor);
    }

}
