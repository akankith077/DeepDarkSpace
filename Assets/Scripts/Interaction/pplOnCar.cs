using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class pplOnCar : MonoBehaviourPunCallbacks
{
    public Hashtable passenger;
    public List<int> passengers = new List<int>();
    private int[] passengerIDs = { };
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*for (int i = 0; i < passengerIDs.Length; i++)
        {
            Debug.Log("Passengers on the carpet are: " + passengerIDs[i]);
        }*/
    }

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
}