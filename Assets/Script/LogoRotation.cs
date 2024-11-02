

using UnityEngine;

namespace Script
{
    
public class LogoRotation : MonoBehaviour 
{
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
}