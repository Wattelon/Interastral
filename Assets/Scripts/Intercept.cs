using UnityEngine;

public class Intercept : MonoBehaviour
{
    public static Vector3 FirstOrderIntercept(
        Vector3 shooterPosition,
        Vector3 shooterVelocity,
        float shotSpeed,
        Vector3 targetPosition,
        Vector3 targetVelocity
    )
    {
        var targetRelativePosition = targetPosition - shooterPosition;
        var targetRelativeVelocity = targetVelocity - shooterVelocity;
        var t = FirstOrderInterceptTime(
            shotSpeed,
            targetRelativePosition,
            targetRelativeVelocity
        );
        return targetPosition + t * (targetRelativeVelocity);
    }

    private static float FirstOrderInterceptTime(
        float shotSpeed,
        Vector3 targetRelativePosition,
        Vector3 targetRelativeVelocity
    )
    {
        var velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
        {
            return 0f;
        }

        var a = velocitySquared - shotSpeed * shotSpeed;
        
        if (Mathf.Abs(a) < 0.001f)
        {
            var t = -targetRelativePosition.sqrMagnitude /
                    (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
            return Mathf.Max(t, 0f);
        }

        var b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        var c = targetRelativePosition.sqrMagnitude;
        var determinant = b * b - 4f * a * c;

        switch (determinant)
        {
            case > 0f:
            {
                float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                    t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
                if (t1 > 0f)
                {
                    return t2 > 0f ? Mathf.Min(t1, t2) : t1;
                }
                else
                {
                    return Mathf.Max(t2, 0f);
                }
            }
            case < 0f:
                return 0f;
            default:
                return Mathf.Max(-b / (2f * a), 0f);
        }
    }
}