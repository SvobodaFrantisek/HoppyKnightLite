using UnityEngine;

public class ShopkeeperScript : MonoBehaviour
{
    public GameObject shopUI;
    public GameManager gameManager;
    public bool playerInRange = false;

    public int healthCost = 0;
    public int speedCost = 0;
    public int jumpBoostCost = 0;
    public int doubleJumpCost = 0;
    public int strengthCost = 0;

    private void Start()
    {
        if (shopUI != null)
            shopUI.SetActive(false);
        else
        {
            Debug.LogError("shop panel neni prirazen");
        }

    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInRange == true)
        {
            OpenShop();
        }

    }

    public void OpenShop()
    {
        if (shopUI == null)
        {
            return;
        }
        shopUI.SetActive(!shopUI.activeSelf);
        if (shopUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUI != null && shopUI.activeSelf)
            {
                OpenShop();
            }
        }
    }


    public void BuyHealth()
    {
       gameManager.buyItem(healthCost);
    }
    public void BuySpeed()
    {
        gameManager.buyItem(speedCost);
    }
    public void BuyJumpBoost()
    {
        gameManager.buyItem(jumpBoostCost);
    }

    public void BuyDoubleJump()
    {
       gameManager.buyItem(doubleJumpCost);
    }

    public void BuyStrength()
    {
       gameManager.buyItem(strengthCost);
    }
}

