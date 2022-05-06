using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Valve.VR;

public class CarpetNav : MonoBehaviourPunCallbacks
{

    public SteamVR_Input_Sources handType;
    public BezierCurve bezier;
    public LoggingScript logScript;

    public SteamVR_Action_Boolean groupTeleportationActive;
    public SteamVR_Action_Boolean groupTeleportationConfirm;
    public SteamVR_Action_Boolean backToCar;
    public SteamVR_Action_Boolean groupCancel;
    public SteamVR_Action_Boolean navigatorToggle;
    public SteamVR_Action_Boolean circleFormAction;
    public SteamVR_Action_Boolean presenterFormAction;
    public SteamVR_Action_Boolean semiCircFormAction;
    public SteamVR_Action_Boolean loggingButton;

    public GameObject platformObj;
    public GameObject hmdObj;
    public GameObject carpetObj = null;
    public GameObject oldCarpet;
    public GameObject teleportIndicator;
    private GameObject helmetObj;
    private GameObject leftHand;
    private GameObject leftWristBand;
    private GameObject rightWristBand;
    private GameObject navTag;
    private GameObject arrowObj;
    private GameObject PosObj;
    private GameObject backToCarObj;
    private GameObject LoggingObj;
    private Vector3 locationObj;
    public Vector3 fireLocation;
    public Vector3 foodlocation;


    public Vector3 nextPos;
    private Vector3 edgeCorrection = new Vector3(1f, 0f, 1f);
    public List<int> passengers = new List<int>();
    private List<Vector3> carpetPosList = new List<Vector3>();
    private int[] passengerIDs = { };
    private int index = 0;
    private int f = 0;
    private float myID = 0;
    private float otherID = 0;
    private float ControllerRotation = 0;
    public float highFiveHeight = 1.1f;
    private float myRotAngles = 0.0f;
    private float cntrlrRotScale = 1.5f;
    private float carpetOldScale = 0.0f;
    private float passenger1 = 0.0f;
    private float passenger2 = 0.0f;

    private GameObject ViewPort;
    private Vector3 smallScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 normalScale = new Vector3(1f, 1f, 1f);
    private Vector3 lookAtPos = new Vector3(0f, 0f, 0f);


    public bool onCarpet = false;
    public bool grouped = false;
    public bool navigatorMode = false;
    public bool carpIsMine = false;
    private bool logCheck = false;

    private bool backToCarCheck = false;
    private bool teleButtonCheck = false;
    private bool locationObjFound = true;
    private bool check = false;
    public bool cicrleForm = false;
    public bool semiCircForm = false;
    public bool presenterForm = false;
    public bool joystickButtonActive = false;
    private bool groupTeleButton = false;
    private bool carpetMoveLock = false;

    public Material gold;
    public Material old;

    public Material invisible;
    public Material glow;

