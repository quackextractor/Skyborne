using UnityEngine;
using System.Collections;

public class MoneyPickup : MonoBehaviour
{
    public int value = 1;
    public float collectSpeed = 1f;     // Base speed for collection arc
    public float lifeTime = 10f;
    public float delayBeforeCollect = 1f; // Delay before start moving towards player
    public float arcHeight = 2f;         // Height of the arc

    private Rigidbody rb;
    private bool _collected = false;
    private Transform _target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(int moneyValue)
    {
        value = moneyValue;
        // random impulse
        rb.AddForce(new Vector3(Random.Range(-1f, 1f), 1, 0) * 5f, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only collect when the collider belongs to an object tagged "Player"
        if (!_collected && other.CompareTag("Player"))
        {
            _collected = true;
            rb.isKinematic = true;

            // Target the main object tagged "Player" in the scene
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            _target = playerObj != null ? playerObj.transform : other.transform;

            // Start the collection coroutine
            StartCoroutine(CollectRoutine());
        }
    }

    private IEnumerator CollectRoutine()
    {
        // Wait before starting the arc
        yield return new WaitForSeconds(delayBeforeCollect);

        Vector3 startPos = transform.position;
        float t = 0f;

        // Continue until reached player
        while (t < 1f)
        {
            // Dynamic end point (player may move)
            Vector3 endPos = _target.position;
            Vector3 controlPoint = (startPos + endPos) * 0.5f + Vector3.up * arcHeight;

            // Accelerate over time
            float speedMultiplier = 1f + t * 2f; // 1→3
            t += Time.deltaTime * collectSpeed * speedMultiplier;
            t = Mathf.Clamp01(t);

            // Quadratic Bézier interpolation
            Vector3 m1 = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 m2 = Vector3.Lerp(controlPoint, endPos, t);
            transform.position = Vector3.Lerp(m1, m2, t);

            yield return null;
        }

        // Snap to player and collect
        transform.position = _target.position;
        CurrencyManager.Instance.AddMoney(value);
        Destroy(gameObject);
    }
}
