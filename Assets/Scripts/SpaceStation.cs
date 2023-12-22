using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceStation : MonoBehaviour
{
    private PlayerShipController _player;
    private bool _repair;

    private void Start()
    {
        _player = FindObjectOfType<PlayerShipController>();
    }

    private void Update()
    {
        if (_repair)
        {
            _player.Damage(-5 * Time.deltaTime, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _player.gameObject)
        {
            _repair = true;
            StartCoroutine(ReplenishMissiles());
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _player.gameObject)
        {
            _repair = false;
        }
    }

    private IEnumerator ReplenishMissiles()
    {
        while (_repair)
        {
            if (_player.missileCount < 8)
            {
                _player.missileCount++;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
