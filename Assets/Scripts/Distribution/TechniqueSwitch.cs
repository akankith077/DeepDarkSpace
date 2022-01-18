using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TechniqueSwitch : MonoBehaviourPunCallbacks
{
    public bool withNavigator = false;
    private bool check = false;
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
        //GameObject Hand = GameObject.Find("/ViewingSetup/Platform/ControllerRight/ComicHandRight(Clone)");
        GetComponent<CarpetNav>().enabled = !check;
        GetComponent<CarpetNavOwnerTransf>().enabled = check;
        Debug.Log("RPC Recieved to " + GetComponent<PhotonView>().OwnerActorNr);
        }
    }
}
