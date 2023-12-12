using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Ship Stats", fileName = "New Stats", order = 51)]
public class ShipStatsSO : ScriptableObject
{
    [SerializeField] private float maxDurability = 100;
    [SerializeField] private float maxShield = 100;
    [SerializeField] private float throttlePower = 5;
    [SerializeField] private float steeringPower = 1;
    [SerializeField] private float laserDamage = 0.1f;
    [SerializeField] private float missileDamage = 25;
    [SerializeField] private float shieldRegenDelay = 5;
    [SerializeField] private float shieldRegenRate = 5;

    public float MaxDurability => maxDurability;
    public float MaxShield => maxShield;
    public float ThrottlePower => throttlePower;
    public float SteeringPower => steeringPower;
    public float LaserDamage => laserDamage;
    public float MissileDamage => missileDamage;
    public float ShieldRegenDelay => shieldRegenDelay;
    public float ShieldRegenRate => shieldRegenRate;
}