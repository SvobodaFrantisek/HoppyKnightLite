using UnityEngine;

public class Gem : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float flySpeed = 15f;

    public GameManager gameManager;
    public bool isFlyingToPlayer = false;
    private Transform playerPosition;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager nenalezen");
        }
    }

    void Update()
    {
        if (!isFlyingToPlayer)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.up, (rotationSpeed * 5f) * Time.deltaTime);

            Vector3 targetPosition = playerPosition.position + Vector3.up * 1.1f;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                CollectGem();
            }
        }
    }

    public void CollectGem()
    {
        if (gameManager != null)
        {
            gameManager.AddGem(1);
        }

        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFlyingToPlayer)
        {
            playerPosition = other.transform;
            isFlyingToPlayer = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}
