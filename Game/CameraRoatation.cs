using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Xml.Serialization;

public class CameraRoatation : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;

    [SerializeField] private Button Up;
    [SerializeField] private Button Down;
    [SerializeField] private Button Left;
    [SerializeField] private Button Right;
    // Buttons for camera movement

    private Vector3 previousPosition;
    // Update is called once per frame
    private float verticalAngle = 0.0f; // Track the vertical rotation angle
    private float maxVerticalAngle = 90.0f; // Maximum angle to rotate upwards
    private float minVerticalAngle = 25f; // Minimum angle to rotate downwards, allowing to go underneath
    
    void Update()
    {    

        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);

        }

        if (Input.GetMouseButton(0))
        {
            // previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 cameraRotation = cam.transform.rotation.eulerAngles;

            // Calculate the new vertical angle, ensuring it stays within the desired range
            verticalAngle += direction.y * 180;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

            // Rotate the camera around the target horizontally
            cam.transform.position = target.position;
            cam.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);

            // Apply the clamped vertical rotation
            cam.transform.rotation = Quaternion.Euler(verticalAngle, cam.transform.rotation.eulerAngles.y, 0);

            // Move the camera back to maintain distance from the target
            cam.transform.Translate(new Vector3(0, 0, -15));

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);

        }

        
    }



    

    

}
