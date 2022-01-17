using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TechniqueSwitch : MonoBehaviourPunCallbacks
{
    public bool withNavigator = false;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CarpetNav>().enabled = true;
        GetComponent<CarpetNavOwnerTransf>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if ((withNavigator == false) && (GetComponent<CarpetNavOwnerTransf>().enabled == true))
            {
                GetComponent<CarpetNav>().enabled = true;
                GetComponent<CarpetNavOwnerTransf>().enabled = false;
                photonView.RPC("SwitchingTechnique", RpcTarget.AllViaServer, new object[] { withNavigator });
            }
            if ((withNavigator == true) && (GetComponent<CarpetNav>().enabled == true))
            {
                GetComponent<CarpetNav>().enabled = false;
                GetComponent<CarpetNavOwnerTransf>().enabled = true;
                //Debug.Log("In the SWITCH LOOP");
                photonView.RPC("SwitchingTechnique", RpcTarget.AllViaServer, new object[] { withNavigator });
            }
        }
    }

    [PunRPC]
    void SwitchingTechnique(bool check)
    {   
         if (photonView.IsMine)
        {
        GameObject Hand = GameObject.Find("/ViewingSetup/Platform/ControllerRight/ComicHandRight(Clone)");
        Hand.GetComponent<CarpetNav>().enabled = !check;
        Hand.GetComponent<CarpetNavOwnerTransf>().enabled = check;
        Debug.Log("RPC Recieved to " + Hand.GetComponent<PhotonView>().OwnerActorNr);
        }
    }
}
