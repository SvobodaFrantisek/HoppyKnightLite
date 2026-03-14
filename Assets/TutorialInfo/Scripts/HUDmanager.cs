using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
   
    public TMP_Text coinText;
    public Slider healthBar;
    public TMP_Text gemText;
    void Start()
    {
        

    }

    
    void Update()
    {
       

    }


    public void UpdateCoins(int coins)
    {
        coinText.text = "coins: "  + coins;

    }
    public void UpdateGems(int gems, int remainGems)
    {
        gemText.text = "gems: " + gems + "/" + remainGems;
    }

    public void UpdateHealth(int health)
    {
        healthBar.value = health;
    }
}
