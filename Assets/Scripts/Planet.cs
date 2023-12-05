using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] private float density;
    [SerializeField] private float torque;

    private Rigidbody _rigidbody;
    private float _mass;
    private float _radius;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _radius = transform.lossyScale.x;
        _mass = Mathf.Pow(_radius, 3) * Mathf.PI * 4 / 3 * density;
        _rigidbody.mass = _mass;
    }

    private void FixedUpdate()
    {
        _rigidbody.AddTorque(transform.up * (_mass * torque));
        Debug.Log(_rigidbody.angularVelocity);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody collision))
        {
            var pullDirection = transform.position - collision.transform.position;
            var sqrDistance = pullDirection.sqrMagnitude;
            var force = _mass / sqrDistance;
            pullDirection.Normalize();
            if (other.TryGetComponent(out Planet planet))
            {
                if (sqrDistance <= Mathf.Pow(_radius * 2, 2))
                {
                    if (Vector3.Angle(collision.velocity, pullDirection) > 30)
                    {
                        //Дописать сюда эффект от приливных сил и создание кольца
                    }
                    //А сюда элз иф с простым уничтожением и эффектами
                    Destroy(planet.gameObject);
                    return;
                }
            }
            else
            {
                var torqueDirection = Vector3.Angle(other.transform.forward, pullDirection) <= 100 ?
                    Vector3.Cross(other.transform.forward, pullDirection) :
                    Vector3.Cross(pullDirection, other.transform.forward);
                collision.AddTorque(torqueDirection * (torqueDirection.sqrMagnitude * force / 20));
            }

            collision.AddForce(pullDirection * force, ForceMode.Force);
        }
    }
}