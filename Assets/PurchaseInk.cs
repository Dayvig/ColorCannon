using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class PurchaseInk : MonoBehaviour
{
    public AudioSource source;

    public void BuyInk(Product product)
    {
        GameManager.instance.rainbowInk += (int)(product.definition.payout.quantity);
        SaveLoadManager.instance.SaveUnlocks();
        SoundManager.instance.PlaySFX(source, GameModel.instance.bulletSounds[5]);
        UIManager.instance.unlockButton.setupUnlockButton();
        UIManager.instance.notebookInkDisplay.SetTextColorTransition(Color.green);
    }
}
