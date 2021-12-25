using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PointingDirectedSteeringNavigation : MonoBehaviour
{
    // 3D pointing directed steering navigation for a VR user

    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Single triggerAction;
    public float maxSpeed = 5.0f; // meters per second
    private GameObject controllerObject;
    private bool floorBound = true;
    private void Start()
    {
        controllerObject = GameObject.Find("/ViewingSetup/Platform/ControllerRight"); 
    }
    void Update()
    {
        float inputValue = triggerAction.GetAxis(handType);
       
        if (inputValue > 0.0f)
        {   
            Vector3 controllerForward = controllerObject.transform.rotation * Vector3.forward;

            if (floorBound)
            {
                controllerForward.y = 0.0f;
            }
            
            Vector3 platformMovement = controllerForward.normalized * inputValue * maxSpeed * Time.deltaTime;
            this.transform.position += platformMovement;
        }

    }

    void GroundFollowingFunc()  // function to keep avatar grounded
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.2f))
        {
            this.transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        }
        if (!Physics.Raycast(transform.position, Vector3.down, 0.3f))
        {
            this.transform.Translate(Vector3.down * Time.deltaTime * 3, Space.World);
        }
    }
}
