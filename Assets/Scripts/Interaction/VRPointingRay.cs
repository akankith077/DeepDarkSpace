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
    public float myID = 0;
    public float carpetResize = 0.2f;
    public float carpetMaxSize = 3.2f;
    private float triggerSize = 0;
    public LayerMask rayCastLayers;

    public SteamVR_Action_Boolean rayActivationAction;
    public SteamVR_Action_Boolean rayDraggingAction;
    public SteamVR_Action_Boolean DelRayActive;
    public SteamVR_Action_Boolean DelRayPress;
    public SteamVR_Action_Boolean switchRay;
    public SteamVR_Action_Single Trigger;
    public HandSide controllerHand;

    private SteamVR_Input_Sources handType;
    private GameObject controllerObject;
    private GameObject viewingSetup;
    private GameObject rayVisCylinder;
    private GameObject carpet;
    private GameObject hitObj;
    public GameObject switchButton;
    private bool rayCastActive = false;
    private bool carpetActive = false;
    private bool resize = false;
    private bool carpetFound = false;
    private bool redRayActive = false;
    private bool triggerCheck = false;
    private RaycastHit currentHit;
    public Material Red;
    public Material Green;

    private DraggableObject requestedOwnershipObject;
    private DraggableObject draggedObject;

    private Vector3 hitObjPoint;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            viewingSetup = GameObject.Find("/ViewingSetup");
            myID = transform.GetComponent<PhotonView>().OwnerActorNr;
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
            carpet.transform.SetParent(viewingSetup.transform, false);
            carpet.GetComponent<MeshRenderer>().enabled = false;
            carpet.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            carpet.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;
        TriggerCheck();
        if (switchRay.GetStateDown(handType))
        {
            if (redRayActive)
            {
                redRayActive = false;
                switchButton.transform.GetComponent<Renderer>().material = Green;
                rayVisCylinder.transform.GetChild(0).GetComponent<Renderer>().material = Green;
            }
            else
            {
                redRayActive = true;
                switchButton.transform.GetComponent<Renderer>().material = Red;
                rayVisCylinder.transform.GetChild(0).GetComponent<Renderer>().material = Red;
            }
        }
        if (rayActivationAction.GetStateDown(handType) || triggerCheck)
        {
            ActivateRayCast(true);
        }
        else if (rayActivationAction.GetStateUp(handType) || !triggerCheck)
        {
            ActivateRayCast(false);
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = false;
            carpetFound = false;
        }

        if (rayCastActive)
        {
            Ray ray = new Ray(controllerObject.transform.position, controllerObject.transform.forward);
            //if (!draggedObject)

            currentHit = ComputeIntersection(ray);
            UpdateRayTransform(currentHit, ray);
            rayVisCylinder.GetComponentInChildren<MeshRenderer>().enabled = true;
            //Debug.Log("HIT OBJECT NAME  ===  " + currentHit.collider.name);
            if (currentHit.collider)
            {
                if (currentHit.collider.name == "Collider") //To Resize carpet
                {
                    hitObj = currentHit.transform.gameObject;
                    //Debug.Log("HIT OBJECT NAME  Parent Name ===  " + hitObj.transform.parent.gameObject.name);
                    //hitObj.transform.GetChild(1).transform.gameObject.layer = 2;
                    carpetFound = true;
                    hitObjPoint = currentHit.transform.position;
                }
            }
            if (DelRayPress.GetStateDown(handType))
            {
                if (currentHit.collider.name == "Collider")
                {
                    //hitObj = currentHit.transform.parent.transform.gameObject;
                    if (!hitObj.GetComponent<PhotonView>().IsMine)
                    {
                        hitObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        hitObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    hitObj.transform.localScale = new Vector3(0.0f, 1.0f, 0.0f);
                    hitObj.transform.GetComponent<MeshRenderer>().enabled = false;

                    if (hitObj.GetComponent<PhotonView>().CreatorActorNr != myID)
                    {
                        hitObj.GetComponent<PhotonView>().TransferOwnership(hitObj.GetComponent<PhotonView>().CreatorActorNr);
                        hitObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(hitObj.GetComponent<PhotonView>().CreatorActorNr);
                    }
                }
            }

            if (rayDraggingAction.GetStateDown(handType) && carpetFound == false)
            {
                if (!carpet.GetComponent<PhotonView>().IsMine)
                {
                    carpet.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    carpet.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                }
                if (currentHit.transform.name == "Floor")
                {
                    carpet.transform.position = currentHit.point;
                    carpet.GetComponentInChildren<MeshRenderer>().enabled = true;
                    carpet.GetComponent<BoxCollider>().enabled = true;
                    carpetActive = true;
                }

                if (currentHit.collider)
                {
                    DraggableObject dragComponent = currentHit.collider.gameObject.GetComponent<DraggableObject>();
                    //Debug.Log("HIT OBJECT Try Requesting ownershippp  ===  " + currentHit.collider.name);
                    if (dragComponent)
                    {
                        //Debug.Log("HIT OBJECT Requesting ownershippp  ===  " + currentHit.collider.name);
                        requestedOwnershipObject = dragComponent;
                        dragComponent.HandleOwnershipRequest();
                    }
                }
            }
            if (redRayActive && rayDraggingAction.GetStateDown(handType) && carpetFound)
            {
                if (currentHit.collider.name == "Collider")
                {
                    //hitObj = currentHit.transform.parent.transform.gameObject;
                    if (!hitObj.GetComponent<PhotonView>().IsMine)
                    {
                        hitObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        hitObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    hitObj.transform.localScale = new Vector3(0.0f, 1.0f, 0.0f);
                    hitObj.transform.GetComponent<MeshRenderer>().enabled = false;

                    if (hitObj.GetComponent<PhotonView>().CreatorActorNr != myID)
                    {
                        hitObj.GetComponent<PhotonView>().TransferOwnership(hitObj.GetComponent<PhotonView>().CreatorActorNr);
                        hitObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(hitObj.GetComponent<PhotonView>().CreatorActorNr);
                    }
                }

            }

            if (carpetActive && !redRayActive) //CREATION OF CARPET
            {
                float carpetSize = Vector3.Distance(new Vector3(carpet.transform.position.x, 0, carpet.transform.position.z), new Vector3(currentHit.point.x, 0, currentHit.point.z));
                //Debug.Log("Carpet size" + carpetSize);
                if (carpetSize > carpetMaxSize)
                {
                    carpetSize = carpetMaxSize;
                }
                carpet.transform.localScale = new Vector3(carpetSize, 1.0f, carpetSize);
                //carpet.GetComponent<BoxCollider>().size = new Vector3(carpetSize, 2.0f, carpetSize);
            }

            if (carpetFound && !redRayActive) //RESIZING OF CARPET
            {
                if (rayDraggingAction.GetStateDown(handType))
                {
                    if (!hitObj.GetComponent<PhotonView>().IsMine)
                    {
                        hitObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                        hitObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    resize = true;
                    hitObj.transform.GetChild(1).transform.gameObject.layer = 2;
                }
            }

            if (resize)
            {
                float carpetSize = Vector3.Distance(new Vector3(hitObjPoint.x, 0, hitObjPoint.z), new Vector3(currentHit.point.x, 0, currentHit.point.z));
                if (carpetSize > carpetMaxSize)
                {
                    carpetSize = carpetMaxSize;
                }
                hitObj.transform.localScale = new Vector3(carpetSize + carpetResize, 1.0f, carpetSize + carpetResize);
            }

            if (rayDraggingAction.GetStateUp(handType))
            {
                if (resize)
                {
                    hitObj.transform.GetChild(1).transform.gameObject.layer = 0;
                    if (hitObj.GetComponent<PhotonView>().CreatorActorNr != myID)
                    {
                        hitObj.GetComponent<PhotonView>().TransferOwnership(hitObj.GetComponent<PhotonView>().CreatorActorNr);
                        hitObj.transform.GetChild(0).GetComponent<PhotonView>().TransferOwnership(hitObj.GetComponent<PhotonView>().CreatorActorNr);
                    }
                    resize = false;
                }
                carpetActive = false;
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
    public void TriggerCheck()
    {
        triggerSize = Trigger.GetAxis(handType);
        if (triggerSize > 0.2)
        {
            triggerCheck = true;
            Debug.Log("tRIGGER IS PULLED");
        }
        else
        {
            triggerCheck = false;
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
