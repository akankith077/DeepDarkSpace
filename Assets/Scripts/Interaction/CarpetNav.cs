using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Valve.VR;

public class CarpetNav : MonoBehaviourPunCallbacks
{

    public SteamVR_Input_Sources handType;
    public BezierCurve bezier;

    public SteamVR_Action_Boolean groupTeleportationActive;
    public SteamVR_Action_Boolean groupTeleportationConfirm;
    public SteamVR_Action_Boolean backToCar;
    public SteamVR_Action_Boolean groupCancel;
    public SteamVR_Action_Boolean navigatorToggle;
    public SteamVR_Action_Boolean circleFormAction;
    public SteamVR_Action_Boolean presenterFormAction;
    public SteamVR_Action_Boolean semiCircFormAction;
    public SteamVR_Action_Boolean scaling;

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

    public Vector3 nextPos;
    private Vector3 edgeCorrection = new Vector3(1f, 0f, 1f);
    public List<int> passengers = new List<int>();
    private List<Vector3> carpetPosList = new List<Vector3>();
    private int[] passengerIDs = { };
    private int index = 0;
    private float myID = 0;
    private float otherID = 0;
    private float ControllerRotation = 0;
    public float highFiveHeight = 1.1f;
    private float myRotAngles = 0.0f;
    private float cntrlrRotScale = 1.8f;

    private GameObject ViewPort;
    private Vector3 smallScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 normalScale = new Vector3(1f, 1f, 1f);


    public bool onCarpet = false;
    public bool grouped = false;
    public bool navigatorMode = false;
    public bool carpIsMine = false;

    private bool backToCarCheck = false;
    private bool teleButtonCheck = false;
    public bool cicrleForm = false;
    public bool semiCircForm = false;
    public bool presenterForm = false;
    public bool joystickButtonActive = false;

    public Material gold;
    public Material old;

    void Start()
    {
        if (photonView.IsMine)
        {
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            ViewPort = GameObject.Find("/ViewingSetup");
            hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
            helmetObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera/Robot/CameraTranslateOnly/RobotHead/Helmet");
            teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
            leftHand = GameObject.Find("/ViewingSetup/Platform/ControllerLeft/ComicHandLeft(Clone)");
            leftWristBand = this.transform.GetChild(2).gameObject;
            rightWristBand = leftHand.transform.GetChild(2).gameObject;
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

            if (carpetObj != null) //To check the owner of the carpet
            {
                carpetOwnershipCheck();
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
                rightWristBand.GetComponent<MeshRenderer>().enabled = true;
                leftWristBand.GetComponent<MeshRenderer>().enabled = true;
                passengers = carpetObj.GetComponent<pplOnCar>().carpetList;
                passengerIDs = passengers.ToArray();
            }

            if (!grouped)
            {
                rightWristBand.GetComponent<MeshRenderer>().enabled = false;
                leftWristBand.GetComponent<MeshRenderer>().enabled = false;
                carpetObj = null;
                oldCarpet = null;
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

                if (index < carpetPosList.Count)
                {
                    index++;
                }
            }
        }
    }

