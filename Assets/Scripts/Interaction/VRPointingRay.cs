using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public enum HandSide { Right, Left };

public class VRPointingRay : MonoBehaviourPun, IPunOwnershipCallbacks
{
    // Implementation of a pointing ray for a desktop user

    public float maxRayLength = 100.0f;
    public float rayRadius = 0.05f;
    public LayerMask rayCastLayers;

    public SteamVR_Action_Boolean rayActivationAction;
    public SteamVR_Action_Boolean rayDraggingAction;
    public HandSide controllerHand;

    private SteamVR_Input_Sources handType;
    private GameObject controllerObject;
    private GameObject rayVisCylinder;
    private GameObject carpet;
    private bool rayCastActive = false;
    private bool carpetActive = false;
    private RaycastHit currentHit;

    private DraggableObject requestedOwnershipObject;
    private DraggableObject draggedObject;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            if (controllerHand == HandSide.Left)
            {
                handType = SteamVR_Input_Sources.LeftHand;
                controllerObject = GameObject.Find("/ViewingSetup/Platform/ControllerLeft");
            }
            else
            {
                handType = SteamVR_Input_Sources.RightHand;
                controllerObject = GameObject.Find("/ViewingSetup/Platform/ControllerRight");
            }
            rayVisCylinder = PhotonNetwork.Instantiate("Interaction/PointingRay", Vector3.zero, Quaternion.identity);
            rayVisCylinder.name = "PointingRay";
            rayVisCylinder.GetComponent<PhotonTransformView>().m_UseWorldTransform = true;
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = false;
            rayVisCylinder.transform.SetParent(controllerObject.transform);

            carpet = PhotonNetwork.Instantiate("Interaction/carpet", Vector3.zero, Quaternion.identity);
            carpet.GetComponent<MeshRenderer>().enabled = false;
            carpet.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            carpet.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (rayActivationAction.GetStateDown(handType))
        {
            ActivateRayCast(true);
        }
        else if (rayActivationAction.GetStateUp(handType))
        {
            ActivateRayCast(false);
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = false;
        }

        if (rayCastActive)
        {
            Ray ray = new Ray(controllerObject.transform.position, controllerObject.transform.forward);
            //if (!draggedObject)

            currentHit = ComputeIntersection(ray);
            
            UpdateRayTransform(currentHit, ray);
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = true;
            //Debug.Log("HIT OBJECT NAME  ===  " + currentHit.collider.name);
            if (rayDraggingAction.GetStateDown(handType))
            {
                carpet.transform.position = currentHit.point;
                carpet.GetComponentInChildren<MeshRenderer>().enabled = true;
                carpet.GetComponent<BoxCollider>().enabled = true;
                carpetActive = true;
                

                /*if (currentHit.collider)
                {
                    DraggableObject dragComponent = currentHit.collider.gameObject.GetComponent<DraggableObject>();
                    //Debug.Log("HIT OBJECT Try Requesting ownershippp  ===  " + currentHit.collider.name);
                    if (dragComponent)
                    {
                        //Debug.Log("HIT OBJECT Requesting ownershippp  ===  " + currentHit.collider.name);
                        requestedOwnershipObject = dragComponent;
                        dragComponent.HandleOwnershipRequest();
                    }
                }*/
            }
            if (rayDraggingAction.GetStateUp(handType))
            {
                carpetActive = false;
            }
             if(carpetActive)
            {
                float  carpetSize = Vector3.Distance(carpet.transform.position, currentHit.point);
                Debug.Log("Carpet size" + carpetSize);
                carpet.transform.localScale = new Vector3(carpetSize, 1.0f, carpetSize);
                //carpet.GetComponent<BoxCollider>().size = new Vector3(carpetSize, 2.0f, carpetSize);
            }
        }

        if (rayDraggingAction.GetStateUp(handType))
        {
            if (draggedObject)
            {
                draggedObject.OnDraggingEnd();
                draggedObject = null;
            }
        }
    }

    private RaycastHit ComputeIntersection(Ray ray)
    {
        RaycastHit hit;
        Physics.Raycast(ray.origin, ray.direction, out hit, maxRayLength, rayCastLayers);
        return hit;
    }

    private void UpdateRayTransform(RaycastHit hit, Ray ray)
    {
        Vector3 handPosition = controllerObject.transform.position;
        Vector3 goalPosition;

        if (draggedObject)
        {
            goalPosition = ray.origin + currentHit.distance * ray.direction;
        }
        else if (hit.collider != null)
        {
            goalPosition = hit.point;
        }
        else
        {
            goalPosition = handPosition + maxRayLength * ray.direction;
        }

        rayVisCylinder.transform.position = handPosition;
        rayVisCylinder.transform.LookAt(goalPosition);
        rayVisCylinder.transform.localScale = new Vector3(rayRadius, rayRadius, Vector3.Distance(handPosition, goalPosition) / 2.0f);
    }

    public void ActivateRayCast(bool active)
    {
        rayCastActive = active;
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) { }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (requestedOwnershipObject && photonView.IsMine)
        {
            if (requestedOwnershipObject && targetView == requestedOwnershipObject.photonView && targetView.AmOwner)
            {
                requestedOwnershipObject.GetComponent<DraggableObject>().OnDraggingStart(controllerObject);
                draggedObject = requestedOwnershipObject;
                requestedOwnershipObject = null;
            }
        }

    }

    public RaycastHit GetHit()
    {
        return currentHit;
    }

    public SteamVR_Input_Sources GetHandType()
    {
        return handType;
    }

    public GameObject GetControllerObject()
    {
        return controllerObject;
    }
}
