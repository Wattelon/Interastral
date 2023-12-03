using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] private float throttlePower;
    [SerializeField] private bool _isThrottleVertical;

    private Rigidbody _rigidbody;
    [SerializeField] private float _throttle;

    private const float THROTTLE_MULTIPLIER = 1000000;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var direction = _isThrottleVertical ? transform.up : transform.forward;
        _rigidbody.AddForce(direction * (Mathf.Pow(_throttle, 2) * throttlePower * THROTTLE_MULTIPLIER));
        Debug.Log(_rigidbody.velocity.magnitude);
    }

    public void SetThrottle(XRJoystick throttleControl)
    {
        _throttle = throttleControl.value.y + 1;
    }

    public void SetThrottleVertical()
    {
        _isThrottleVertical = !_isThrottleVertical;
    }
}
