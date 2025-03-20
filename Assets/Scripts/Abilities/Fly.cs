using UnityEngine;

public class Fly : MonoBehaviour
{
    private Rigidbody _rb;

    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        _rb.AddForce(transform.forward * 10, ForceMode.Impulse);
    }
}