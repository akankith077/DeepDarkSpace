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
                photonView.RPC("SwitchingTechnique", RpcTarget.AllBuffered, new object[] { withNavigator });
            }
            if ((withNavigator == true) && (GetComponent<CarpetNav>().enabled == true))
            {
                GetComponent<CarpetNav>().enabled = false;
                GetComponent<CarpetNavOwnerTransf>().enabled = true;
                //Debug.Log("In the SWITCH LOOP");
                photonView.RPC("SwitchingTechnique", RpcTarget.AllBuffered, new object[] { withNavigator });
            }
        }
    }

    [PunRPC]
    void SwitchingTechnique(bool check)
    {   
        GetComponent<CarpetNav>().enabled = !check;
        GetComponent<CarpetNavOwnerTransf>().enabled = check;
        Debug.Log("RPC Recieved to " + GetComponent<PhotonView>().OwnerActorNr);
    }
}
