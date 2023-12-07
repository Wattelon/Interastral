using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipStatsSO statsSO;
    [SerializeField] private float maxLinearVelocity;
    [SerializeField] private float maxAngularVelocity;

    private protected Transform _transform;
    private protected Rigidbody _rigidbody;
    private float _throttlePower;
    private protected float _steeringPower;
    private float _maxHealth;
    private float _curHealth;
    private float _maxShields;
    private float _curShields;
    [SerializeField] private protected Vector2 _steering;
    [SerializeField] private protected float _throttle;
    [SerializeField] private protected bool _isThrottleVertical;

    private const float THROTTLE_MULTIPLIER = 1000000;

    public float CurHealth
    {
        get => _curHealth;
        set => _curHealth = value < 0 ? 0 : value > _maxHealth ? _maxHealth : value;
    }

    private void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = maxAngularVelocity;
        _rigidbody.maxLinearVelocity = maxLinearVelocity;

        _throttlePower = statsSO.ThrottlePower;
        _steeringPower = statsSO.SteeringPower;
        _curHealth = _maxHealth = statsSO.MaxHealth;
        _curShields = _maxShields = statsSO.MaxShields;
    }

    private void FixedUpdate()
    {
        var direction = _isThrottleVertical ? _transform.up : _transform.forward;
        _rigidbody.AddForce(direction * (Mathf.Pow(_throttle, 2) * _throttlePower * THROTTLE_MULTIPLIER));
        _rigidbody.AddTorque(_transform.forward * (-_steering.x * _steeringPower), ForceMode.Acceleration);
        _rigidbody.AddTorque(_transform.right * (_steering.y * _steeringPower), ForceMode.Acceleration);
    }
}