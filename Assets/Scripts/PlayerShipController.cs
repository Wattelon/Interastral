using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class PlayerShipController : BaseShipController
{
    [SerializeField] private List<Material> crosshairMaterials;
    [SerializeField] private MeshRenderer crosshair;
    private void Update()
    {
        crosshair.material = crosshairMaterials[_missileLockTimer <= 0 ? 1 : 0];
    }

    public void SetThrottle(XRJoystick throttleControl)
    {
        _throttle = throttleControl.value.y + 1;
    }

    public void SetThrottleVertical()
    {
        _isThrottleVertical = !_isThrottleVertical;
    }

    public void SetSteering(XRJoystick joystick)
    {
        _steering = joystick.value;
    }

    public void TriggerShoot(bool toggle)
    {
        Shoot(toggle);
    }

    public void ToggleEquipment()
    {
        _equipment = _equipment == Equipment.Laser ? Equipment.Missile : Equipment.Laser;
    }
}