using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Checkpoint zajima jen hrace.
        if (!other.CompareTag("Player"))
            return;

        Respawn respawn = other.GetComponent<Respawn>();
        if (respawn != null)
        {
            // Pri dotyku si hrac ulozi novy aktivni respawn point.
            respawn.SetRespawnPoint(transform);
        }
    }
}
