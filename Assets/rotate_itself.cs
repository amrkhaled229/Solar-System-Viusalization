using UnityEngine;

public class SelfRotation : MonoBehaviour
{
    public float rotationSpeedX = 10f; // Speed of rotation around the X-axis
    public float rotationSpeedY = 20f; // Speed of rotation around the Y-axis

    void Update()
    {
        // Rotate the object around the X-axis and Y-axis
        transform.Rotate(rotationSpeedX * Time.deltaTime, rotationSpeedY * Time.deltaTime, 0);
    }
}
