using UnityEngine;

public class ShipCollision : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionVelocityThreshold;
 
    private ShipController _ship;

    private void Awake()
    {
        _ship = transform.GetComponent<ShipController>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (LayerMaskUnpack.IsLayerInMask(other.gameObject.layer, collisionLayer) && other.relativeVelocity.sqrMagnitude >= Mathf.Pow(collisionVelocityThreshold, 2))
        {
            _ship.CurDurability -= other.relativeVelocity.sqrMagnitude / 100;
        }
    }
}