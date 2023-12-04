using UnityEngine;

public class GravitationalField : MonoBehaviour
{
    [SerializeField] private float density;
    private float _mass;
    private float _radius;

    private void Start()
    {
        _radius = transform.parent.lossyScale.x;
        _mass = Mathf.Pow(_radius, 3) * Mathf.PI * 4 / 3 * density;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody collision))
        {
            var pullDirection = transform.position - collision.transform.position;
            var sqrDistance = pullDirection.sqrMagnitude;
            pullDirection.Normalize();
            
            var force = _mass / sqrDistance;
            collision.AddForce(pullDirection * force, ForceMode.Force);

            var torqueDirection = Vector3.Cross(other.transform.forward, pullDirection);
            collision.AddTorque(torqueDirection * (torqueDirection.sqrMagnitude * force / 5));
        }
    }
}
