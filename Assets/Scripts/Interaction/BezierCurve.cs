using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BezierCurve : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool endPointDetected;

    public LayerMask rayCastLayers;

    public Vector3 EndPoint
    {
        get { return endpoint; }
    }

    public LineRenderer curvedLine;
    private Vector3 endpoint;
    private Vector3[] controlPoints;
    private int segmnetCount = 40;
    private float extendStep;
    private float extensionFactor;
    private GameObject controllerObject;
    private Transform controllerTrans;
    private Transform networkControllerTrans;
    private bool drawLine = false;
    //private int layerMask = 1 << 8;

    void Start()
    {
        controlPoints = new Vector3[3];
        curvedLine.enabled = false;
        extendStep = 20f;
        controllerObject = GameObject.Find("/ViewingSetup/Platform/ControllerRight");
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            controllerTrans = controllerObject.transform;
            UpdateControlPoints(controllerTrans);
        }
        else
        {
            networkControllerTrans = transform.parent.gameObject.transform;
            UpdateControlPoints(networkControllerTrans);
        }
        HandleExtention();
        DrawCurvedLine();
        DrawLine(drawLine);
    }


    public void GetDrawLine(bool draw)
    {
        drawLine = draw;
    }

    public void DrawLine(bool drawline)
    {
        curvedLine.enabled = drawline;
    }

    void HandleExtention()
    {
        if (extensionFactor == 0f)
            return;

        float finalExtention = extendStep + Time.deltaTime * 3f;
        extendStep = Mathf.Clamp(finalExtention, 2.5f, 7.5f);
    }

    void UpdateControlPoints(Transform cntrlTrans)
    {
        controlPoints[0] = cntrlTrans.position; //Contoller position
        controlPoints[1] = controlPoints[0] + (cntrlTrans.transform.forward * extendStep * 2f / 5f); //middle point
        controlPoints[2] = controlPoints[0] + (cntrlTrans.transform.forward * extendStep * 3f / 5f) + Vector3.up * -12f; //final location
    }

    void DrawCurvedLine()
    {
        curvedLine.positionCount = 1;
        curvedLine.SetPosition(0, controlPoints[0]);

        Vector3 prevPosition = controlPoints[0];
        Vector3 nextPosition = prevPosition;
        for (int i = 1; i < segmnetCount; i++)
        {
            float t = i / (float)segmnetCount;
            curvedLine.positionCount = i + 1;

            if (i == segmnetCount)
            {
                Vector3 endDirection = Vector3.Normalize(prevPosition - curvedLine.GetPosition(i - 2));
                nextPosition = prevPosition + endDirection * 2f;
            }

            else
            {
                nextPosition = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);
            }

            if (CheckCollider(prevPosition, nextPosition))
            {
                curvedLine.SetPosition(i, endpoint);
                endPointDetected = true;
                return;
            }

            else
            {
                curvedLine.SetPosition(i, nextPosition);
                endPointDetected = false;
                prevPosition = nextPosition;
            }
        }
    }

    bool CheckCollider(Vector3 start, Vector3 end)
    {
        Ray r = new Ray(start, end - start);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, Vector3.Distance(start, end), rayCastLayers))
        {
            endpoint = hit.point;
            return true;
        }
        else
        {
            return false;
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return
            Mathf.Pow((1f - t), 2) * p0 +
            2f * (1f - t) * t * p1 +
            Mathf.Pow(t, 2) * p2;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //bool drawCheck = drawLine;
            //stream.SendNext(drawCheck);
        }
        else
        {
            //bool drawcheck = (bool)stream.ReceiveNext();
            //drawLine = drawcheck;
        }
    }

}    //Reference: https://github.com/FusedVR/GearTeleporter/blame/master/Assets/Teleporter/Scripts/Bezier.cs 
