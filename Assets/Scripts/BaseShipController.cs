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
    [SerializeField] public int missileCount;
    [SerializeField] private List<Transform> missileLaunchers;
    [SerializeField] private AudioSource shieldBreak;
    [SerializeField] private AudioSource shieldCharge;
    [SerializeField] private SphereCollider locationSphere;
    [SerializeField] private GameObject explosion;

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
    [SerializeField] private float _missileReloadTime;
    private protected float _missileReloadTimer;
    private float _shieldRegenDelay;
    private float _shieldRegenRate;
    private float _shieldRegenTimer;
    private float _targetLocateRange;
    private bool _isMissileTracking;
    private bool _shieldExhausted;
    private bool _shieldCharging;
    private bool _shieldFull = true;
    private Rigidbody _lockedTarget;
    private RaycastHit _laserHit;
    private AudioSource _audioEngines;
    private readonly List<Blaster> _blasters = new();
    private protected readonly List<Rigidbody> _targets = new();
    [SerializeField] private protected bool _isShootingLaser;
    [SerializeField] private protected Vector2 steering;
    [SerializeField] private protected float throttle;
    [SerializeField] private protected bool isThrottleVertical;
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

    private float curDurability
    {
        get => _curDurability;
        set
        {
            //Debug.Log($"Durability damaged for {_curDurability - value}, currently {value} durability");
            _curDurability = value < 0 ? 0 : value > _maxDurability ? _maxDurability : value;
            if (_curDurability == 0)
            {
                Dead?.Invoke(_rigidbody);
                Instantiate(explosion, _transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            DurabilityChanged?.Invoke(_curDurability);
        }
    }

    private float curShield
    {
        get => _curShield;
        set
        {
            //Debug.Log($"Shield damaged for {_curShield - value}, currently {value} shield");
            _curShield = value < 0 ? 0 : value > _maxShield ? _maxShield : value;
            _shieldExhausted = _curShield == 0;
            if (!isEnemy && _shieldExhausted)
            {
                shieldBreak.Play();
            }
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

        locationSphere.radius = _targetLocateRange;
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
        var direction = isThrottleVertical ? _transform.up : _transform.forward;
        _rigidbody.AddForce(direction * (Mathf.Pow(throttle, 2) * _throttlePower), ForceMode.Acceleration);
        _rigidbody.AddTorque(_transform.forward * (-steering.x * _steeringPower), ForceMode.Acceleration);
        _rigidbody.AddTorque(_transform.right * (steering.y * _steeringPower), ForceMode.Acceleration);

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
            curShield += _shieldRegenRate * Time.deltaTime;
            if (!isEnemy && !_shieldCharging)
            {
                shieldCharge.Play();
                _shieldCharging = true;
            }
        }
        else
        {
            _shieldRegenTimer -= Time.deltaTime;
            _shieldCharging = false;
        }
        
        UpdateMissileLock();
        
        _audioEngines.volume = throttle / 2;
        _audioEngines.pitch = 0.9f + throttle / 10;
        _missileReloadTimer -= Time.fixedDeltaTime;
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
                TargetLocked?.Invoke(_lockedTarget, false);
                _lockedTarget = null;
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
                if (toggle && missileCount > 0 && _missileReloadTimer <= 0)
                {
                    var launcher = missileLaunchers[missileCount % missileLaunchers.Count];
                    var launchedMissile = Instantiate(missile, launcher.position, launcher.rotation).GetComponent<Missile>();
                    if (_missileLockTimer <= 0)
                    {
                        launchedMissile.Launch(this, _lockedTarget);
                        _lockedTarget.GetComponent<BaseShipController>().Dead += launchedMissile.ProcessDeadTarget;
                        _missileLockTimer = _missileLockTime / 2;
                    }
                    else
                    {
                        launchedMissile.Launch(this, null);
                        _missileLockTimer = _missileLockTime;
                    }
                    launcher.GetComponent<AudioSource>().Play();
                    _missileReloadTimer = _missileReloadTime;
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
            curDurability -= value * durabilityMultiplier;
        }
        else
        {
            curShield -= value * shieldMultiplier;
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