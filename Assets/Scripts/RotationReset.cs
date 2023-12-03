using UnityEngine;

public class RotationReset : MonoBehaviour
{
    public void ResetY()
    {
        var rotation = transform.localRotation.eulerAngles;
        rotation.y = 0;
        transform.localRotation = Quaternion.Euler(rotation);
    }
}
