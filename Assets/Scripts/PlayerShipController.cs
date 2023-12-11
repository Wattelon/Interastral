using UnityEngine.XR.Content.Interaction;

public class PlayerShipController : ShipController
{
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
}