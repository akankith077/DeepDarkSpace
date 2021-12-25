using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using TMPro;

public class UserManager : MonoBehaviourPunCallbacks
{
    // Class instantiated for each user in a room to manage their
    // avatar behavior

    // reference to UserManager instance related to the local user
    public static GameObject localUserInstance;
    private static GameObject teleportIndicator;
    private static GameObject groupTeleIndicator;

    public GameObject shirtGeometry;

    void Awake()
    {
        this.transform.GetComponentInChildren<TMP_Text>().text = photonView.Owner.NickName;

        if (photonView.IsMine)
        {
            UserManager.localUserInstance = this.gameObject;
            bool vrMode = PlayerPrefs.GetInt(GlobalSettings.vrEnabledPrefKey) != 0.0;
            string shirtColor = PlayerPrefs.GetString(GlobalSettings.colorPrefKey);

            // hide own avatar and attach it to camera transform
            SetLayerRecursively(this.gameObject, GlobalSettings.hideLayer);
            this.transform.SetParent(Camera.main.transform, false);

            // create tracked hands for VR avatar
            if (vrMode)
            {
                string avatarType = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);
                teleportIndicator = PhotonNetwork.Instantiate("Interaction/" + avatarType + " Tele", new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0);
                teleportIndicator.name = "TELE";

                GameObject leftHand = PhotonNetwork.Instantiate(GlobalSettings.vrResourcesPath + "ComicHandLeft", new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0);
                GameObject rightHand = PhotonNetwork.Instantiate(GlobalSettings.vrResourcesPath + "ComicHandRight", new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0);
                GameObject trackingLeftHand = GameObject.Find("/ViewingSetup/Platform/ControllerLeft");
                GameObject trackingRightHand = GameObject.Find("/ViewingSetup/Platform/ControllerRight");
                leftHand.transform.SetParent(trackingLeftHand.transform, false);
                rightHand.transform.SetParent(trackingRightHand.transform, false);

                GameObject platform = GameObject.Find("/ViewingSetup/Platform");
                teleportIndicator.transform.SetParent(platform.transform, false);
            }

            this.photonView.RPC("SetShirtColor", RpcTarget.AllBuffered, new object[] { shirtColor });
        } else
        {
            this.tag = "Avatar";
        }
    }

    void SetLayerRecursively(GameObject startObject, int layer)
    {
        startObject.layer = layer;

        foreach (Transform child in startObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    [PunRPC]
    void SetShirtColor(string colorName)
    {
        Material shirtMaterial = Resources.Load<Material>(GlobalSettings.shirtMaterialsPath + colorName);
        shirtGeometry.GetComponent<Renderer>().material = shirtMaterial;
    }
}
