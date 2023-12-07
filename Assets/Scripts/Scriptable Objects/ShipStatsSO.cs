using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Ship Stats", fileName = "New Stats", order = 51)]
public class ShipStatsSO : ScriptableObject
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxShields;
    [SerializeField] private float throttlePower;
    [SerializeField] private float steeringPower;

    public float MaxHealth => maxHealth;
    public float MaxShields => maxShields;
    public float ThrottlePower => throttlePower;
    public float SteeringPower => steeringPower;
}
