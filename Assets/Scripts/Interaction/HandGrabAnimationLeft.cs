using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;
using Photon.Realtime;

public class HandGrabAnimationLeft : MonoBehaviourPun
{
    public SteamVR_Input_Sources handType;
    public Animator anim;
    public SteamVR_Action_Boolean handHoldAction;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (handHoldAction.GetStateDown(handType))
            {
                anim.SetBool("Check", true);
            }
            if (handHoldAction.GetStateUp(handType))
            {
                anim.SetBool("Check", false);
            }
        }
    }
}
