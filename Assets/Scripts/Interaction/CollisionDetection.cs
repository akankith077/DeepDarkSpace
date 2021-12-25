using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;
public class CollisionDetection : MonoBehaviourPunCallbacks
{
    private GameObject other;
    private GameObject platformObj;
    private GameObject hmdCam;
    private GameObject groupTeleIndicator;
    private GameObject teleIndicator;
    private GameObject otherHandObj;

    public Material invisibleHand;
    public Material invisibleHand1;
    public Material orginalHand;

    private LineRenderer connectLine;
    private LineRenderer groupLine;

    public Animator anim;

    public SteamVR_Action_Boolean handHoldAction;
    public SteamVR_Input_Sources handType;
    public SteamVR_Input_Sources handType1;
    public SteamVR_Action_Boolean teleportConfirmAction;
    public SteamVR_Action_Vibration controllerVibration;
    //public SteamVR_Action_Single teleportAction;
    public SteamVR_Action_Vector2 joystickPrimary;


    private Vector3 offSet;
    private Vector3 relPos;
    private Vector3 n_connectLineStartPos;
    private Vector3 n_connectLineEndPos;
    Vector3 myTransform;
    Vector3 myCamTransform;
    Vector3 myOldTransform;
    Vector3 otherPos;
    private Quaternion camRotation;

    private int myID;
    private int otherID;
    private int otherViewID;
    private float teleRot;
    private float inputValue;
    private bool otherFound = false;
    private bool onceCheck = false;
    private bool handHold = false;
    private bool check = false;
    private bool buttonCheck = false;
    private bool n_connectLine = false;
    private float handheldTime = 0.0f;

