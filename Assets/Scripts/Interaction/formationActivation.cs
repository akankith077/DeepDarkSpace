using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class formationActivation : MonoBehaviour
{
    public CarpetNav carpetNavScript;
    public GameObject circle;
    public GameObject semiCircle;
    public GameObject presenter;

    public SteamVR_Action_Boolean cicleAct;
    public SteamVR_Action_Boolean semiCircAct;
    public SteamVR_Action_Boolean presenterAct;

    public Material ON;
    public Material OFF;

    public SteamVR_Input_Sources handType;

    void Update()
    {   
        if(carpetNavScript.carpetObj == null){
            circle.transform.GetComponent<MeshRenderer>().enabled = false;
            semiCircle.transform.GetComponent<MeshRenderer>().enabled = false;
            presenter.transform.GetComponent<MeshRenderer>().enabled = false;
        }
        else{
            circle.transform.GetComponent<MeshRenderer>().enabled = true;
            semiCircle.transform.GetComponent<MeshRenderer>().enabled = true;
            presenter.transform.GetComponent<MeshRenderer>().enabled = true;
        }
        if (cicleAct.GetStateDown(handType))
        {
            circle.GetComponent<Renderer>().material = ON;
            semiCircle.GetComponent<Renderer>().material = OFF;
            presenter.GetComponent<Renderer>().material = OFF;
        }

        if (semiCircAct.GetStateDown(handType))
        {
            circle.GetComponent<Renderer>().material = OFF;
            semiCircle.GetComponent<Renderer>().material = ON;
            presenter.GetComponent<Renderer>().material = OFF;
        }

        if (presenterAct.GetStateDown(handType))
        {
            circle.GetComponent<Renderer>().material = OFF;
            semiCircle.GetComponent<Renderer>().material = OFF;
            presenter.GetComponent<Renderer>().material = ON;
        }


        if (cicleAct.GetStateUp(handType) || semiCircAct.GetStateUp(handType) || presenterAct.GetStateUp(handType))
        {
            circle.GetComponent<Renderer>().material = OFF;
            semiCircle.GetComponent<Renderer>().material = OFF;
            presenter.GetComponent<Renderer>().material = OFF;
        }

    }
}
