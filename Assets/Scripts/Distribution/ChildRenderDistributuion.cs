using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChildRenderDistributuion : MonoBehaviourPunCallbacks
{
    private Renderer[] renderChildren;
    private Renderer renderSelf;

    public GameObject shirtObject;


    // Start is called before the first frame update
    void Start()
    {
        renderChildren = GetComponentsInChildren<MeshRenderer>();
        renderSelf = this.GetComponent<MeshRenderer>();
        if (photonView.IsMine)
        {
            string shirtColor = PlayerPrefs.GetString(GlobalSettings.colorPrefKey);
            this.photonView.RPC("SetTransparentShirtColor", RpcTarget.AllBuffered, new object[] { shirtColor });
        }
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

    [PunRPC]
    void SetTransparentShirtColor(string colorName)
    {
        Material shirtMaterial = Resources.Load<Material>(GlobalSettings.shirtMaterialsPath + colorName + " 1");
        shirtObject.GetComponent<Renderer>().material = shirtMaterial;

    }

}
