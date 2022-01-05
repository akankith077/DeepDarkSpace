﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class pplOnCar : MonoBehaviourPunCallbacks
{
    public Hashtable passenger;
    private Vector3 initialCarPos;
    private Vector3 newCarPos;
    public List<int> passengers = new List<int>();
    public List<Vector3> carPos = new List<Vector3>();
    private List<pplOnCar> ppls = new List<pplOnCar>();
    private int[] passengerIDs = { };
    private int i = 0;
    public int listLength = 0;
    void Start()
    {
        
        //List<Part> parts = new List<Part>();
        //parts.Add(new Part() { PartName = "crank arm", PartId = 1234 });
    }

    // Update is called once per frame
    void Update()
    {
        initialCarPos = this.transform.position;
        if (initialCarPos != newCarPos)
        {
            //ppls.Add(new pplOnCar() { Index = i, CarpetPos = newCarPos });
            carPos.Add(initialCarPos);
            newCarPos = initialCarPos;
            //i++;
        }
        listLength = carPos.Count;
    }

    public int Index { get; set; }

    public Vector3 CarpetPos { get; set; }

    private void OnCollisionEnter(Collision collision) //*************** When user enters the carpet
    {
        int checkID = collision.gameObject.GetComponent<PhotonView>().OwnerActorNr;
        string name = collision.gameObject.GetComponent<PhotonView>().Owner.NickName;//*************** Checks Actor number
        passengers.Add(checkID);
        passengerIDs = passengers.ToArray();
    }
    private void OnCollisionExit(Collision collision) //*************** When user enters the carpet
    {
        int checkID = collision.gameObject.GetComponent<PhotonView>().OwnerActorNr;
        string name = collision.gameObject.GetComponent<PhotonView>().Owner.NickName;//*************** Checks Actor number
        passengers.Remove(checkID);
        passengerIDs = passengers.ToArray();
    }

    public List<int> carpetList
    {
        get { return passengers; }
    }

    public List<Vector3> carpetPosList
    {
        get { return carPos; }
    }

    public List<pplOnCar> carpetPositionsList
    {
        get { return ppls; }
    }

}