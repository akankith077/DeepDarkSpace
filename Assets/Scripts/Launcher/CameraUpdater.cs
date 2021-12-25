using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUpdater : MonoBehaviour
{
    // Updates the camera rotation in the launcher menu
    private Camera cam;

    void Awake()
    {
        cam = this.GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 rotationPoint = cam.transform.position + 80.0f * cam.transform.forward;
        cam.transform.RotateAround(rotationPoint, Vector3.up, 10 * Time.deltaTime);
    }
}