    void Start()
    {
        if (photonView.IsMine)
        {
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
            connectLine = GetComponent<LineRenderer>();
            connectLine.enabled = false;
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            hmdCam = GameObject.Find("/ViewingSetup/Platform/HMDCamera");

            string avatarType = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);
            teleIndicator = GameObject.Find("/ViewingSetup/Platform/TELE"); //Finding teleportation sprite
            teleIndicator.transform.SetParent(platformObj.transform.parent.transform);

            groupTeleIndicator = PhotonNetwork.Instantiate("Interaction/GroupTeleIndicator", Vector3.zero, Quaternion.identity);
            groupTeleIndicator.GetComponentInChildren<MeshRenderer>().enabled = false;
            groupTeleIndicator.name = "GroupTELE";

            groupTeleIndicator.transform.SetParent(platformObj.transform.parent.transform);

            myTransform = platformObj.transform.position;
            myOldTransform = myTransform;

            anim = GetComponent<Animator>();
        }
        else
        {
            connectLine = GetComponent<LineRenderer>();
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandHolding(); //To check if gripper button is active
            TeleButtonCheck();

            myTransform = platformObj.transform.position;
            myCamTransform = hmdCam.transform.position;
            camRotation = hmdCam.transform.rotation;


            //string avatarType = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);

            teleIndicator = GameObject.Find("/ViewingSetup/TELE"); //Finding teleportation sprite

            groupTeleIndicator = GameObject.Find("/ViewingSetup/GroupTELE");

            teleRot = teleIndicator.transform.eulerAngles.y;


            if (handHold) //***************If pressing gripper button
            {
                if (otherFound && otherID != myID) //***************If collision with another user
                {
                    anim.SetBool("HandHold", true);
                    transform.GetChild(2).gameObject.GetComponentInChildren<MeshRenderer>().enabled = true; //***************Sets Transparent material to Holding hand when grouped

                    //Debug.Log("otherID: " + otherID);
                    //Debug.Log("myID: " + PhotonNetwork.CurrentRoom.GetPlayer(otherID).CustomProperties["HandPressed"]);

                    transform.GetChild(2).gameObject.GetComponentInChildren<Renderer>().material = invisibleHand1;

                    if ((int)PhotonNetwork.CurrentRoom.GetPlayer(otherID).CustomProperties["HandPressed"] == myID)
                    {
                        transform.GetChild(2).gameObject.GetComponentInChildren<Renderer>().material = invisibleHand;

                        //Timer code start

                        handheldTime += Time.deltaTime;
                        Vector3 telepos = groupTeleIndicator.transform.position;

                        //Timer code end
                        //inputValue = teleportAction.GetAxis(handType1);

                        if (inputValue > 0.3f)
                        {
                            teleGroupIndicatorSet();
                        }
                        if (inputValue > 0.1f && inputValue <= 0.3f)
                        {
                            photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(otherID), false);
                        }

                        if (otherViewID != 0)
                        {
                            connectLine.enabled = true;
                            otherHandObj = PhotonView.Find(otherViewID).gameObject;
                            connectLine.SetPosition(0, transform.GetChild(2).transform.position);
                            connectLine.SetPosition(1, otherHandObj.transform.position);

                        }
                    }
                    else
                    {
                        transform.GetChild(2).gameObject.GetComponentInChildren<MeshRenderer>().enabled = false; //*************** Sets hand to original colour when ungrouped
                        //photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(otherID), false); //MIGHT CAUSE ERROR
                        transform.GetChild(2).gameObject.GetComponentInChildren<Renderer>().material = invisibleHand1;
                        connectLine.enabled = false;
                    }
                }
                else
                {
                    anim.SetBool("Check", true);

                    //var myHashtable = new ExitGames.Client.Photon.Hashtable();
                    //myHashtable.Add("HandPressed", -1);
                    //PhotonNetwork.LocalPlayer.SetCustomProperties(myHashtable);

                    transform.GetChild(2).gameObject.GetComponentInChildren<MeshRenderer>().enabled = false; //*************** Sets hand to original colour when ungrouped

                }
            }
            else
            {
                anim.SetBool("HandHold", false);
                anim.SetBool("Check", false);
                connectLine.enabled = false;
                transform.GetChild(2).gameObject.GetComponentInChildren<MeshRenderer>().enabled = false; //*************** Sets hand to original colour when ungrouped
                //groupTeleIndicator.GetComponentInChildren<MeshRenderer>().enabled = false
                //photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(otherID), teleIndicator.activeSelf);
                /*
                var myHashtable = new ExitGames.Client.Photon.Hashtable();
                myHashtable.Add("HandPressed", -1);
                PhotonNetwork.LocalPlayer.SetCustomProperties(myHashtable);*/
            }
        }
    }

    void TeleButtonCheck()
    {
        //float inputValue = teleportAction.GetAxis(handType1);

        if (inputValue > 0.0f)
        {
            buttonCheck = true;
            //if (otherFound) { photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(otherID), true); }
        }
        if (inputValue <= 0.0f)
        {
            buttonCheck = false;

        }
    }


    private void OnCollisionStay(Collision collision) //*************** When collided with other user
    {
        myID = transform.GetComponent<PhotonView>().OwnerActorNr;
        int checkID = collision.gameObject.GetComponent<PhotonView>().OwnerActorNr;//*************** Checks Actor number

        if (checkID != myID)
        {
            other = collision.gameObject;
            int viewID = other.GetComponent<PhotonView>().ViewID;
            otherID = checkID;
            otherViewID = viewID;
            otherFound = true;
            otherPos = other.GetComponent<PhotonTransformView>().transform.position;
            offSet = otherPos - myCamTransform; //*************** Calculates offset from user and group member
        }
        else
        {
            //Comes here if collided hand is my other hand.
        }
    }

    void HandHolding() //*************** To check if gripper button is pressed down
    {
        if (handHoldAction.GetStateDown(handType))
        {
            controllerVibration.Execute(0, 0.05f, 0.002f, 0.8f, handType);
            handHold = true;
            var myHashtable = new ExitGames.Client.Photon.Hashtable();
            myHashtable.Add("HandPressed", otherID);
            PhotonNetwork.LocalPlayer.SetCustomProperties(myHashtable);
        }
        if (handHoldAction.GetStateUp(handType))
        {
            if (otherFound)
            {
                Debug.Log("************** Hand held for: " + handheldTime + " seconds");
                photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(otherID), false);
            }
            controllerVibration.Execute(0, 0.05f, 0.002f, 0.8f, handType);
            handHold = false;
            otherFound = false;
            otherID = 99;
            handheldTime = 0.0f;
            var myHashtable = new ExitGames.Client.Photon.Hashtable();
            myHashtable.Add("HandPressed", otherID);
            PhotonNetwork.LocalPlayer.SetCustomProperties(myHashtable);
        }
    }

    void teleGroupIndicatorSet() //*************** Sets group member teleportation indicator
    {
        if (teleportConfirmAction.GetLastStateDown(handType1))
        {
            photonView.RPC("RemoteTeleportTranslate", PhotonNetwork.CurrentRoom.GetPlayer(otherID),
                groupTeleIndicator.transform.position,
                camRotation,
                transform.name,
                platformObj.transform.rotation
            );
        }
        float step = 1;
        float localRot = teleRot;
        for (var i = 0f; i < (360 / step); i++)
        {
            if (localRot > i * step && localRot <= (i + 1) * step)
            {
                localRot = (i * step) + 1;
            }
        }

        groupTeleIndicator.transform.position = teleIndicator.transform.position;
        groupTeleIndicator.transform.rotation = Quaternion.Slerp(Quaternion.AngleAxis(groupTeleIndicator.transform.rotation.y, Vector3.up) //sets the rotation of Other's Camera
                                    , Quaternion.AngleAxis(localRot, Vector3.up)
                                    , 1f);

        if (this.transform.name == "ComicHandRight(Clone)")
        {
            groupTeleIndicator.transform.position = groupTeleIndicator.transform.TransformPoint(Vector3.right); //Set's user to right of Other.
        }
        if (this.transform.name == "ComicHandLeft(Clone)")
        {
            groupTeleIndicator.transform.position = groupTeleIndicator.transform.TransformPoint(Vector3.left); //Set's user to right of Other.
        }
        //groupTeleIndicator.GetComponentInChildren<MeshRenderer>().enabled = true;
        photonView.RPC("RemoteTeleIndicatorUpdate", PhotonNetwork.CurrentRoom.GetPlayer(otherID),
                                    groupTeleIndicator.transform.position, true, teleIndicator.transform.rotation
                                );


    }


    [PunRPC]
    void RemoteTeleportTranslate(Vector3 otherTransform, Quaternion otherCamRotate, string handLoR, Quaternion otherPlatRot)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdCam = GameObject.Find("/ViewingSetup/Platform/HMDCamera");

        relPos = new Vector3(hmdCam.transform.localPosition.x, 0f, hmdCam.transform.localPosition.z);

        float angle = otherCamRotate.eulerAngles.y;
        float step = 1;

        for (var i = 0f; i < (360 / step); i++)
        {
            if (angle > i * step && angle <= (i + 1) * step)
            {
                angle = (i * step) + 1;
            }
        }
        Quaternion PlatQuat = platformObj.transform.rotation;

        Vector3 anglePlat = platformObj.transform.rotation.eulerAngles;
        anglePlat.y += angle - hmdCam.transform.rotation.eulerAngles.y;

        PlatQuat.eulerAngles = anglePlat;

        platformObj.transform.rotation = PlatQuat;

            /*Quaternion.Slerp(Quaternion.AngleAxis(hmdCam.transform.rotation.eulerAngles.y , Vector3.up) //sets the rotation of Other's Camera
                                        , Quaternion.AngleAxis(angle, Vector3.up)
                                        , 1.0f);*/

        Vector3 groundPosition = new Vector3(hmdCam.transform.position.x, platformObj.transform.position.y, hmdCam.transform.position.z);

        Vector3 translateVector = otherTransform - groundPosition;

        platformObj.transform.position += translateVector;

        //platformObj.transform.RotateAround(hmdCam.transform.position, Vector3.up, -angle);

    }

    [PunRPC]
    void RemoteTeleIndicatorUpdate(Vector3 otherTeleTransform, bool check, Quaternion otherTeleRot)
    {
        if (check)
        {
            string avatarType = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);
            teleIndicator = GameObject.Find("/ViewingSetup/TELE");
            Vector3 newRot = new Vector3(teleIndicator.transform.eulerAngles.x, otherTeleRot.eulerAngles.y, teleIndicator.transform.eulerAngles.z);
            teleIndicator.transform.rotation = Quaternion.Euler(newRot);
            teleIndicator.transform.position = otherTeleTransform;
            teleIndicator.GetComponent<MeshRenderer>().enabled = check;
            teleIndicator.GetComponent<CurvedRay>().GetDrawLine(check);
            //groupLine.enabled = check;

            /*if (groupLine.enabled)
            {
                Debug.Log("Found Linerenderer");
                GameObject leftHand = GameObject.Find("/ViewingSetup/Platform/ControllerLeft");

                groupLine.SetPosition(0, teleIndicator.transform.position);
                groupLine.SetPosition(1, leftHand.transform.position);

                Vector3 midPos = Vector3.Lerp(teleIndicator.transform.position, leftHand.transform.position, 0.5f);
                midPos.y = leftHand.transform.position.y + 1.0f;
                var pointList = new List<Vector3>();

                for (float ratio = 0; ratio <= 1; ratio += 10)
                {
                    var tangent1 = Vector3.Lerp(teleIndicator.transform.position, midPos, ratio);
                    var tangent2 = Vector3.Lerp(midPos, leftHand.transform.position, ratio);
                    var curve = Vector3.Lerp(tangent1, tangent2, ratio);

                    pointList.Add(curve);
                }

                //line.positionCount = pointList.Count;
                //line.SetPositions(pointList.ToArray());
                //line.enabled = check;
            }*/
        }
    }

    [PunRPC]
    void RemoteTeleIndicatorAcitve(bool checkActive1)
    {
        string avatarType = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);
        teleIndicator = GameObject.Find("/ViewingSetup/TELE");
        teleIndicator.GetComponent<MeshRenderer>().enabled = checkActive1;
        teleIndicator.GetComponent<CurvedRay>().GetDrawLine(checkActive1);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //*************** For activation and deactivation of Group teleindicator
    {
        if (stream.IsWriting)
        {
            //bool teleIndiCheck = groupTeleIndicator.GetComponentInChildren<MeshRenderer>().enabled;
            //stream.SendNext(teleIndiCheck);
            //bool connectLineCheck = connectLine.enabled;
            //stream.SendNext(connectLineCheck);
            //stream.SendNext(transform.GetChild(2).transform.position);
            //stream.SendNext(otherHandObj.transform.position);

        }
        else
        {
            //bool teleIndiCheck = (bool)stream.ReceiveNext();
            //groupTeleIndicator.GetComponentInChildren<MeshRenderer>().enabled = teleIndiCheck;
            //connectLine.enabled = (bool)stream.ReceiveNext();
            //n_connectLineStartPos = (Vector3)stream.ReceiveNext();
            //n_connectLineEndPos = (Vector3)stream.ReceiveNext();
            //connectLine.SetPosition(0, n_connectLineStartPos);
            //connectLine.SetPosition(0, n_connectLineEndPos);
        }
    }
}
