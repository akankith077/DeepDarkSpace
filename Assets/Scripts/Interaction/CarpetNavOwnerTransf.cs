using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Valve.VR;

public class CarpetNavOwnerTransf : MonoBehaviourPunCallbacks
{

    public SteamVR_Input_Sources handType;

    public SteamVR_Action_Boolean groupTeleportationActive;
    public SteamVR_Action_Boolean groupTeleportationConfirm;
    public SteamVR_Action_Boolean backToCar;
    public SteamVR_Action_Boolean groupCancel;

    public GameObject platformObj;
    public GameObject hmdObj;
    public GameObject carpetObj;
    public GameObject oldCarpet;
    public GameObject teleportIndicator;
    private GameObject leftHand;
    public Vector3 nextPos;

    public List<int> passengers = new List<int>();
    public List<Vector3> carpetPosList = new List<Vector3>();
    private int[] passengerIDs = { };
    public int index = 0;
    private float myID = 0;
    public float minHandHeight = 0f;

    private bool teleButtonCheck = false;
    public bool onCarpet = false;
    public bool grouped = false;
    public bool backToCarCheck = false;
    public bool carpIsMine = false;
    private GameObject otherHand;

    void Start()
    {
        if (photonView.IsMine)
        {
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
            teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
            leftHand = GameObject.Find("/ViewingSetup/Platform/ControllerLeft/ComicHandLeft(Clone)");
        }
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            ButtonCheck();

            if (teleButtonCheck && carpetObj != null && carpIsMine ==true)
            {
                GroupTeleActive();
            }
            if (groupTeleportationActive.GetStateUp(handType) && carpetObj != null && carpIsMine == true)
            {
                GroupTeleDeactivate();
            }

            if (groupTeleportationConfirm.GetStateDown(handType) && carpetObj != null && carpIsMine == true)
            {
                GroupTeleportation();
            }
            if (carpetObj != null)
            {
                this.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
                leftHand.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
                passengers = carpetObj.GetComponent<pplOnCar>().carpetList;
                passengerIDs = passengers.ToArray();


                /*Debug.Log("Passengers on the carpet are: ");
                for (int i = 0; i < passengerIDs.Length; i++)
                {
                    Debug.Log(passengerIDs[i]);
                }*/
            }
            else
            {
                this.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;

                leftHand.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
            }
            if (onCarpet == false && grouped == true && backToCarCheck == true)
            {
                /*for (float i = 0; i <= 1; i++) //ANIMATION FOR GROUPED BUT OUTSIDE CARPET
                { 
                    GameObject wristBand = this.gameObject.transform.GetChild(2).gameObject;
                    Vector3 anim = (wristBand.transform.localScale.x, i, 0.3f);
                    wristBand.transform.localScale.y += 1;
                }*/

                carpetPosList = oldCarpet.transform.GetComponent<pplOnCar>().carpetPosList;

                nextPos = carpetPosList[index];
                teleportIndicator.transform.position = nextPos;
                teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
                teleportIndicator.transform.GetComponent<CurvedRay>().GetDrawLine(true);

            }
            else if (onCarpet == false && grouped == true && backToCar.GetStateUp(SteamVR_Input_Sources.LeftHand))
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

            if (carpetObj != null && carpetObj.GetComponent<PhotonView>().IsMine)
            {
                carpIsMine = true; 
                
                Debug.Log("This is my carpet  " + carpIsMine);
            }
            else
            {
                carpIsMine = false;
                
                Debug.Log("This is my carpet" + carpIsMine);
            }
        }
    }

    private void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        if (collision.gameObject.name == "carpet(Clone)")
        {
            carpetObj = collision.gameObject;
            
            oldCarpet = carpetObj;
            grouped = true;
            onCarpet = true;
        }
       
    }
    private void OnCollisionExit(Collision collision) //*************** When user enters the carpet
    {   
        Debug.Log("The Colllided Object Name is " + collision.gameObject.name);
        if (collision.gameObject.name == "carpet(Clone)")
        {
            index = oldCarpet.transform.GetComponent<pplOnCar>().carpetPosList.Count - 1;
            carpetObj = null;
            onCarpet = false;
        }

         if(collision.gameObject.name == "ComicHandRight(Clone)")
        {   
            otherHand = collision.gameObject;
            if (this.transform.parent.transform.position.y > minHandHeight)
            {
                TransferOwner();
            }
        }
    }


    public void TransferOwner()
    {
        if (carpetObj != null)
        {
            if (!carpIsMine)
            {   
                carpetObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                carpetObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                carpetObj.transform.GetChild(1).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                string shirtColor = PlayerPrefs.GetString(GlobalSettings.colorPrefKey);
                carpetObj.GetComponent<pplOnCar>().ChangeCarColour(shirtColor);

                //carpIsMine = true;
            }
            else
            {   
                Debug.Log("OWNERSHIP  TRANSFERED");
                //carpIsMine = false;
            }
        }
    }

    public void GroupTeleActive()
    {
        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        Vector3 translateVector = groundPosition - carpetObj.transform.position;
        carpetObj.transform.GetChild(0).position = teleportIndicator.transform.position - translateVector;
        carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        //carpetObj.gameObject.transform.SetParent(platformObj.transform); 
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            if (passengerIDs[i] != myID)
            {
                photonView.RPC("RemoteTeleIndicatorAcitve", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), carpetObj.transform.GetChild(0).position, carpetObj.transform.position);
            }
        }
    }



    public void GroupTeleDeactivate()
    {
        carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        for (int i = 0; i < passengerIDs.Length; i++)
        {
            if (passengerIDs[i] != myID)
            {
                photonView.RPC("RemoteTeleIndicatorDeAcitve", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), false);
            }
        }
    }

    public void GroupTeleportation()
    {
        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        Vector3 translateVector = teleportIndicator.transform.position - groundPosition;
        carpetObj.transform.position += translateVector;

        for (int i = 0; i < passengerIDs.Length; i++)
        {
            if (passengerIDs[i] != myID)
            {
                photonView.RPC("RemoteTeleportation", PhotonNetwork.CurrentRoom.GetPlayer(passengerIDs[i]), carpetObj.transform.GetChild(0).position, carpetObj.transform.position);
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
    }

    [PunRPC]
    void RemoteTeleIndicatorAcitve(Vector3 carpetChild, Vector3 carpet)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
        Vector3 translateVector = groundPosition - carpet;
        teleportIndicator.transform.position = carpetChild + translateVector;

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = true;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(true);
    }


    [PunRPC]
    void RemoteTeleportation(Vector3 carpetChild, Vector3 carpet)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);

        Vector3 translateVector = teleportIndicator.transform.position - groundPosition;

        platformObj.transform.position += translateVector;
    }

    [PunRPC]
    void RemoteTeleIndicatorDeAcitve(bool check)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/TELE");

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = check;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(check);
    }

}
