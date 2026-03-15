using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour
{
    public GameManager gameManager;
    public string nextSceneName;
    public float portalDelay = 1.5f;

    // Tohle ma byt animator hrace, ktery prehraje Win animaci.
    public Animator portalAnimator;

    // Brani opakovanemu spousteni portalu vic krat po sobe.
    private bool isFinishing = false;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Portal reaguje jen na hrace a jen jednou.
        if (isFinishing) return;
        if (!other.CompareTag("Player")) return;

        // Dokud nema hrac vsechny gemy, level se nesmi dokoncit.
        if (gameManager != null && gameManager.gems < gameManager.remainGems)
        {
            Debug.Log("Musis sebrat vsechny gemy.");
            return;
        }

        StartCoroutine(FinishLevel(other.gameObject));
    }

    IEnumerator FinishLevel(GameObject player)
    {
        isFinishing = true;

        FPSController controller = player.GetComponent<FPSController>();
        if (controller != null)
            // Input zamkneme, ale movement script nechame bezet kvuli gravitaci.
            controller.SetInputLocked(true);

        CombatScript combat = player.GetComponent<CombatScript>();
        if (combat != null)
            combat.enabled = false;

        // Pred Win triggerem srovname animator do "neutralniho" stavu,
        // aby ho neprebily jine any-state prechody jako Jump/Falling/Dodge.
        Animator targetAnimator = portalAnimator != null ? portalAnimator : player.GetComponentInChildren<Animator>();
        if (targetAnimator != null)
        {
            targetAnimator.ResetTrigger("Jump");
            targetAnimator.ResetTrigger("doublejump");
            targetAnimator.ResetTrigger("Attack");
            targetAnimator.ResetTrigger("Dodge");
            targetAnimator.ResetTrigger("Win");
            targetAnimator.SetFloat("Speed", 0f);
            targetAnimator.SetBool("IsGrounded", true);
            targetAnimator.SetBool("IsClimbing", false);
            targetAnimator.SetTrigger("Win");
        }

        // Kratka pauza pro animaci / efekty pred nactenim dalsi sceny.
        yield return new WaitForSeconds(portalDelay);

        if (nextSceneName == null)
        {
          Debug.LogError("Next scene name is not set in the PortalScript.");

        }
        // Samotne dokonceni levelu.
        SceneManager.LoadScene(nextSceneName);  

    }

}
