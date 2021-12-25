using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DraggableObject : MonoBehaviourPun, IPunOwnershipCallbacks
{
    // Makes an attached child object draggable

    public Material highlightMaterial;

    private bool objectIsCurrentlyDragged = false;
    private GameObject parentBeforeDragging;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            Material[] materials = mr.materials;
            Material[] newMaterials = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; ++i)
            {
                newMaterials[i] = materials[i];
            }
            newMaterials[materials.Length] = highlightMaterial;
            mr.materials = newMaterials;
        }
        
    }

    public void HandleOwnershipRequest()
    {
        base.photonView.RequestOwnership();
        Debug.Log("Ownership being requested");
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (targetView != base.photonView)
        {
            return;
        }

        if (!objectIsCurrentlyDragged)
        {
            base.photonView.TransferOwnership(requestingPlayer);
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (targetView != base.photonView)
            return;
    }

    public void OnDraggingStart(GameObject controller)
    {
        if (this.transform.parent)
        {
            parentBeforeDragging = this.transform.parent.gameObject;
        }
        else
        {
            parentBeforeDragging = null;
        }
        
        this.transform.SetParent(controller.transform);
        objectIsCurrentlyDragged = true;
    }

    public void OnDraggingEnd()
    {
        if (parentBeforeDragging)
        {
            this.transform.SetParent(parentBeforeDragging.transform);
        }
        else
        {
            this.transform.SetParent(null);
        }
        
        parentBeforeDragging = null;
        objectIsCurrentlyDragged = false;
    }
}
