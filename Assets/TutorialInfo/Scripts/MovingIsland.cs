using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MovingIsland : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0f;
    public bool startAtPointA = true;

    private Transform currentTarget;
    private float waitTimer;

    public Vector3 FrameDelta { get; private set; }

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("MovingIsland nema nastavene oba body.", this);
            enabled = false;
            return;
        }

        transform.position = startAtPointA ? pointA.position : pointB.position;
        currentTarget = startAtPointA ? pointB : pointA;
        FrameDelta = Vector3.zero;
    }

    void Update()
    {
        Vector3 previousPosition = transform.position;

        if (currentTarget == null)
        {
            FrameDelta = Vector3.zero;
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            FrameDelta = Vector3.zero;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget.position) <= 0.01f)
        {
            transform.position = currentTarget.position;
            currentTarget = currentTarget == pointA ? pointB : pointA;
            waitTimer = waitTimeAtPoint;
        }

        FrameDelta = transform.position - previousPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawSphere(pointA.position, 0.2f);
        Gizmos.DrawSphere(pointB.position, 0.2f);
    }
}
