using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraYawOnly : MonoBehaviour
{
    // Cancels the pitch (x) and roll (z) rotations of this game object
    // such that children are only affected by the translation and
    // yaw (y) angle of its parent

    public GameObject prefabRoot;

    void LateUpdate()
    {
        Vector3 prefabRootEulerAngles = prefabRoot.transform.eulerAngles;
        this.transform.rotation = Quaternion.Euler(0.0f, prefabRootEulerAngles.y, 0.0f);
    }
}
