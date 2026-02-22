using UnityEngine;

public class GameManager : MonoBehaviour
{


    public HUDmanager hudManager;
    public int coins = 0;
    public int health = 100;
    void Start()
    {
        hudManager = FindAnyObjectByType<HUDmanager>();
    }

  
    void Update()
    {
        
    }

    public void AddCoin(int amount)
        {
            coins += amount;
        if (hudManager != null)
        {
            hudManager.UpdateCoins(coins);
        }
        else
        {
            Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazení mincí.");
        }

            Debug.Log("Coins: " + coins);
    }

    public void buyItem(int cost)
    {
        
            if (coins >= cost)
            {
                coins -= cost;
                if (hudManager != null)
                {
                    hudManager.UpdateCoins(coins);
                }
                else
                {
                    Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazení mincí.");
                }
                Debug.Log("Item koupen, zůstatek mincí: " + coins);
            }
            else
            {
                Debug.Log("Nemáte dostatek mincí na koupi tohoto itemu.");
            }
    }
    public int getCoins()
    {
        return coins;
    }
}
