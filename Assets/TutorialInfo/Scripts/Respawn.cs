using UnityEngine;

public class Respawn : MonoBehaviour
{
    // Aktualni misto, kam se hrac vrati po padu pod mapu.
    public Transform respawnPoint;
    // Jednoducha kontrola padu pod level. Hodnota se bude ladit podle vysky ostrovu.
    public float fallLimitY = -20f;

    private CharacterController cc;
    private FPSController fpsController;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        fpsController = GetComponent<FPSController>();
    }

    void Update()
    {
        // Pro prvni verzi je nejjednodussi sledovat jen Y pozici hrace.
        if (transform.position.y < fallLimitY)
        {
            PlayerRespawn();
        }
    }

    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        // Checkpoint jen prepise, kam se ma dalsi respawn vratit.
        respawnPoint = newRespawnPoint;
    }

    public void PlayerRespawn()
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("Respawn point neni nastaveny.");
            return;
        }

        if (cc != null)
            // CharacterController je lepsi pri teleportu na chvili vypnout,
            // aby se nezasekl do kolize nebo neudelal divny skok.
            cc.enabled = false;

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        if (cc != null)
            cc.enabled = true;

        if (fpsController != null)
            fpsController.ResetMovementState();
    }
}
