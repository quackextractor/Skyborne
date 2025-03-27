using UnityEngine;

public class Fly : MonoBehaviour
{
    private Rigidbody _rb;
    public GameObject player;
    private Vector3 _position;

    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        _position = player.transform.forward;
        transform.position = player.transform.position+ player.transform.forward;

    }

    // Update is called once per frame
    private void Update()
    {
        _rb.AddForce(_position, ForceMode.Impulse);
    }
}