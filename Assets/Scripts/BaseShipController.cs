using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseShipController : MonoBehaviour
{
    [SerializeField] private ShipStatsSO statsSO;
    [SerializeField] private float maxLinearVelocity;
    [SerializeField] private float maxAngularVelocity;
    [SerializeField] private protected List<Transform> blasters;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool isEnemy;
    [SerializeField] private GameObject missile;
    [SerializeField] private int missileCount;
    [SerializeField] private List<Transform> missileLaunchers;

    private protected Transform _transform;
    private protected Rigidbody _rigidbody;
    private protected float _steeringPower;
    private float _throttlePower;
    private float _maxDurability;
    private float _curDurability;
    private float _maxShield;
    private float _curShield;
    private float _laserDamage;
    private float _missileDamage;
    private float _missileRange;
    private float _missileLockAngle;
    private float _missileLockTime;
    private protected float _missileLockTimer;
    private float _shieldRegenDelay;
    private float _shieldRegenRate;
    private float _shieldRegenTimer;
    private float _targetLocateRange;
    private bool _isMissileTracking;
    private bool _shieldExhausted;
    private bool _shieldFull = true;
    private Rigidbody _lockedTarget;
    private RaycastHit _laserHit;
    private AudioSource _audioEngines;
    private readonly List<Blaster> _blasters = new();
    private readonly List<Rigidbody> _targets = new();
    [SerializeField] private bool _isShootingLaser;
    [SerializeField] private protected Vector2 _steering;
    [SerializeField] private protected float _throttle;
    [SerializeField] private protected bool _isThrottleVertical;
    [SerializeField] private protected Equipment _equipment;

    private const float THROTTLE_MULTIPLIER = 1000000;
    
    public delegate void Condition(Rigidbody rigidbody);
    public event Condition Dead;
    public delegate void StatChanged(float value);
    public event StatChanged DurabilityChanged;
    public event StatChanged ShieldChanged;
    public delegate void TargetAcquired(Rigidbody target, bool isAcquired);
    public event TargetAcquired TargetLocated;
    public event TargetAcquired TargetLocked;
    
    public float CurDurability
    {
        get => _curDurability;
        set
        {
            Debug.Log($"Durability damaged for {_curDurability - value}, currently {value} durability");
            _curDurability = value < 0 ? 0 : value > _maxDurability ? _maxDurability : value;
            if (_curDurability == 0)
            {
                Dead?.Invoke(_rigidbody);
                Destroy(gameObject);
            }
            DurabilityChanged?.Invoke(_curDurability);
        }
    }
    
    public float CurShield
    {
        get => _curShield;
        set
        {
            Debug.Log($"Shield damaged for {_curShield - value}, currently {value} shield");
            _curShield = value < 0 ? 0 : value > _maxShield ? _maxShield : value;
            _shieldExhausted = _curShield == 0;
            _shieldFull = value >= _maxShield;
            ShieldChanged?.Invoke(_curShield);
        }
    }

    private void Awake()
    {
        _transform = transform;
        _audioEngines = GetComponent<AudioSource>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = maxAngularVelocity;
        _rigidbody.maxLinearVelocity = maxLinearVelocity;

        _throttlePower = statsSO.ThrottlePower;
        _steeringPower = statsSO.SteeringPower;
        _curDurability = _maxDurability = statsSO.MaxDurability;
        _curShield = _maxShield = statsSO.MaxShield;
        _laserDamage = statsSO.LaserDamage;
        _missileDamage = statsSO.MissileDamage;
        _missileRange = statsSO.MissileLockRange;
        _missileLockAngle = statsSO.MissileLockAngle;
        _missileLockTimer = _missileLockTime = statsSO.MissileLockTime;
        _shieldRegenDelay = statsSO.ShieldRegenDelay;
        _shieldRegenRate = statsSO.ShieldRegenRate;
        _targetLocateRange = statsSO.TargetLocateRange;

        GetComponent<SphereCollider>().radius = _targetLocateRange;
    }

    private protected void Start()
    {
        foreach (var blaster in blasters)
        {
            var blasterStruct = new Blaster
            {
                Transform = blaster,
                LineRenderer = blaster.GetComponent<LineRenderer>(),
                ParticleSystem = blaster.GetComponent<ParticleSystem>(),
                AudioSource = blaster.GetComponent<AudioSource>()
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
                if (Physics.Raycast(blaster.Transform.position, blaster.Transform.forward, out _laserHit, Mathf.Infinity, hitMask))
                {
                    blaster.LineRenderer.SetPosition(1, Vector3.forward * _laserHit.distance);
                    _laserHit.transform.GetComponent<BaseShipController>().Damage(_laserDamage, false, 0.5f);
                }
                else
                {
                    blaster.LineRenderer.SetPosition(1, Vector3.forward * 1000);
                }
            }
        }
        
        if (_shieldRegenTimer <= 0 && !_shieldFull)
        {
            CurShield += _shieldRegenRate * Time.deltaTime;
        }
        else
        {
            _shieldRegenTimer -= Time.deltaTime;
        }
        
        UpdateMissileLock();
        
        _audioEngines.volume = _throttle / 2;
        _audioEngines.pitch = 0.9f + _throttle / 10;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnemy ? other.TryGetComponent(out PlayerShipController _) : other.TryGetComponent(out EnemyShipController _) && !_targets.Contains(other.GetComponent<Rigidbody>()))
        {
            var otherRigidbody = other.GetComponent<Rigidbody>();
            _targets.Add(otherRigidbody);
            TargetLocated?.Invoke(otherRigidbody, true);
            other.GetComponent<BaseShipController>().Dead += ProcessDeadTarget;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isEnemy ? other.TryGetComponent(out PlayerShipController _) : other.TryGetComponent(out EnemyShipController _) && _targets.Contains(other.GetComponent<Rigidbody>()))
        {
            var otherRigidbody = other.GetComponent<Rigidbody>();
            _targets.Remove(otherRigidbody);
            TargetLocated?.Invoke(otherRigidbody, false);
            other.GetComponent<BaseShipController>().Dead -= ProcessDeadTarget;
        }
    }

    private void UpdateMissileLock()
    {
        if (_isMissileTracking && _lockedTarget is not null)
        {
            var error = _lockedTarget.position - _transform.position;
            var errorDir = Quaternion.Inverse(_transform.rotation) * error.normalized;

            if (_targets.Contains(_lockedTarget) && Vector3.Angle(Vector3.forward, errorDir) <= _missileLockAngle)
            {
                _missileLockTimer -= Time.fixedDeltaTime;
                TargetLocked?.Invoke(_lockedTarget, _missileLockTimer <= 0);
            }
            else
            {
                _isMissileTracking = false;
                _missileLockTimer = _missileLockTime;
                _lockedTarget = null;
                TargetLocked?.Invoke(_lockedTarget, false);
            }
        }
        else if (_targets.Count > 0)
        {
            float sqrMag = 0;
            foreach (var target in _targets)
            {
                var error = target.position - _transform.position;
                var errorDir = Quaternion.Inverse(_transform.rotation) * error.normalized;

                if (Vector3.Angle(Vector3.forward, errorDir) <= _missileLockAngle) {
                    if (_lockedTarget is null || error.sqrMagnitude < sqrMag)
                    {
                        _lockedTarget = target;
                        sqrMag = error.sqrMagnitude;
                        _isMissileTracking = true;
                    }
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
                    blaster.AudioSource.enabled = _isShootingLaser;
                }
                break;
            case Equipment.Missile:
                if (toggle && missileCount > 0)
                {
                    var launcher = missileLaunchers[missileCount % missileLaunchers.Count];
                    var launchedMissile = Instantiate(missile, launcher.position, launcher.rotation).GetComponent<Missile>();
                    if (_missileLockTimer <= 0)
                    {
                        launchedMissile.Launch(this, _lockedTarget);
                        _lockedTarget.GetComponent<BaseShipController>().Dead += launchedMissile.ProcessDeadTarget;
                    }
                    else
                    {
                        launchedMissile.Launch(this, null);
                    }
                    launcher.GetComponent<AudioSource>().Play();
                    missileCount--;
                }
                break;
            case Equipment.Manipulator:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Damage(float value, bool bypassShield = false, float shieldMultiplier = 1, float durabilityMultiplier = 1)
    {
        if (bypassShield || _shieldExhausted)
        {
            CurDurability -= value * durabilityMultiplier;
        }
        else
        {
            CurShield -= value * shieldMultiplier;
            _shieldRegenTimer = _shieldRegenDelay;
        }
    }

    private void ProcessDeadTarget(Rigidbody target)
    {
        _targets.Remove(target);
        if (_lockedTarget == target)
        {
            _lockedTarget = null;
            _missileLockTimer = _missileLockTime;
        }
        TargetLocated?.Invoke(target, false);
        TargetLocked?.Invoke(target, false);
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
    public AudioSource AudioSource;
}