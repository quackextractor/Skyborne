using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Fly:MonoBehaviour
{
 
    private GameObject player;
    private Vector3 _position;
    [SerializeField] private float _timestamp = 4 ;
    [SerializeField] private float _attack = 4 ;
    [SerializeField] private float _knockback = 4 ;
    [SerializeField] private float speed = 0.1f ;
    // Start is called before the first frame update
    private void Start()
    {
        
        player = GameObject.FindGameObjectWithTag("Player");
        _position = player.transform.forward.normalized;
        transform.position = player.transform.position + player.transform.forward;
        _timestamp += Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_timestamp < Time.time)
        {
            Destroy(this.gameObject);
        }
        transform.position += _position * speed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.name != "Player" && other.TryGetComponent<Target>(out Target target))
        {
            target.TakeAttack(new Attack(_attack, _knockback, transform.position));
            Destroy(this.gameObject);
        }
        else
        {
            return;
        }
    }
}