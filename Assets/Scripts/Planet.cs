using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] private float density;
    [SerializeField] private float torque;
    [SerializeField] private bool isTorqueRandom;

    private Rigidbody _rigidbody;
    private float _mass;
    private float _radius;
    private const double GRAVITATIONAL_CONSTANT = 6.67E-11;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _radius = transform.lossyScale.x;
        _mass = Mathf.Pow(_radius, 3) * Mathf.PI * 4 / 3 * density;
        _rigidbody.mass = _mass;
        if (isTorqueRandom)
        {
            torque = Random.Range(-torque, torque);
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.AddTorque(transform.up * (_mass * torque));
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
                    if (Vector3.Angle(collision.linearVelocity, pullDirection) > 30)
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
                var torqueDirection = Vector3.Angle(other.transform.forward, pullDirection) <= 150 ?
                    Vector3.Cross(other.transform.forward, pullDirection) :
                    Vector3.Cross(pullDirection, other.transform.forward);
                collision.AddTorque(torqueDirection * force);
            }

            collision.AddForce(pullDirection * force, ForceMode.Force);
        }
    }
}