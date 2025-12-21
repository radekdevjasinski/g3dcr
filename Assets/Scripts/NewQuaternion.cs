using UnityEngine;

public struct NewQuaternion
{
    public float w, x, y, z;

    public NewQuaternion(float w, float x, float y, float z)
    {
        this.w = w; this.x = x; this.y = y; this.z = z;
    }

    // tworzenia kwaterniona z osi i kąta (Angle-Axis)
    public static NewQuaternion FromAngleAxis(float angleDeg, Vector3 axis)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float halfAngle = angleRad / 2f;
        float s = Mathf.Sin(halfAngle);

        return new NewQuaternion(
            Mathf.Cos(halfAngle),
            axis.x * s,
            axis.y * s,
            axis.z * s
        );
    }

    // mnożenie kwaternionów (składanie rotacji)
    public static NewQuaternion operator *(NewQuaternion a, NewQuaternion b)
    {
        return new NewQuaternion(
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
            a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w
        );
    }
    // normalizacja kwaternionu (redukcja błędów numerycznych)
    public NewQuaternion Normalize()
    {
        float magnitude = Mathf.Sqrt(w * w + x * x + y * y + z * z);
        if (magnitude < 0.0001f) return new NewQuaternion(1, 0, 0, 0);
        return new NewQuaternion(w / magnitude, x / magnitude, y / magnitude, z / magnitude);
    }
    // konwersja na format Unity, aby silnik mógł go użyć
    public Quaternion ToUnityQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}