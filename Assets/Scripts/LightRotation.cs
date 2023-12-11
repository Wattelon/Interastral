using UnityEngine;

public class LightRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    private Transform _transform;
    
    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        _transform.Rotate(Vector3.up * rotationSpeed);
    }
}