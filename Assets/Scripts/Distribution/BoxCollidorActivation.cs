using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCollidorActivation : MonoBehaviourPunCallbacks, IPunObservable
{
    // Defines network serialization and de-serialization for the "enabled"
    // property of the MeshRenderer component.

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
                bool BoxCollidorActivated = GetComponent<BoxCollider>().enabled;
                stream.SendNext(BoxCollidorActivated);
        }
        else
        {
                bool BoxCollidorActivated = (bool)stream.ReceiveNext();
                GetComponent<BoxCollider>().enabled = BoxCollidorActivated;
        }
    }
}