    void Start()
    {
        if (photonView.IsMine)
        {
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            ViewPort = GameObject.Find("/ViewingSetup");
            hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");

            helmetObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera/Robot/CameraTranslateOnly/RobotHead/Helmet");
            teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
            arrowObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera/arrow");
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
            leftHand = GameObject.Find("/ViewingSetup/Platform/ControllerLeft/ComicHandLeft(Clone)");
            leftWristBand = this.transform.GetChild(2).gameObject;
            rightWristBand = leftHand.transform.GetChild(2).gameObject;
            backToCarObj = leftHand.transform.GetChild(7).gameObject;
            LoggingObj = leftHand.transform.GetChild(8).gameObject;
            navTag = this.transform.GetChild(4).gameObject;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            ButtonCheck();
            ControllerRot();
            JoysitckCheck();
            myRotAngles = hmdObj.transform.localEulerAngles.y;
            if (carpetObj != null) //To check the owner of the carpet
            {
                float scaleVal = carpetObj.transform.localScale.x;
                edgeCorrection = new Vector3(carpetObj.transform.localScale.x - (scaleVal / 2), 0.0f, carpetObj.transform.localScale.x - (scaleVal / 2));
            }

            if (navigatorMode) //Navigator Mode Indication
            {
                navTag.GetComponent<Canvas>().enabled = true;
            }
            else
            {
                navTag.GetComponent<Canvas>().enabled = false;
            }

            if (teleButtonCheck && carpetObj != null && !joystickButtonActive) //Tele Indicator Activation
            {
                if (!navigatorMode)
                {
                    if (!carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        carpetObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    GroupTeleActive();
                }
                else
                {
                    if (carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        GroupTeleActive();
                    }
                }
            }
            if (groupTeleportationActive.GetStateUp(handType) && carpetObj != null) //Tele Indicator De-Activation
            {

                if (!navigatorMode)
                {
                    GroupTeleDeactivate();
                    if (carpetObj.GetComponent<PhotonView>().CreatorActorNr != myID)
                    {
                        carpetObj.GetComponent<PhotonView>().TransferOwnership(carpetObj.GetComponent<PhotonView>().CreatorActorNr);
                        carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(carpetObj.GetComponent<PhotonView>().CreatorActorNr);
                    }
                }
                else
                {
                    if (carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        GroupTeleDeactivate();
                    }
                }
            }
            if (cicrleForm && carpetObj != null) //Circle fromation
            {
                if (!navigatorMode)
                {
                    if (!carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        carpetObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    CircleFormation();
                }
            }
            if (semiCircForm && carpetObj != null) //Circle fromation
            {
                if (!navigatorMode)
                {
                    if (!carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        carpetObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    SemiCircleFormation();
                }
            }
            if (presenterForm && carpetObj != null) //Circle fromation
            {
                if (!navigatorMode)
                {
                    if (!carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        carpetObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    PresenterFormation();
                }
            }
            if (groupTeleButton && carpetObj != null)
            {
                float scale = transform.parent.transform.eulerAngles.x / 70f;
                carpetObj.transform.localScale = new Vector3(carpetOldScale + scale, 1.0f, carpetOldScale + scale);
            }

            if (groupTeleportationConfirm.GetStateUp(handType) && carpetObj != null) //Group Teleportaion 
            {
                if (!navigatorMode)
                {
                    GroupTeleportation();
                }
                else
                {
                    if (carpetObj.GetComponent<PhotonView>().IsMine)
                    {
                        GroupTeleportation();
                    }
                }
            }


            if (carpetObj != null) // Grouping wrist band indication 
            {
                //rightWristBand.GetComponent<MeshRenderer>().enabled = true;
                //leftWristBand.GetComponent<MeshRenderer>().enabled = true;
                passengers = carpetObj.GetComponent<pplOnCar>().carpetList;
                passengerIDs = passengers.ToArray();
                Array.Sort(passengerIDs);
            }

            if (!grouped)
            {
                rightWristBand.GetComponent<MeshRenderer>().enabled = false;
                leftWristBand.GetComponent<MeshRenderer>().enabled = false;
                carpetObj = null;
                oldCarpet = null;
            }
            if (onCarpet == false && grouped == true) //BACK TO CARPET 
            {
                BackToCarGlow();
            }

            if (onCarpet == false && grouped == true && backToCarCheck == true) //BACK TO CARPET 
            {
                carpetPosList = oldCarpet.transform.GetComponent<pplOnCar>().carpetPosList;
                nextPos = carpetPosList[index];
                teleportIndicator.transform.position = nextPos + edgeCorrection;

                teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;

                teleportIndicator.transform.GetComponent<CurvedRay>().GetDrawLine(true);
                Vector3 hmdRot = new Vector3(0, hmdObj.transform.eulerAngles.y - hmdObj.transform.eulerAngles.z, 0); //Adding controller Z rotation REMOVE AND ADD CONTRLR ROT
                teleportIndicator.transform.rotation = Quaternion.Euler(hmdRot);
            }

            else if (onCarpet == false && grouped == true && backToCar.GetStateUp(SteamVR_Input_Sources.LeftHand)) //BACK TO CARPET 
            {
                teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
                teleportIndicator.transform.GetComponent<CurvedRay>().GetDrawLine(false);
                Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);

                Vector3 translateVector = teleportIndicator.transform.position - groundPosition;

                platformObj.transform.position += translateVector;
                logScript.backToCarpetData(platformObj.transform.position);
                if (index < carpetPosList.Count)
                {
                    index++;
                }
            }

            if (locationObjFound)
            {
                arrowObj.transform.LookAt(locationObj, Vector3.up);
            }
        }
    }
    public void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        if (photonView.IsMine)
        {
            if (collision.gameObject.name == "carpet(Clone)")
            {
                if (collision.gameObject.transform.localScale.x > 0.2)
                {
                    carpetObj = collision.gameObject;
                    oldCarpet = carpetObj;
                    grouped = true;
                    onCarpet = true;
                }
                if (backToCarObj != null)
                {
                    backToCarObj.GetComponent<MeshRenderer>().material = invisible;
                }
            }
            if (collision.gameObject.name == "Food")
            {
                locationObj = foodlocation;
                locationObjFound = true;
                arrowObj.transform.GetChild(0).transform.GetComponent<MeshRenderer>().enabled = true;
            }
            else if (collision.gameObject.name == "Fire")
            {
                locationObj = fireLocation;
                locationObjFound = true;
                arrowObj.transform.GetChild(0).transform.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
    public void OnCollisionExit(Collision collision) //*************** When user exits the carpet
    {
        if (photonView.IsMine)
        {
            if (collision.gameObject.name == "carpet(Clone)")
            {
                if (collision.gameObject.transform.localScale.x > 0.2)
                {
                    if (grouped)
                    {
                        index = oldCarpet.transform.GetComponent<pplOnCar>().carpetPosList.Count - 1;
                    }
                    carpetObj = null;
                    Debug.Log("Carpet object Null");
                    onCarpet = false;
                    teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(false);
                }
            }
            if (collision.gameObject.name == "ComicHandRight(Clone)")
            {
                if (carpetObj != null && !carpIsMine && navigatorMode)
                {
                    //Debug.Log("Collided With other hand at height : " + collision.gameObject.transform.position.y);
                    if (collision.gameObject.transform.position.y > highFiveHeight)
                        TransferOwner();
                }
            }
        }
    }

    public void ControllerRot()
    {
        if (joystickButtonActive)
        {
            ControllerRotation = (this.transform.parent.transform.localEulerAngles.z);
            ControllerRotation = ControllerRotation - 180;
        }
    }
    public void carpetOwnershipCheck()
    {
        if (carpetObj != null && carpetObj.GetComponent<PhotonView>().IsMine)
        {
            carpIsMine = true;
            if (navigatorMode)
            {
                helmetObj.GetComponent<MeshRenderer>().enabled = false;
                rightWristBand.GetComponent<Renderer>().material = old;
                leftWristBand.GetComponent<Renderer>().material = old;
            }
            else
            {
                helmetObj.GetComponent<MeshRenderer>().enabled = false;
                rightWristBand.GetComponent<Renderer>().material = old;
                leftWristBand.GetComponent<Renderer>().material = old;
            }
        }
        else
        {
            carpIsMine = false;
            if (navigatorMode)
            {
                helmetObj.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public void ButtonCheck()
    {
        if (groupTeleportationActive.GetStateDown(handType))
        {
            teleButtonCheck = true;
        }
        else if (groupTeleportationActive.GetStateUp(handType))
        {
            teleButtonCheck = false;
        }

        if (backToCar.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            backToCarCheck = true;
        }
        else if (backToCar.GetStateUp(SteamVR_Input_Sources.LeftHand))
        {
            backToCarCheck = false;
        }

        if (groupCancel.GetStateUp(handType))
        {
            grouped = false;
        }

        if (circleFormAction.GetStateDown(handType))
        {
            cicrleForm = true;
            if (logCheck && onCarpet)
            {
                logScript.addData(1, carpetObj.transform.position);
            }
        }
        else if (circleFormAction.GetStateUp(handType))
        {
            cicrleForm = false;
            teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(false);
            if (carpetObj != null)
            {
                GroupTeleDeactivate();
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
            teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        if (semiCircFormAction.GetStateDown(handType))
        {
            semiCircForm = true;

            if (logCheck && onCarpet)
            {
                logScript.addData(2, carpetObj.transform.position);
            }
        }
        else if (semiCircFormAction.GetStateUp(handType))
        {
            semiCircForm = false;
            teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(false);
            if (carpetObj != null)
            {
                GroupTeleDeactivate();
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
            teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        if (presenterFormAction.GetStateDown(handType))
        {
            presenterForm = true;
            if (logCheck && onCarpet)
            {
                logScript.addData(3, carpetObj.transform.position);
            }
        }
        else if (presenterFormAction.GetStateUp(handType))
        {
            presenterForm = false;
            teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(false);
            if (carpetObj != null)
            {
                GroupTeleDeactivate();
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
            teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        if (groupTeleportationConfirm.GetStateUp(handType))
        {
            if (logCheck && onCarpet)
            {
                logScript.addData(4, carpetObj.transform.position);
            }
            GroupTeleDeactivate();
            carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }

        if (navigatorToggle.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (navigatorMode)
            {
                navigatorMode = false;
            }
            else
            {
                navigatorMode = true;
            }
        }

        if (loggingButton.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            Debug.Log("Button Pressed");
            if (logCheck)
            {
                logCheck = false;
                Debug.Log(" ******************** Logging deactive");
                backToCarObj.GetComponent<MeshRenderer>().material = invisible;
                logScript.printData();
            }
            else
            {
                Debug.Log(" ******************** Logging active");
                logCheck = true;
                backToCarObj.GetComponent<MeshRenderer>().material = glow;
            }
        }
    }

    public void JoysitckCheck()
    {
        if (cicrleForm || semiCircForm || presenterForm)
        {
            joystickButtonActive = true;
        }
        else
        {
            joystickButtonActive = false;
        }
    }

    public void BackToCarGlow()
    {
        if (Time.fixedTime % .5 < .2)
        {
            backToCarObj.GetComponent<MeshRenderer>().material = invisible;
        }
        else
        {
            backToCarObj.GetComponent<MeshRenderer>().material = glow;
        }

    }

    private float AngleDifference(float Angle1, float Angle2)
    {
        float Difference = Angle2 - Angle1;
        if (Difference > 180) Difference -= 360;
        else if (Difference < -180) Difference += 360;
        return Difference;
    }

    public void TransferOwner()
    {
        if (carpetObj != null)
        {
            if (!carpetObj.GetComponent<PhotonView>().IsMine)
            {
                carpetObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                carpetObj.transform.GetChild(1).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                string shirtColor = PlayerPrefs.GetString(GlobalSettings.colorPrefKey);
                carpetObj.GetComponent<pplOnCar>().ChangeCarColour(shirtColor);
            }
        }
    }
    public void GroupTeleActive()
    {
        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        Vector3 translateVector = groundPosition - carpetObj.transform.position;
        Vector3 IndicatorRotation = new Vector3(0, this.transform.parent.transform.eulerAngles.y - this.transform.parent.eulerAngles.z, 0);

        translateVector.y = 0f;
        carpetObj.transform.GetChild(0).position = bezier.EndPoint - translateVector;
        carpetObj.transform.GetChild(0).transform.rotation = Quaternion.Euler(IndicatorRotation);
        carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        ControllerRotation = (this.transform.parent.transform.localEulerAngles.z);
        if (ControllerRotation > 180)
        {
            ControllerRotation = ControllerRotation - 360;
        }
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
            carpetObj.transform.GetChild(0).position,
            carpetObj.transform.position,
            ControllerRotation);
        }
    }


    public void GroupTeleDeactivate()
    {
        carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            photonView.RPC("RemoteTeleIndicatorDeAcitve", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), false);
        }
    }

    public void GroupTeleportation()
    {
        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        if (joystickButtonActive && carpetObj != null)
        {
            for (int i = 0; i < passengerIDs.Length; i++)
            {
                photonView.RPC("RemoteTeleportation", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
                    carpetObj.transform.GetChild(0).position,
                    carpetObj.transform.position);

            }
            carpetObj.transform.position = bezier.EndPoint;
        }
        else
        {
            Vector3 translateVector = bezier.EndPoint - groundPosition;
            translateVector.y = 0f;
            for (int i = 0; i < passengerIDs.Length; i++)
            {

                photonView.RPC("RemoteTeleportation", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
                    carpetObj.transform.GetChild(0).position,
                    carpetObj.transform.position);

            }
            carpetObj.transform.position += translateVector;
        }

    }

    public void CircleFormation()
    {
        f = 0;
        float ActorNr = transform.GetComponent<PhotonView>().OwnerActorNr;
        int passengerSize = passengerIDs.Length;
        for (int i = 0; i < passengerSize; i++)
        {
            if (ActorNr == passengerIDs[i])
            {
                PosObj = carpetObj.transform.GetChild(0).transform.GetChild(0).gameObject;
            }
            else
            {
                PosObj = carpetObj.transform.GetChild(0).transform.GetChild(1 + f).gameObject;
                f++;
            }

            Vector3 carRotation = new Vector3(0, transform.parent.transform.rotation.eulerAngles.y - transform.parent.transform.rotation.eulerAngles.z, 0); //Adding controller Z rotation to teleport indicator  

            if (!carpetMoveLock)
            { carpetObj.transform.GetChild(0).position = bezier.EndPoint; }

            carpetObj.transform.GetChild(0).transform.rotation = Quaternion.Euler(carRotation);
            carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            photonView.RPC("RemoteCircleForm", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
                    PosObj.transform.position,
                    carpetObj.transform.GetChild(0).position
                    );
        }
    }

    public void SemiCircleFormation()
    {
        f = 0;
        float ActorNr = transform.GetComponent<PhotonView>().OwnerActorNr;
        int passengerSize = passengerIDs.Length;
        for (int i = 0; i < passengerSize; i++)
        {
            if (passengerSize == 2)
            {
                if (ActorNr == passengerIDs[i])
                {
                    PosObj = carpetObj.transform.GetChild(0).transform.GetChild(1).gameObject;
                }
                else
                {
                    PosObj = carpetObj.transform.GetChild(0).transform.GetChild(2).gameObject;
                }
            }
            else
            {
                if (ActorNr == passengerIDs[i])
                {
                    PosObj = carpetObj.transform.GetChild(0).transform.GetChild(0).gameObject;
                }
                else
                {
                    PosObj = carpetObj.transform.GetChild(0).transform.GetChild(1 + f).gameObject;
                    f++;
                }
            }
            Vector3 carRotation = new Vector3(0, transform.parent.transform.rotation.eulerAngles.y - transform.parent.transform.rotation.eulerAngles.z, 0); //Adding controller Z rotation to teleport indicator  
            if (!carpetMoveLock)
            { carpetObj.transform.GetChild(0).position = bezier.EndPoint; }
            carpetObj.transform.GetChild(0).transform.rotation = Quaternion.Euler(carRotation);
            carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            photonView.RPC("RemoteSemiCircleForm", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
                PosObj.transform.position,
                carpetObj.transform.GetChild(0).transform.forward
                );
        }
    }
    public void PresenterFormation()
    {
        f = 0;
        float ActorNr = transform.GetComponent<PhotonView>().OwnerActorNr;
        int passengerSize = passengerIDs.Length;
        for (int i = 0; i < passengerSize; i++)
        {
            if (ActorNr == passengerIDs[i])
            {
                PosObj = carpetObj.transform.GetChild(0).transform.GetChild(0).gameObject;
                lookAtPos = carpetObj.transform.GetChild(0).transform.GetChild(3).gameObject.transform.position;
            }
            else
            {
                PosObj = carpetObj.transform.GetChild(0).transform.GetChild(4 + f).gameObject;
                lookAtPos = carpetObj.transform.GetChild(0).transform.GetChild(0).gameObject.transform.position;
                f++;
            }

            Vector3 carRotation = new Vector3(0, transform.parent.transform.rotation.eulerAngles.y - transform.parent.transform.rotation.eulerAngles.z, 0); //Adding controller Z rotation to teleport indicator  

            if (!carpetMoveLock)
            { carpetObj.transform.GetChild(0).position = bezier.EndPoint; }

            carpetObj.transform.GetChild(0).transform.rotation = Quaternion.Euler(carRotation);
            carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            photonView.RPC("RemotePresenterForm", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
                    PosObj.transform.position,
                    lookAtPos
                    );
        }
    }


    //**************************************************************************************************************** RPCs

    [PunRPC]
    void RemoteTeleIndicatorAcitve(Vector3 carpetChild, Vector3 carpet, float IndicatorRotation)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
        Vector3 camRot = new Vector3(0.0f, hmdObj.transform.eulerAngles.y - hmdObj.transform.eulerAngles.z, 0.0f); // Camera Rotation
        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z); // Ground postion relative to camera

        Vector3 translateVector = groundPosition - carpet;
        translateVector.y = 0f;

        teleportIndicator.transform.position = carpetChild + translateVector;

        teleportIndicator.transform.localRotation = Quaternion.Euler(camRot);

        teleportIndicator.transform.RotateAround(carpetChild, Vector3.up, -IndicatorRotation);
        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(true);
    }

    [PunRPC]
    void RemoteTeleIndicatorDeAcitve(bool check)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = check;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(check);
    }


    [PunRPC]
    void RemoteTeleportation(Vector3 carpetChild, Vector3 carpet)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);

        Vector3 translateVector = teleportIndicator.transform.position - groundPosition;
        translateVector.y = 0f;

        platformObj.transform.position += translateVector;

        groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);

        float difference = AngleDifference(platformObj.transform.eulerAngles.y, teleportIndicator.transform.eulerAngles.y);

        platformObj.transform.RotateAround(hmdObj.transform.position, Vector3.up, (difference - hmdObj.transform.localEulerAngles.y));
    }

    [PunRPC]
    void RemoteCircleForm(Vector3 newPos, Vector3 carpetPos)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
        teleportIndicator.transform.position = newPos;

        teleportIndicator.transform.LookAt(carpetPos);

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(true);
    }

    [PunRPC]
    void RemoteSemiCircleForm(Vector3 newPos, Vector3 direction)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
        teleportIndicator.transform.position = newPos;
        direction.y = 0;
        teleportIndicator.transform.forward = direction;
        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(true);
    }

    [PunRPC]
    void RemotePresenterForm(Vector3 newPos, Vector3 lookPos)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
        teleportIndicator.transform.position = newPos;
        teleportIndicator.transform.LookAt(lookPos);

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(true);
    }

    /*
    [PunRPC]
    void SwitchingTechnique(bool check)
    {
        //GameObject Hand = GameObject.Find("/ViewingSetup/Platform/ControllerRight/ComicHandRight(Clone)");
        GetComponent<CarpetNav>().navigatorMode = check;
        Debug.Log("RPC Recieved to " + GetComponent<PhotonView>().OwnerActorNr);
    }

    [PunRPC]
    void RemoteScaling(bool check, Vector3 carpetPos) //TODELELTE
    {
        ViewPort = GameObject.Find("/ViewingSetup");
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        if (check)
        {
            platformObj.transform.SetParent(null);
            ViewPort.transform.position = new Vector3(carpetPos.x, ViewPort.transform.position.y, carpetPos.z);
            platformObj.transform.SetParent(ViewPort.transform, true);
            ViewPort.transform.localScale = smallScale;
        }
        else
        {
            platformObj.transform.SetParent(null);
            ViewPort.transform.position = new Vector3(carpetPos.x, ViewPort.transform.position.y, carpetPos.z);
            platformObj.transform.SetParent(ViewPort.transform, true);
            ViewPort.transform.localScale = normalScale;
        }
    }

     public void ScalingChange(bool scaleCheck) //todelete
    {
        Debug.Log("Scaled");
        if (scaleCheck)
        {
            platformObj.transform.SetParent(null);
            carpetObj.transform.SetParent(null);
            carpetObj.transform.localScale = new Vector3(carpetObj.transform.localScale.x / 1.8f, carpetObj.transform.localScale.y, carpetObj.transform.localScale.z / 1.8f);
            ViewPort.transform.position = new Vector3(carpetObj.transform.position.x, ViewPort.transform.position.y, carpetObj.transform.position.z);
            platformObj.transform.SetParent(ViewPort.transform, true);
            ViewPort.transform.localScale = smallScale;
            carpetObj.transform.SetParent(ViewPort.transform, true);
        }
        else
        {
            platformObj.transform.SetParent(null);
            carpetObj.transform.SetParent(null);
            ViewPort.transform.position = new Vector3(carpetObj.transform.position.x, ViewPort.transform.position.y, carpetObj.transform.position.z);
            carpetObj.transform.localScale = new Vector3(carpetObj.transform.localScale.x * 1.8f, carpetObj.transform.localScale.y, carpetObj.transform.localScale.z * 1.8f);
            platformObj.transform.SetParent(ViewPort.transform, true);
            ViewPort.transform.localScale = normalScale;
            carpetObj.transform.SetParent(ViewPort.transform, true);
        }
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            if (passengerIDs[i] != myID)
            {
                photonView.RPC("RemoteScaling", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), scaleCheck, carpetObj.transform.position);
            }
        }

    }*/

}
