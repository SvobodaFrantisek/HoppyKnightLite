using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDmanager : MonoBehaviour
{
    public Slider healthBar;
    public TMP_Text gemText;

    public void SetHealthMax(int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
        }
    }

    public void UpdateGems(int gems, int remainGems)
    {
        if (gemText != null)
        {
            gemText.text = "Gems: " + gems + "/" + remainGems;
        }
    }

    public void UpdateHealth(int health)
    {
        if (healthBar != null)
        {
            healthBar.value = Mathf.Clamp(health, 0f, healthBar.maxValue);
        }
    }
}
