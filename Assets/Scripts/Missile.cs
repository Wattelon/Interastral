using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] private float lifetime;
    [SerializeField] private float speed;
    [SerializeField] private float trackingAngle;
    [SerializeField] private float damage;
    [SerializeField] private float damageRadius;
    [SerializeField] private float turningGForce;
    [SerializeField] private float explosionForce;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private GameObject explosion;
    
    private BaseShipController _owner;
    private Rigidbody _target;
    private Transform _transform;
    private Vector3 _lastPosition;
    private float _timer;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _lastPosition = _rigidbody.position;
        _timer = lifetime;
    }

    private void FixedUpdate()
    {
        _timer -= Time.fixedDeltaTime;

        if (_timer <= 0)
        {
            Explode();
            return;
        }
        
        CheckCollision();
        TrackTarget();

        _rigidbody.velocity = _rigidbody.rotation * new Vector3(0, 0, speed);
    }
    
    public void Launch(BaseShipController owner, Rigidbody target)
    {
        _owner = owner;
        _target = target;
    }

    private void CheckCollision() {

        var currentPosition = _rigidbody.position;
        var error = currentPosition - _lastPosition;
        var ray = new Ray(_lastPosition, error.normalized);

        if (Physics.Raycast(ray, out var hit, error.magnitude, collisionMask.value)) {
            var other = hit.collider.gameObject.GetComponentInParent<BaseShipController>();

            if (other is not null && other != _owner) {
                hit.rigidbody.AddExplosionForce(explosionForce * 100, _rigidbody.position, 0);
                _rigidbody.position = hit.point;
                Explode();
            }
        }

        _lastPosition = currentPosition;
    }

    private void TrackTarget()
    {
        if (_target is not null)
        {
            var targetPosition = Intercept.FirstOrderIntercept(_rigidbody.position, Vector3.zero, speed,
                _target.position, _target.velocity);
            var error = targetPosition - _rigidbody.position;
            var targetDir = error.normalized;
            var currentDir = _rigidbody.rotation * Vector3.forward;

            if (Vector3.Angle(currentDir, targetDir) > trackingAngle)
            {
                Explode();
                return;
            }

            var maxTurnRate = turningGForce / speed;
            var dir = Vector3.RotateTowards(currentDir, targetDir, maxTurnRate * Time.fixedDeltaTime, 0);
            _rigidbody.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void Explode()
    {
        var hits = Physics.OverlapSphere(_rigidbody.position, damageRadius, collisionMask.value);
        List<BaseShipController> ships = new();
        foreach (var hit in hits) {
            var other = hit.GetComponentInParent<BaseShipController>();
            if (!ships.Contains(other))
            {
                ships.Add(other);
            }
        }
        
        foreach (var ship in ships)
        {
            var shipRigidbody = ship.GetComponent<Rigidbody>();
            var explosionDistance = (shipRigidbody.position - _rigidbody.position).magnitude;
            explosionDistance = Mathf.Max(1, explosionDistance / 10);
            shipRigidbody.AddExplosionForce(explosionForce, _rigidbody.position, damageRadius);
            ship.Damage(damage / explosionDistance, false, 2);
        }

        var vfx = Instantiate(explosion, _rigidbody.position, Quaternion.identity);
        Destroy(vfx, 3);
        Destroy(gameObject);
    }

    public void ProcessDeadTarget(Rigidbody target)
    {
        if (_target == target)
        {
            _target = null;
        }
    }
}