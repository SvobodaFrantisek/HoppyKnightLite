using UnityEngine;

public class Coin : MonoBehaviour
{
    
    public float rotationSpeed = 100f;
    public float hoverSpeed = 2f;
    public float hoverHeight = 0.25f;

   
    public float flySpeed = 15f; 

    private GameManager gameManager;
    private Vector3 startPosition;

    
    private bool isFlyingToPlayer = false;
    private Transform playerTransform;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager nenalezen");
        }

        startPosition = transform.position;
    }

    void Update()
    {
        
        if (!isFlyingToPlayer)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
        
        else
        {
            
            transform.Rotate(Vector3.up, (rotationSpeed * 5) * Time.deltaTime);

            Vector3 targetPosition = playerTransform.position + Vector3.up * 1.1f;


            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

            
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                CollectCoin();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Player") && !isFlyingToPlayer)
        {
           
            isFlyingToPlayer = true;
            playerTransform = other.transform;

           
            GetComponent<Collider>().enabled = false;
        }
    }

    
    void CollectCoin()
    {
        Debug.Log("Coin collected!");
        if (gameManager != null)
        {
            gameManager.AddCoin(1);
        }
        Destroy(gameObject);
    }
}