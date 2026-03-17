using UnityEngine;

public class GameManager : MonoBehaviour
{
    public HUDmanager hudManager;
    public int gems = 0;
    public int remainGems = 10;
    public int maxHealth = 100;
    public int health = 100;
    public float damageCooldown = 0.35f;
    public float respawnInvulnerability = 1f;

    private Respawn playerRespawn;
    private float nextAllowedDamageTime;

    void Start()
    {
        hudManager = FindAnyObjectByType<HUDmanager>();
        playerRespawn = FindAnyObjectByType<Respawn>();
        ResetHealth();
        nextAllowedDamageTime = 0f;

        if (hudManager != null)
        {
            hudManager.UpdateGems(gems, remainGems);
        }
        else
        {
            Debug.LogError("HUDmanager nenalezen.");
        }
    }

    public void AddGem(int amount)
    {
        gems += amount;

        if (hudManager != null)
        {
            hudManager.UpdateGems(gems, remainGems);
        }
        else
        {
            Debug.LogError("HUDmanager nenalezen, nelze aktualizovat zobrazeni gemu.");
        }

        Debug.Log("Gems: " + gems);
    }

    public void DamagePlayer(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (Time.time < nextAllowedDamageTime)
        {
            return;
        }

        health = Mathf.Max(health - amount, 0);
        nextAllowedDamageTime = Time.time + damageCooldown;
        UpdateHealthUI();

        if (health <= 0)
        {
            HandlePlayerDeath();
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
        UpdateHealthUI();
    }

    void HandlePlayerDeath()
    {
        ResetActiveEnemies();

        if (playerRespawn != null)
        {
            playerRespawn.PlayerRespawn();
        }
        else
        {
            Debug.LogWarning("Respawn nenalezen, hrac se nemuze vratit na checkpoint.");
        }

        ResetHealth();
        nextAllowedDamageTime = Time.time + respawnInvulnerability;
    }

    void ResetActiveEnemies()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (EnemyController enemy in enemies)
        {
            enemy.ResetEnemy();
        }
    }

    void UpdateHealthUI()
    {
        if (hudManager != null)
        {
            hudManager.SetHealthMax(maxHealth);
            hudManager.UpdateHealth(health);
        }
    }
}
