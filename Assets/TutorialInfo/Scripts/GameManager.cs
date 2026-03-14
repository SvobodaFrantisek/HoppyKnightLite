using UnityEngine;

public class GameManager : MonoBehaviour
{


    public HUDmanager hudManager;
    public int coins = 0;
    public int gems = 0;
    public int remainGems = 10;
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

    public void addGem(int amount)
    {
        gems += amount; 

        if (hudManager != null)
        {
            hudManager.UpdateGems(gems, remainGems);
        }
        else
        {
            Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazení gemů.");
        }
        Debug.Log("Gems: " + gems);
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
