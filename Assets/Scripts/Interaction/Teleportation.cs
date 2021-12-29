using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;


public class Teleportation : MonoBehaviourPunCallbacks, IPunObservable
{
    public BezierCurve bezier;
    //private CurvedRay curvRAY;
    private bool teleportEnabled;
    private bool timeCheckBool = false;
    private float activityTime = 0.0f;

    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean teleportAction;
    public SteamVR_Action_Boolean teleportConfirmAction;
    public SteamVR_Action_Boolean snapLeftAction;
    public SteamVR_Action_Boolean snapRightAction;
    public SteamVR_Action_Boolean TimerAction;
    public SteamVR_Action_Vibration controllerVibration;

    private GameObject controllerObject;
    private GameObject platformObject;
    private GameObject rotateObject;
    private GameObject teleportIndicator;

    private bool bezierCheck;
    private bool onceCheck;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    void Start()
    {
        if (photonView.IsMine)
        {
            teleportIndicator = GameObject.Find("/ViewingSetup/Platform/TELE");
            teleportIndicator.GetComponent<MeshRenderer>().enabled = false;
            teleportEnabled = false;
            if (this.gameObject.name == "Bezier left")
            {
                controllerObject = GameObject.Find("/ViewingSetup/Platform/ControllerLeft");
            }
            else
            {
                controllerObject = GameObject.Find("/ViewingSetup/Platform/ControllerRight");
            }

            platformObject = controllerObject.gameObject.transform.parent.gameObject;
            rotateObject = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (teleportAction.GetStateDown(handType))
            {
                teleportEnabled = true;
            }

            if (teleportAction.GetStateUp(handType))
            {
                teleportEnabled = false;
                ToggleTeleportMode(teleportEnabled);
            }

            if (teleportEnabled)
            {
                HandleTeleport();
                ToggleTeleportMode(teleportEnabled);
            }

            bezierCheck = bezier.endPointDetected;

            if (TimerAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
            {
                Debug.Log("Button Pressed");
                if (timeCheckBool)
                {
                    timeCheckBool = false;
                    Debug.Log(" ******************** THE WHOLE ACTIVITY TOOK : " + activityTime);
                    activityTime = 0;
                }
                else
                {
                    Debug.Log(" ****************** ACTIVITY STARTED");
                    timeCheckBool = true;
                }
            }
            if (timeCheckBool)
            {
                activityTime += Time.deltaTime;
            }

        }
        else
        {
            ToggleTeleportMode(teleportEnabled);
        }
    }

    void HandleTeleport()
    {
        if (bezierCheck)
        {

            teleportIndicator.transform.position = bezier.EndPoint; //Sets teleport indicator position
            Vector3 IndicatorRotation = new Vector3(0, rotateObject.transform.eulerAngles.y - rotateObject.transform.eulerAngles.z, 0); //Adding controller Z rotation to teleport indicator  
            teleportIndicator.transform.rotation = Quaternion.Euler(IndicatorRotation); //Set teleport indicator rotation mapped to controller rotation
            //teleportIndicator.transform.forward  = rotateObject.transform.forward; 
            teleportIndicator.GetComponent<MeshRenderer>().enabled = true;

            if (teleportConfirmAction.GetStateDown(handType))
            {
                teleportIndicator.GetComponent<MeshRenderer>().enabled = false;

                float ControllerRotation = controllerObject.transform.localEulerAngles.z;
                float HeadRotation = rotateObject.transform.localEulerAngles.y;

                if (ControllerRotation > 180)
                {
                    ControllerRotation = ControllerRotation - 360;
                }
                Vector3 groundPosition = new Vector3(rotateObject.transform.position.x, platformObject.transform.position.y, rotateObject.transform.position.z);

                Vector3 translateVector = bezier.EndPoint - groundPosition;

                platformObject.transform.position += translateVector;

                //platformObject.transform.RotateAround(rotateObject.transform.position, Vector3.up, -ControllerRotation); //Sets the User rotaion as the indicator rotation
            }
        }
    }


    void ToggleTeleportMode(bool check)
    {
        bezier.GetDrawLine(check);

        if (!teleportEnabled && photonView.IsMine)
        {
            teleportIndicator.GetComponent<MeshRenderer>().enabled = false; //set teleport indicator object inactive
        }
    }
    void SnapToTurn()
    {
        if (snapLeftAction.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            //Debug.Log("***SNAP LEFT***");
            platformObject.transform.RotateAround(rotateObject.transform.position, Vector3.up, -45);
        }
        if (snapRightAction.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            //Debug.Log("***SNAP RIGHT***");
            platformObject.transform.RotateAround(rotateObject.transform.position, Vector3.up, 45);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool teleIndiCheck = teleportEnabled;
            stream.SendNext(teleIndiCheck);
        }
        else
        {
            bool teleIndiCheck = (bool)stream.ReceiveNext();
            teleportEnabled = teleIndiCheck;
            bezier.GetDrawLine(teleportEnabled);
        }
    }

}
