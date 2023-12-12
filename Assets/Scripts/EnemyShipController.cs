using UnityEngine;

public class EnemyShipController : ShipController
{
    private Transform _player;
    private Vector2 lastInput;

    private void Start()
    {
        base.Start();
        _player = FindObjectOfType<PlayerShipController>().transform;
    }

    private void Update()
    {
        var error = _player.position - _transform.position;
        error = Quaternion.Inverse(_transform.rotation) * error;
        
        var errorDir = error.normalized;
        var pitchError = new Vector3(0, error.y, error.z).normalized;
        var rollError = new Vector3(error.x, error.y, 0).normalized;
        var yawError = new Vector3(error.x, 0, error.z).normalized;

        if (Vector3.Angle(Vector3.forward, errorDir) < 15) {
            var yaw = Vector3.SignedAngle(Vector3.forward, yawError, Vector3.up);
            yaw = Mathf.Clamp(yaw, -1, 1);
            _rigidbody.AddTorque(transform.up * (yaw * _steeringPower), ForceMode.Acceleration);
            Shoot(true);
        }
        else
        {
            Shoot(false);
            var roll = Vector3.SignedAngle(Vector3.up, rollError, Vector3.forward);
            _steering.x = Mathf.Clamp(roll, -1, 1);
        }
        var pitch = Vector3.SignedAngle(Vector3.forward, pitchError, Vector3.right);
        _steering.y = Mathf.Clamp(pitch, -1, 1);

        _throttle = Mathf.Clamp(error.sqrMagnitude / 10000, 0, 2);
    }
}