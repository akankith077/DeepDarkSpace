using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Valve.VR;

public class ArrowDistribution : MonoBehaviourPunCallbacks
{
    private Renderer[] renderChildren;
    private Renderer renderSelf;
    public GameObject shirtObject;
    private bool buttonCheck = true;
    public SteamVR_Action_Boolean activation;
    // Start is called before the first frame update
    void Start()
    {
        renderChildren = GetComponentsInChildren<MeshRenderer>();
        renderSelf = this.GetComponent<MeshRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        ButtonCheck();
        if (renderSelf.enabled == true)
            Activate();
        else
            DeActivate();
    }
    void Activate()
    {
        Renderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (Renderer r in renderers)
            r.enabled = true;
    }

    void DeActivate()
    {
        Renderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
    }
    void ButtonCheck()
    {
        if (activation.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (renderSelf.enabled)
            {
                renderSelf.enabled = false;
            }
            else
            {
                renderSelf.enabled = true;
            }
        }
    }
}
