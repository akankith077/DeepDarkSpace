using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DesktopPointingRay : MonoBehaviourPun, IPunOwnershipCallbacks
{ 
    // Implementation of a pointing ray for a desktop user

    public float maxRayLength = 100.0f;
    public float rayRadius = 0.05f;
    public LayerMask rayCastLayers;
    public GameObject cursorRotationObject;
    public GameObject handForFeedbackRay;
    public KeyCode rayActivationKey = KeyCode.Mouse0;
    public KeyCode rayDraggingKey = KeyCode.Mouse1;

    private GameObject rayVisCylinder;
    private bool rayCastActive = false;
    private RaycastHit currentHit;

    private DraggableObject requestedOwnershipObject;
    private DraggableObject draggedObject;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(rayActivationKey))
        {
            ActivateRayCast(true);
        } 
        else if (Input.GetKeyUp(rayActivationKey))
        {
            ActivateRayCast(false);
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = false;
        }

        if (rayCastActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            cursorRotationObject.transform.forward = ray.direction;
            if (!draggedObject)
            {
                currentHit = ComputeIntersection(ray);
            }
            UpdateRayTransform(currentHit, ray);
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = true;

            if (Input.GetKeyDown(rayDraggingKey))
            {
                if (currentHit.collider)
                {
                    DraggableObject dragComponent = currentHit.collider.gameObject.GetComponentInParent<DraggableObject>();
                    if (dragComponent)
                    {
                        requestedOwnershipObject = dragComponent;
                        dragComponent.HandleOwnershipRequest();
                    }
                }
            }
        }

        if (Input.GetKeyUp(rayDraggingKey))
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
        Vector3 handPosition = handForFeedbackRay.transform.position;
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
        handForFeedbackRay.transform.LookAt(goalPosition);
    }

    public void ActivateRayCast(bool active)
    {
        rayCastActive = active;
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) {}

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (requestedOwnershipObject && photonView.IsMine)
        {
            if (requestedOwnershipObject && targetView == requestedOwnershipObject.photonView && targetView.AmOwner)
            {
                requestedOwnershipObject.GetComponent<DraggableObject>().OnDraggingStart(cursorRotationObject);
                draggedObject = requestedOwnershipObject;
                requestedOwnershipObject = null;
            }
        }
        
    }


    public RaycastHit GetHit()
    {
        return currentHit;
    }
}
