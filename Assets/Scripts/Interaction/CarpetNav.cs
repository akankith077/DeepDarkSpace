using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Valve.VR;

public class CarpetNav : MonoBehaviourPunCallbacks
{

    public SteamVR_Input_Sources handType;

    public SteamVR_Action_Boolean groupTeleportationActive;
    public SteamVR_Action_Boolean groupTeleportationConfirm;

    public GameObject platformObj;
    public GameObject hmdObj;
    public GameObject carpetObj;
    public GameObject teleportIndicator;

    public List<int> passengers = new List<int>();
    private int[] passengerIDs = { };
    private float myID = 0;

    private bool teleButtonCheck = false;

    void Start()
    {
        if (photonView.IsMine)
        {
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
            teleportIndicator = GameObject.Find("/ViewingSetup/Platform/TELE");
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
        }
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            ButtonCheck();

            if (teleButtonCheck && carpetObj != null)
            {
                GroupTeleActive();
            }
            if (groupTeleportationActive.GetStateUp(handType) && carpetObj != null)
            {
                GroupTeleDeactivate();
            }

            if (groupTeleportationConfirm.GetStateDown(handType) && carpetObj != null)
            {
                GroupTeleportation();
            }
            if (carpetObj != null)
            {
                this.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
                passengers = carpetObj.GetComponent<pplOnCar>().carpetList;
                passengerIDs = passengers.ToArray();


                Debug.Log("Passengers on the carpet are: ");
                for (int i = 0; i < passengerIDs.Length; i++)
                {
                    Debug.Log(passengerIDs[i]);
                }
            }
            if (carpetObj == null)
            {
                this.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        if (collision.gameObject.name == "carpet(Clone)")
        {
            carpetObj = collision.gameObject;
        }
    }
    private void OnCollisionExit(Collision collision) //*************** When user enters the carpet
    {
        if (collision.gameObject.name == "carpet(Clone)")
        {
            carpetObj = null;
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
    }

    [PunRPC]
    void RemoteTeleIndicatorAcitve(Vector3 carpetChild, Vector3 carpet)  //*************** RPC for teleportation
    {
        platformObj = GameObject.Find("/ViewingSetup/Platform");
        hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        teleportIndicator = GameObject.Find("/ViewingSetup/Platform/TELE");

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
        teleportIndicator = GameObject.Find("/ViewingSetup/Platform/TELE");

        Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);

        Vector3 translateVector = teleportIndicator.transform.position - groundPosition;

        platformObj.transform.position += translateVector;
    }

        [PunRPC]
    void RemoteTeleIndicatorDeAcitve(bool check)  //*************** RPC for teleportation
    {
        teleportIndicator = GameObject.Find("/ViewingSetup/Platform/TELE");

        teleportIndicator.transform.GetComponent<MeshRenderer>().enabled = check;
        teleportIndicator.GetComponent<CurvedRay>().GetDrawLine(check);
    }
}
