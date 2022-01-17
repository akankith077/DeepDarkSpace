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
        this.GetComponent<CarpetNav>().enabled = true;
        this.GetComponent<CarpetNavOwnerTransf>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!withNavigator && this.GetComponent<CarpetNav>().enabled == false)
        {
            this.GetComponent<CarpetNav>().enabled = true;
            this.GetComponent<CarpetNavOwnerTransf>().enabled = false; 
            this.photonView.RPC("SwitchingTechnique", RpcTarget.AllBuffered, new object[] { withNavigator });
        }
        if (withNavigator && this.GetComponent<CarpetNavOwnerTransf>().enabled == false)
        {
            this.GetComponent<CarpetNav>().enabled = false;
            this.GetComponent<CarpetNavOwnerTransf>().enabled = true;
            //Debug.Log("In the SWITCH LOOP");
            this.photonView.RPC("SwitchingTechnique", RpcTarget.AllBuffered, new object[] { withNavigator });
        }
    }

    [PunRPC]
    void SwitchingTechnique(bool check)
    {
        this.GetComponent<CarpetNav>().enabled = !check;
        this.GetComponent<CarpetNavOwnerTransf>().enabled = check;
        Debug.Log("RPC Recieved to " + GetComponent<PhotonView>().OwnerActorNr);
    }
}
