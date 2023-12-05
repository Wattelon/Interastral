using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] private float throttlePower;
    [SerializeField] private float maxLinearVelocity;
    [SerializeField] private float maxAngularVelocity;

    private Transform _transform;
    private Rigidbody _rigidbody;
    [SerializeField] private Vector2 _steering;
    [SerializeField] private float _throttle;
    [SerializeField] private bool _isThrottleVertical;

    private const float THROTTLE_MULTIPLIER = 1000000;

    private void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = maxAngularVelocity;
        _rigidbody.maxLinearVelocity = maxLinearVelocity;
    }

    private void FixedUpdate()
    {
        var direction = _isThrottleVertical ? _transform.up : _transform.forward;
        _rigidbody.AddForce(direction * (Mathf.Pow(_throttle, 2) * throttlePower * THROTTLE_MULTIPLIER));
        _rigidbody.AddTorque(_transform.forward * -_steering.x, ForceMode.Acceleration);
        _rigidbody.AddTorque(_transform.right * _steering.y, ForceMode.Acceleration);
        //Debug.Log(_rigidbody.angularVelocity + "\n" + _rigidbody.angularVelocity.magnitude);
    }

    public void SetThrottle(XRJoystick throttleControl)
    {
        _throttle = throttleControl.value.y + 1;
    }

    public void SetThrottleVertical()
    {
        _isThrottleVertical = !_isThrottleVertical;
    }

    public void SetSteering(XRJoystick joystick)
    {
        _steering = joystick.value;
    }
}
