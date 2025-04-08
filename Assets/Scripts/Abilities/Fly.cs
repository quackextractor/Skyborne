using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Fly : Fireball
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
        Debug.Log("yaaay");


    }

    // Update is called once per frame
    private void Update()
    {
        _rb.AddForce(_position, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.name != "Player")
        {
            Target target = other.GetComponent<Target>();
            target.TakeAttack(new Attack(10, 10, _position));
            Destroy(this.gameObject);
        }
    }
}