﻿using System.Collections;
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
    public SteamVR_Action_Boolean backToCar;
    public SteamVR_Action_Boolean groupCancel;

    public GameObject platformObj;
    public GameObject hmdObj;
    public GameObject carpetObj;
    public GameObject oldCarpet;
    public GameObject teleportIndicator;
    private GameObject leftHand;
    private GameObject leftWristBand;
    private GameObject rightWristBand;
    public Vector3 nextPos;

    public List<int> passengers = new List<int>();
    public List<Vector3> carpetPosList = new List<Vector3>();
    private int[] passengerIDs = { };
    public int index = 0;
    private float myID = 0;
    private float otherID = 0;
    public float highFiveHeight = 1.2f;

    private bool teleButtonCheck = false;
    public bool onCarpet = false;
    public bool grouped = false;
    public bool backToCarCheck = false;
    public bool navigatorMode = false;
    public bool carpIsMine = false;

    void Start()
    {
        if (photonView.IsMine)
        {
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
            teleportIndicator = GameObject.Find("/ViewingSetup/TELE");
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
            leftHand = GameObject.Find("/ViewingSetup/Platform/ControllerLeft/ComicHandLeft(Clone)");
            leftWristBand = this.transform.GetChild(2).gameObject;
            rightWristBand = leftHand.transform.GetChild(2).gameObject;
        }
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            ButtonCheck();

            if (carpetObj != null)
            {
                carpetOwnershipCheck();
            }

            if (teleButtonCheck && carpetObj != null)
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
            if (groupTeleportationActive.GetStateUp(handType) && carpetObj != null)
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

            if (groupTeleportationConfirm.GetStateDown(handType) && carpetObj != null)
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
            if (carpetObj != null)
            {
                rightWristBand.GetComponent<MeshRenderer>().enabled = true;
                leftWristBand.GetComponent<MeshRenderer>().enabled = true;
                passengers = carpetObj.GetComponent<pplOnCar>().carpetList;
                passengerIDs = passengers.ToArray();

            }
            /*else
            {
                rightWristBand.GetComponent<MeshRenderer>().enabled = false;
                leftWristBand.GetComponent<MeshRenderer>().enabled = false;
            }*/

            if (onCarpet == false && grouped == true && backToCarCheck == true)
            {
                
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
        }
    }

    private void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        if (collision.gameObject.name == "carpet(Clone)")
        {
            if (collision.gameObject.transform.localScale.x > 0.1)
            {
                carpetObj = collision.gameObject;
                oldCarpet = carpetObj;
                grouped = true;
                onCarpet = true;
            }
        }
    }
    private void OnCollisionExit(Collision collision) //*************** When user enters the carpet
    {
        if (collision.gameObject.name == "carpet(Clone)")
        {
            index = oldCarpet.transform.GetComponent<pplOnCar>().carpetPosList.Count - 1;
            carpetObj = null;
            onCarpet = false;
        }
        if (collision.gameObject.name == "ComicHandRight(Clone)")
        {
            if (carpetObj != null && !carpIsMine && navigatorMode)
            {   
                if(collision.gameObject.transform.position.y > highFiveHeight)
                TransferOwner();
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

    public void carpetOwnershipCheck()
    {
        if (carpetObj != null && carpetObj.GetComponent<PhotonView>().IsMine)
        {
            carpIsMine = true;

            //Debug.Log("This is my carpet  " + carpIsMine);
        }
        else
        {
            carpIsMine = false;

            //Debug.Log("This is my carpet" + carpIsMine);
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

    [PunRPC]
    void SwitchingTechnique(bool check)
    {
        if (photonView.IsMine)
        {
            //GameObject Hand = GameObject.Find("/ViewingSetup/Platform/ControllerRight/ComicHandRight(Clone)");
            GetComponent<CarpetNav>().navigatorMode = check;
            Debug.Log("RPC Recieved to " + GetComponent<PhotonView>().OwnerActorNr);
        }
    }
}
