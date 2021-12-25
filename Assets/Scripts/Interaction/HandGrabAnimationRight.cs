using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;
using Photon.Realtime;

public class HandGrabAnimationRight : MonoBehaviourPun
{
    public SteamVR_Input_Sources handType;
    public Animator anima;
    public SteamVR_Action_Boolean handHoldAction;

    void Awake()
    {
        anima = GetComponent<Animator>();
    }
    void Update()
    {
        if (photonView.IsMine)
        {
            if (handHoldAction.GetStateDown(handType))
            {
                anima.SetBool("Check", true);
            }
            if (handHoldAction.GetStateUp(handType))
            {
                anima.SetBool("Check", false);
            }
        }
    }
}
