using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Ship Stats", fileName = "New Stats", order = 51)]
public class ShipStatsSO : ScriptableObject
{
    [SerializeField] private float maxDurability;
    [SerializeField] private float maxShield;
    [SerializeField] private float throttlePower;
    [SerializeField] private float steeringPower;
    [SerializeField] private float laserDamage;
    [SerializeField] private float missileDamage;

    public float MaxDurability => maxDurability;
    public float MaxShield => maxShield;
    public float ThrottlePower => throttlePower;
    public float SteeringPower => steeringPower;
    public float LaserDamage => laserDamage;
    public float MissileDamage => missileDamage;
}