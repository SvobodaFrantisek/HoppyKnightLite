using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
   
    public TMP_Text coinText;
    public Slider healthBar;
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

    public void UpdateHealth(int health)
    {
        healthBar.value = health;
    }
}
