using UnityEngine;

public class ShipCollision : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionVelocityThreshold;
 
    private BaseShipController baseShip;

    private void Awake()
    {
        baseShip = transform.GetComponent<BaseShipController>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (LayerMaskUnpack.IsLayerInMask(other.gameObject.layer, collisionLayer) && other.relativeVelocity.sqrMagnitude >= Mathf.Pow(collisionVelocityThreshold, 2))
        {
            baseShip.Damage(other.relativeVelocity.sqrMagnitude / 100, true);
        }
    }
}