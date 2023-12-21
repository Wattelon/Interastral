using System.Collections;
using UnityEngine;

public class EnemyShipController : BaseShipController
{
    private Transform _target;
    private Vector2 _lastInput;

    private new void Start()
    {
        base.Start();
        TargetLocated += OnTargetLocated;
    }
    
    private void Update()
    {
        if (_target is not null)
        {
            var error = _target.position - _transform.position;
            error = Quaternion.Inverse(_transform.rotation) * error;

            var errorDir = error.normalized;
            var pitchError = new Vector3(0, error.y, error.z).normalized;
            var rollError = new Vector3(error.x, error.y, 0).normalized;
            var yawError = new Vector3(error.x, 0, error.z).normalized;

            if (Vector3.Angle(Vector3.forward, errorDir) < 15)
            {
                var yaw = Vector3.SignedAngle(Vector3.forward, yawError, Vector3.up);
                yaw = Mathf.Clamp(yaw, -1, 1);
                _rigidbody.AddTorque(transform.up * (yaw * _steeringPower), ForceMode.Acceleration);
                if (_equipment == Equipment.Laser && !_isShootingLaser)
                {
                    Shoot(true);
                }
            }
            else
            {
                if (_equipment == Equipment.Laser && _isShootingLaser)
                {
                    Shoot(false);
                }

                var roll = Vector3.SignedAngle(Vector3.up, rollError, Vector3.forward);
                _steering.x = Mathf.Clamp(roll, -1, 1);
            }

            var pitch = Vector3.SignedAngle(Vector3.forward, pitchError, Vector3.right);
            _steering.y = Mathf.Clamp(pitch, -1, 1);

            _throttle = Mathf.Clamp(error.sqrMagnitude / 10000, 0, 2);

            if (missileCount > 0 && _missileLockTimer <= 0 && _missileReloadTimer <= 0)
            {
                StartCoroutine(ShootMissile());
            }
        }
    }

    private void OnTargetLocated(Rigidbody target, bool isLocated)
    {
        if (isLocated)
        {
            _target ??= target.transform;
        }
        else
        {
            if (_target == target.transform)
            {
                _target = _targets.Count > 0 ? _targets[0].transform : null;
                _steering = Vector2.zero;
                _throttle = 0;
                Shoot(false);
            }
        }
    }

    private IEnumerator ShootMissile()
    {
        Shoot(false);
        _equipment = Equipment.Missile;
        Shoot(true);
        yield return new WaitForSeconds(2f);
        _equipment = Equipment.Laser;
    }
}