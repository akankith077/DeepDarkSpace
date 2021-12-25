using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopCameraControls : MonoBehaviour
{
    // Keyboard and mouse navigation for a desktop camera

    public float speed = 5.0f; // meters per second
    public float angularSpeed = 60.0f; // degrees per second
    public KeyCode navigationBindingKey = KeyCode.Space;
    public KeyCode rotationKey = KeyCode.LeftShift;
    private float distanceToGround = 1.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private float maxPitch = 90;
    private bool floorBound = true;

    void Update()
    {
        float verticalPositionChange = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float horizontalPositionChange = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        UpdatePosition(verticalPositionChange, horizontalPositionChange);

        if (Input.GetKey(rotationKey))
        {
            UpdateRotation();
        }

        if (Input.GetKeyUp(navigationBindingKey))
        {
            Debug.Log("The Navigation Key has been pressed");
            floorBound = !floorBound;
        }
        
    }


    void UpdatePosition(float verticalPositionChange, float horizontalPositionChange)
    {
        Vector3 forward = this.transform.forward;
        Vector3 right = this.transform.right;
        if (floorBound)
        {
            forward = new Vector3(forward.x, 0.0f, forward.z);
            right = new Vector3(right.x, 0.0f, right.z);
            GroundFollowing();
        }

        this.transform.position += forward * verticalPositionChange;
        this.transform.position += right * horizontalPositionChange;
    }

    void UpdateRotation()
    {
        float newYaw = yaw + (Time.deltaTime * angularSpeed * Input.GetAxis("Mouse X"));
        float newPitch = pitch - (Time.deltaTime * angularSpeed * Input.GetAxis("Mouse Y"));

        if ((-maxPitch) <= newPitch && newPitch <= maxPitch)
        {
            pitch = newPitch;
        }
        yaw = newYaw;

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    void GroundFollowing()
    {
        if (Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.2f))
        {
            transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        }
        else if (!Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.3f))
        {
            transform.Translate(Vector3.down * Time.deltaTime, Space.World);
        }
    }
}
