using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target; // The target object to orbit around
    public float orbitSpeed = 50.0f; // Speed of orbit
    private float yaw = 0.0f; // Horizontal angle
    private float pitch = 0.0f; // Vertical angle

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Make sure the camera is looking at the target from the start
        if (target != null)
        {
            transform.LookAt(target);
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            // Update camera position based on current yaw and pitch
            transform.position = target.position + Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -10);
            transform.LookAt(target);
        }
    }

    public void RotateLeft()
    {
        Debug.Log("He");
        yaw -= orbitSpeed * Time.deltaTime;
    }

    public void RotateRight()
    {
        yaw += orbitSpeed * Time.deltaTime;
    }

    public void RotateUp()
    {
        pitch = Mathf.Max(pitch - orbitSpeed * Time.deltaTime, -80); // Clamp the pitch to prevent flipping over
    }

    public void RotateDown()
    {
        pitch = Mathf.Min(pitch + orbitSpeed * Time.deltaTime, 80); // Clamp the pitch
    }

    
}
