using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TechniqueSwitch : MonoBehaviourPun
{
    public bool withNavigator = false;
    public bool check = false;
    void Update()
    {
        if (photonView.IsMine)
        {
            if ((withNavigator == false) && check )
            {
                photonView.RPC("SwitchingTechnique", RpcTarget.AllBufferedViaServer, new object[] { withNavigator });
                check = false;
            }
            else if ((withNavigator == true) &&  check)
            {
                photonView.RPC("SwitchingTechnique", RpcTarget.AllBufferedViaServer, new object[] { withNavigator });
                check = false;
            }
        }
    }

}
