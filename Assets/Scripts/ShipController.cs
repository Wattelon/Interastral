using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipStatsSO statsSO;
    [SerializeField] private float maxLinearVelocity;
    [SerializeField] private float maxAngularVelocity;
    [SerializeField] private List<Transform> blasters;
    [SerializeField] private LayerMask hitMask;

    private protected Transform _transform;
    private protected Rigidbody _rigidbody;
    private float _throttlePower;
    private protected float _steeringPower;
    private float _maxDurability;
    private float _curDurability;
    private float _maxShield;
    private float _curShield;
    private float _laserDamage;
    private RaycastHit _laserHit;
    private readonly List<Blaster> _blasters = new();
    [SerializeField] private bool _isShootingLaser;
    [SerializeField] private protected Vector2 _steering;
    [SerializeField] private protected float _throttle;
    [SerializeField] private protected bool _isThrottleVertical;
    [SerializeField] private Equipment _equipment;

    private const float THROTTLE_MULTIPLIER = 1000000;
    
    public delegate void StatChanged(float value);

    public event StatChanged DurabilityChanged;
    public event StatChanged ShieldChanged;

    public float CurDurability
    {
        get => _curDurability;
        set
        {
            _curDurability = value < 0 ? 0 : value > _maxDurability ? _maxDurability : value;
            DurabilityChanged?.Invoke(_curDurability);
        }
    }
    
    public float CurShield
    {
        get => _curShield;
        set
        {
            _curShield = value < 0 ? 0 : value > _maxShield ? _maxShield : value; 
            DurabilityChanged?.Invoke(_curShield);
        }
    }

    private void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = maxAngularVelocity;
        _rigidbody.maxLinearVelocity = maxLinearVelocity;

        _throttlePower = statsSO.ThrottlePower;
        _steeringPower = statsSO.SteeringPower;
        _curDurability = _maxDurability = statsSO.MaxDurability;
        _curShield = _maxShield = statsSO.MaxShield;
        _laserDamage = statsSO.LaserDamage;
    }

    private void Start()
    {
        foreach (var blaster in blasters)
        {
            var blasterStruct = new Blaster
            {
                Transform = blaster,
                LineRenderer = blaster.GetComponent<LineRenderer>(),
                ParticleSystem = blaster.GetComponent<ParticleSystem>()
            };
            _blasters.Add(blasterStruct);
        }
    }

    private void FixedUpdate()
    {
        var direction = _isThrottleVertical ? _transform.up : _transform.forward;
        _rigidbody.AddForce(direction * (Mathf.Pow(_throttle, 2) * _throttlePower * THROTTLE_MULTIPLIER));
        _rigidbody.AddTorque(_transform.forward * (-_steering.x * _steeringPower), ForceMode.Acceleration);
        _rigidbody.AddTorque(_transform.right * (_steering.y * _steeringPower), ForceMode.Acceleration);

        if (_isShootingLaser)
        {
            foreach (var blaster in _blasters)
            {
                if (Physics.Raycast(blaster.Transform.position, blaster.Transform.forward, out _laserHit, hitMask))
                {
                    blaster.LineRenderer.SetPosition(1, Vector3.forward * _laserHit.distance);
                    _laserHit.transform.GetComponent<ShipController>().CurDurability -= _laserDamage;
                }
                else
                {
                    blaster.LineRenderer.SetPosition(1, Vector3.forward * 1000);
                }
            }
        }
    }

    private protected void Shoot(bool toggle)
    {
        switch (_equipment)
        {
            case Equipment.Laser:
                _isShootingLaser = toggle;
                foreach (var blaster in _blasters)
                {
                    blaster.LineRenderer.enabled = _isShootingLaser;
                }
                break;
            case Equipment.Missile:
                break;
            case Equipment.Manipulator:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum Equipment
{
    Laser,
    Missile,
    Manipulator
}

public struct Blaster
{
    public Transform Transform;
    public LineRenderer LineRenderer;
    public ParticleSystem ParticleSystem;
}