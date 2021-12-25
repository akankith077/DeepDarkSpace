using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererActiveStateDistribution : MonoBehaviourPunCallbacks, IPunObservable
{
    // Defines network serialization and de-serialization for the "enabled"
    // property of the MeshRenderer component.

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       if (stream.IsWriting)
       {
            bool meshRendererActivated = GetComponentInChildren<MeshRenderer>().enabled;
            stream.SendNext(meshRendererActivated);
       }
       else
       {
            bool meshRendererActivated = (bool)stream.ReceiveNext();
            GetComponentInChildren<MeshRenderer>().enabled = meshRendererActivated;
        }
    }
}