    public void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
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
        }
    }
    public void OnCollisionExit(Collision collision) //*************** When user enters the carpet
    {
        if (collision.gameObject.name == "carpet(Clone)")
        {
            if (grouped)
            {
                index = oldCarpet.transform.GetComponent<pplOnCar>().carpetPosList.Count - 1;
            }
            carpetObj = null;
            Debug.Log("Carpet object Null");
            onCarpet = false;
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

    public void ControllerRot()
    {
        ControllerRotation = (this.transform.parent.transform.localEulerAngles.z * cntrlrRotScale);
        ControllerRotation = ControllerRotation - 180;
        //float step = 10;
        /*if (ControllerRotation > 180)
        {
            ControllerRotation = ControllerRotation - 360;
        }
        for (var i = 0f; i < (360 / step); i++)
        {
            if (ControllerRotation > i * step && ControllerRotation <= (i + 1) * step)
            {
                ControllerRotation = (i * step) + 1;
            }
            if (ControllerRotation < i * (-step) && ControllerRotation >= (i + 1) * (-step))
            {
                ControllerRotation = -(i * step) + 1;
            }
        }
        */
    }
    public void GroupTeleActive()
    {
        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        Vector3 translateVector = groundPosition - carpetObj.transform.position;
        translateVector.y = 0f;
        carpetObj.transform.GetChild(0).position = bezier.EndPoint - translateVector;
        carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        Vector3 IndicatorRotation = new Vector3(0, this.transform.parent.transform.localEulerAngles.y - this.transform.parent.localEulerAngles.z, 0);
        teleportIndicator.transform.rotation = Quaternion.Euler(IndicatorRotation);
        for (int i = 0; i < passengerIDs.Length; i++)
        {
                photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]),
                carpetObj.transform.GetChild(0).position,
                carpetObj.transform.position,
                ControllerRotation,
                IndicatorRotation);
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
        if (joystickButtonActive)
        {
            /*Vector3 translateVec = teleportIndicator.transform.position - groundPosition;
            hmdObj.transform.SetParent(null);
            platformObj.transform.position = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, platformObj.transform.position.z);
            hmdObj.transform.SetParent(platformObj.transform);
            platformObj.transform.localRotation = teleportIndicator.transform.localRotation;
            Quaternion newRotation = hmdObj.transform.localRotation;
            newRotation.z = 0;
            newRotation.x = 0;
            platformObj.transform.localRotation = platformObj.transform.localRotation * Quaternion.Inverse(newRotation);*/
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

    public void carpetOwnershipCheck()
    {
        if (carpetObj != null && carpetObj.GetComponent<PhotonView>().IsMine)
        {
            carpIsMine = true;
            if (navigatorMode)
            {
                helmetObj.GetComponent<MeshRenderer>().enabled = true;
                rightWristBand.GetComponent<Renderer>().material = gold;
                leftWristBand.GetComponent<Renderer>().material = gold;
            }
            else
            {
                helmetObj.GetComponent<MeshRenderer>().enabled = false;
                rightWristBand.GetComponent<Renderer>().material = old;
                leftWristBand.GetComponent<Renderer>().material = old;
            }
            //Debug.Log("This is my carpet  " + carpIsMine);
        }
        else
        {
            carpIsMine = false;
            if (navigatorMode)
            {
                helmetObj.GetComponent<MeshRenderer>().enabled = false;
            }
            //Debug.Log("This is my carpet" + carpIsMine);
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
        }
        else if (circleFormAction.GetStateUp(handType))
        {
            cicrleForm = false;
            teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(false);
            if (carpetObj != null)
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        if (semiCircFormAction.GetStateDown(handType))
        {
            semiCircForm = true;
        }
        else if (semiCircFormAction.GetStateUp(handType))
        {
            semiCircForm = false;
            teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(false);
            if (carpetObj != null)
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
        }

        if (presenterFormAction.GetStateDown(handType))
        {
            presenterForm = true;
        }
        else if (presenterFormAction.GetStateUp(handType))
        {
            presenterForm = false;
            teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(false);
            if (carpetObj != null)
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = false;
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

    public void CircleFormation()
    {
        float angleSection = Mathf.PI * 2f / passengerIDs.Length;
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            float angle = i * angleSection;
            Vector3 newPos = carpetObj.transform.GetChild(0).position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (carpetObj.transform.localScale.x - 0.2f);
            carpetObj.transform.GetChild(0).position = bezier.EndPoint;
            carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            photonView.RPC("RemoteCircleForm", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), newPos, carpetObj.transform.GetChild(0).position, ControllerRotation + myRotAngles);
        }
    }

    public void SemiCircleFormation()
    {
        float angleSection = Mathf.PI * 4f / passengerIDs.Length;
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            float angle = (i * angleSection);// + ControllerRotation;
            Vector3 newPos = carpetObj.transform.GetChild(0).position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * carpetObj.transform.localScale.x;

            //photonView.RPC("RemoteCircleForm", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), newPos, carpetObj.transform.GetChild(0).position);
        }
    }

    [PunRPC]
    void RemoteTeleIndicatorAcitve(Vector3 carpetChild, Vector3 carpet, float controllerRot, Vector3 IndicatorRotation)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        Vector3 translateVector = groundPosition - carpet;
        translateVector.y = 0f;
        teleportIndicator.transform.position = carpetChild + translateVector;
        //Vector3 IndicatorRotation = new Vector3(0, .transform.eulerAngles.y - hmdObj.transform.eulerAngles.z, 0); //Adding controller Z rotation to teleport indicator  
        teleportIndicator.transform.RotateAround(carpetChild, Vector3.up, controllerRot);
        teleportIndicator.transform.rotation = Quaternion.Euler(IndicatorRotation);
        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(true);
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

        hmdObj.transform.SetParent(null);
        platformObj.transform.position = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, platformObj.transform.position.z);
        hmdObj.transform.SetParent(platformObj.transform);
        platformObj.transform.localRotation = teleportIndicator.transform.localRotation;
        Quaternion newRotation = hmdObj.transform.localRotation;
        newRotation.z = 0;
        newRotation.x = 0;

        platformObj.transform.RotateAround(hmdObj.transform.position, Vector3.up, -hmdObj.transform.localEulerAngles.y);
        //platformObj.transform.localRotation = platformObj.transform.localRotation * Quaternion.Inverse(newRotation);
    }

    [PunRPC]
    void RemoteTeleIndicatorDeAcitve(bool check)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = check;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(check);
    }

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

    [PunRPC]
    void RemoteCircleForm(Vector3 newPos, Vector3 carpetPos, float rot)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
        teleportIndicator.transform.position = newPos;
        teleportIndicator.transform.RotateAround(carpetPos, Vector3.up, rot);
        teleportIndicator.transform.LookAt(carpetPos);

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawRightLine(true);
    }

}
