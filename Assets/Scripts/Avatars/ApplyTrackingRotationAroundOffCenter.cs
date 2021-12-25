using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyTrackingRotationAroundOffCenter : MonoBehaviour
{
    // Applies the rotation of the specified prefab root
    // around an external rotation center

    public GameObject prefabRoot;
    public Vector3 localRotCenter = Vector3.zero;

    private Vector3 localPosition;
    private Vector3 localEulerAngles;
    private Vector3 localScale;

    void Start()
    {
        localPosition = this.transform.localPosition;
        localEulerAngles = this.transform.localEulerAngles;
        localScale = this.transform.localScale;
    }

    void LateUpdate()
    {
        ResetTransform();
        Vector3 trackingRotationAngles = prefabRoot.transform.eulerAngles;
        this.transform.Translate(localRotCenter, Space.Self);
        this.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), trackingRotationAngles.y, Space.Self);
        this.transform.Rotate(new Vector3(1.0f, 0.0f, 0.0f), trackingRotationAngles.x, Space.Self);
        this.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), trackingRotationAngles.z, Space.Self);
        this.transform.Translate(-localRotCenter, Space.Self);
        this.transform.Translate(localPosition, Space.Self);
        this.transform.Rotate(localEulerAngles, Space.Self);
        this.transform.localScale = localScale;
        
    }

    void ResetTransform()
    {
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
    }
}
