using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        throttle = throttleControl.value.y + 1;
    }

    public void SetThrottleVertical()
    {
        isThrottleVertical = !isThrottleVertical;
    }

    public void SetSteering(XRJoystick joystick)
    {
        steering = joystick.value;
    }

    public void TriggerShoot(bool toggle)
    {
        Shoot(toggle);
    }

    public void ToggleEquipment()
    {
        Shoot(false);
        _equipment = _equipment == Equipment.Laser ? Equipment.Missile : Equipment.Laser;
    }

    public void ToggleLinearStabilisation()
    {
        _rigidbody.linearDamping = _rigidbody.linearDamping <= 1 ? 10 : 1;
    }
    
    public void ToggleAngularStabilisation()
    {
        _rigidbody.angularDamping = _rigidbody.angularDamping <= 1 ? 10 : 1;
    }

    private void OnDestroy()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}