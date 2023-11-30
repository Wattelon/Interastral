using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float density;
    private float _mass;
    private float _radius;

    private void Start()
    {
        _radius = transform.parent.lossyScale.x;
        _mass = Mathf.Pow(_radius, 3) * density;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody collision))
        {
            Debug.Log(other.name);
            var pullDirection = transform.position - collision.transform.position;
            var sqrDistance = pullDirection.sqrMagnitude;
            pullDirection.Normalize();
            var force = _mass / sqrDistance;
            collision.AddForce(pullDirection * force, ForceMode.Force);
        }
    }
}
