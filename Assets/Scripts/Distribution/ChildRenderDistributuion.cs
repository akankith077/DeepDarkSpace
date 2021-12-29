using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChildRenderDistributuion : MonoBehaviourPunCallbacks
{
    private Renderer[] renderChildren;
    private Renderer renderSelf;
    // Start is called before the first frame update
    void Start()
    {
        renderChildren = GetComponentsInChildren<MeshRenderer>();
        renderSelf = this.GetComponent<MeshRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
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

}
