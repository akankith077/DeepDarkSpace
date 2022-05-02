using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvedRay : MonoBehaviour
{

    public LayerMask rayCastLayers;

    private Vector3 endpoint;
    private Vector3 midpoint;
    private Vector3 headSetMidpoint;
    private Vector3 controllerMidpoint;
    private Transform teleTranform;
    private Vector3[] controlPoints;
    public LineRenderer curvedLine;
    private float extendStep;
    private float extensionFactor;
    private int segmnetCount = 40;
    private GameObject controllerObject;
    private GameObject controllerObjectRight;
    private GameObject controllerObjectLeft;
    private GameObject cameraObject;
    private Transform controllerTrans;
    private Transform networkControllerTrans;
    private bool drawLine = false;
    // Start is called before the first frame update
    void Start()
    {
        teleTranform = this.transform;
        controlPoints = new Vector3[3];
        curvedLine.enabled = false;
        extendStep = 10f;
        controllerObjectLeft = GameObject.Find("/ViewingSetup/Platform/ControllerLeft");
        controllerObjectRight = GameObject.Find("/ViewingSetup/Platform/ControllerRight");
        cameraObject = GameObject.Find("/ViewingSetup/Platform/HMDCamera");
        controllerObject = controllerObjectLeft;
        midpoint = headSetMidpoint;
    }

    void Update()
    {
            controllerTrans = controllerObject.transform;
            headSetMidpoint = cameraObject.transform.position + (cameraObject.transform.forward * extendStep * 2f / 5f) + new Vector3(0.0f, 1.0f, 0.0f);
            controllerMidpoint = controllerTrans.transform.position + (controllerTrans.transform.forward * extendStep * 2f / 5f);
            UpdateControlPoints(controllerTrans);
        HandleExtention();
        DrawCurvedLine();
        DrawLine(drawLine);
        controlPoints[2] = this.transform.position;
    }


    public void GetDrawLine(bool draw)
    {
        drawLine = draw;
        controllerObject = controllerObjectLeft;
        midpoint = headSetMidpoint;
    }

    public void GetDrawRightLine(bool draw)
    {
        drawLine = draw;
        controllerObject = controllerObjectRight;
        midpoint = controllerMidpoint;
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
        controlPoints[1] = midpoint;//middle point


    }
    void DrawCurvedLine()
    {
        if (!curvedLine.enabled)
            return;
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

            //curvedLine.SetPosition(i, endpoint.position);

            if (CheckCollider(prevPosition, nextPosition))
            {
                curvedLine.SetPosition(i, endpoint);
                return;
            }

            else
            {
                curvedLine.SetPosition(i, nextPosition);
                prevPosition = nextPosition;
            }
        }
    }

    bool CheckCollider(Vector3 start, Vector3 end)
    {
        Ray r = new Ray(start, end - start);
        RaycastHit hit;
        if (endpoint == teleTranform.position )
        {
            endpoint = teleTranform.position;
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

}
