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


    private bool teleButtonCheck = false;

    void Start()
    {
        if (photonView.IsMine)
        {
            platformObj = GameObject.Find("/ViewingSetup/Platform");
            hmdObj = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
            teleportIndicator = GameObject.Find("/ViewingSetup/Platform/TELE");
        }
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            ButtonCheck();
            
            if (teleButtonCheck && carpetObj != null)
            {   
                Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
                Vector3 translateVector = groundPosition - carpetObj.transform.position;
                carpetObj.transform.GetChild(0).position = teleportIndicator.transform.position - translateVector;
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                //carpetObj.gameObject.transform.SetParent(platformObj.transform); 
            }
            else if(!teleButtonCheck && carpetObj != null)
            {
                carpetObj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }

            if (groupTeleportationConfirm.GetStateDown(handType) && carpetObj != null)
            {
                Vector3 groundPosition = new Vector3(hmdObj.transform.position.x, platformObj.transform.position.y, hmdObj.transform.position.z);
                Vector3 translateVector = teleportIndicator.transform.position - groundPosition;
                carpetObj.transform.position += translateVector;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        
        carpetObj = collision.gameObject;
        
    }
    private void OnCollisionExit(Collision collision) //*************** When user enters the carpet
    {
        
        carpetObj = null;

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
}
