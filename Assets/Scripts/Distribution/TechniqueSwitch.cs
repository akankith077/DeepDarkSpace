using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TechniqueSwitch : MonoBehaviourPunCallbacks
{
    public bool withNavigator = false;
    public bool check = false;
    void Update()
    {
        if (photonView.IsMine)
        {
            if ((withNavigator == false) && check )
            {
                photonView.RPC("SwitchingTechnique", RpcTarget.AllViaServer, new object[] { withNavigator });
                check = false;
            }
            else if ((withNavigator == true) &&  check)
            {
                photonView.RPC("SwitchingTechnique", RpcTarget.AllViaServer, new object[] { withNavigator });
                check = false;
            }
        }
    }

    [PunRPC]
    void SwitchingTechnique(bool check)
    {   
         if (photonView.IsMine)
        {
        GameObject Hand = GameObject.Find("/ViewingSetup/Platform/ControllerRight/ComicHandRight(Clone)");
        Hand.GetComponent<CarpetNav>().navigatorMode = check;
        Debug.Log("RPC Recieved to " + GetComponent<PhotonView>().OwnerActorNr);
        }
    }
}
