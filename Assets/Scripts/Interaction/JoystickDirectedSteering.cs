using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class JoystickDirectedSteering : MonoBehaviour
{
    // 3D pointing directed steering navigation for a VR user
    public SteamVR_Input_Sources handType1;
    public SteamVR_Input_Sources handType2;
    public SteamVR_Action_Vector2 joystickPrimary;
    public SteamVR_Action_Vector2 joystickSecondary;
    private float maxSpeed = 3.0f; // meters per second
    private float maxRotateSpeed = 33f; // per snap

    private bool floorBound = true;
    private float MIN_TURN_TIME = .25f; // in seconds
    public float lastTurnTime;


    void Start()
    {
        lastTurnTime = Time.time;
    }
    
    void Update()
    {
        if (joystickPrimary != null && joystickSecondary != null)
        {
            Vector2 joystickPrimaryValue = joystickPrimary.GetAxis(handType2);
            Vector2 joystickSecondaryValue = joystickSecondary.GetAxis(handType1);

            if (joystickPrimaryValue.x != 0 || joystickPrimaryValue.y != 0)
            {
                GameObject HMD = GameObject.Find("HMDCamera");
                Vector3 joystickRight = HMD.transform.rotation * Vector3.right;
                Vector3 joystickForward = HMD.transform.rotation * Vector3.forward;
                if (floorBound)
                {
                    joystickForward.y = 0.0f;
                    joystickRight.y = 0.0f;
                }
                this.transform.position += joystickForward.normalized * joystickPrimaryValue.y * maxSpeed * Time.deltaTime;
                this.transform.position += joystickRight.normalized * joystickPrimaryValue.x * maxSpeed * Time.deltaTime;
            }
            if (joystickSecondaryValue.x != 0 && lastTurnTime + MIN_TURN_TIME < Time.time)
            {
                lastTurnTime = Time.time;
                Quaternion target = Quaternion.Euler(0, joystickSecondaryValue.x * maxRotateSpeed, 0);
                this.transform.rotation *= target;
                //this.transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * target, 1);
            }
        }
    }
}
