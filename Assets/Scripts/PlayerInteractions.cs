using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [Header("InteractableInfo")]
    public float sphereCastRadius = 0.5f;
    public LayerMask layers;
    private Vector3 raycastPos;
    public GameObject lookObject;
    private PhysicsObject physicsObject;
    private Camera mainCamera;

    [Header("Pickup")]
    [SerializeField] private Transform pickupParent;
    public GameObject pickupParentLook;
    public GameObject currentlyPickedUpObject;
    private Rigidbody pickupRB;
    public bool freeze;

    [Header("ObjectFollow")]
    [SerializeField] private float minSpeed = 0;
    [SerializeField] private float maxSpeed = 300f;
    [SerializeField] private float maxDistance = 10f;
    private float currentSpeed = 0f;
    private float currentDist = 0f;

    [Header("Rotation")]
    public float rotationSpeed = 100f;
    Quaternion lookRot;
    public Vector2 holdingRotation;
    RaycastHit hit;
    Vector3 fixedRot;

    //Const
    const int pickupLayer = 10;
    const int currentlyPickingUpLayer = 11;

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hit.point, sphereCastRadius);
    }

    //Interactable Object detections and distance check
    void Update()
    {
        //Here we check if we're currently looking at an interactable object
        raycastPos = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.SphereCast(raycastPos, sphereCastRadius, mainCamera.transform.forward, out hit, maxDistance, layers))
        {
            lookObject = hit.collider.transform.root.gameObject;
            if (lookObject.layer != 9 && lookObject.transform.childCount > 0)
            {
                lookObject = lookObject.transform.GetChild(0).gameObject;
            }
        }
        else
        {
            lookObject = null;
        }

        if (currentlyPickedUpObject != null)
        {
            if (Input.GetMouseButton(0))
            {
                holdingRotation.x += Input.GetAxis("Mouse X") * 1.5f;
                holdingRotation.y += Input.GetAxis("Mouse Y") * -1.5f;
                fixedRot = new Vector3(holdingRotation.y, holdingRotation.x, 0);
            }
        }

        //if we press the button of choice
        if (Input.GetMouseButtonDown(1))
        {
            //and we're not holding anything
            if (currentlyPickedUpObject == null)
            {
                //and we are looking an interactable object
                if (lookObject != null)
                {
                    PickUpObject();
                }

            }
            //if we press the pickup button and have something, we drop it
            else
            {
                BreakConnection();
            }
        }


    }

    //Velocity movement toward pickup parent and rotation
    private void FixedUpdate()
    {
        if (currentlyPickedUpObject != null)
        {
            currentDist = Vector3.Distance(pickupParent.position, pickupRB.position);
            currentSpeed = Mathf.SmoothStep(minSpeed, maxSpeed, currentDist / maxDistance);
            currentSpeed *= Time.fixedDeltaTime;
            Vector3 direction = pickupParent.position - pickupRB.position;
            pickupRB.velocity = direction.normalized * currentSpeed;
            //Rotation
            //lookRot = Quaternion.LookRotation(pickupParentLook.transform.position - pickupRB.position);
            //lookRot = Quaternion.Slerp(mainCamera.transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
            lookRot = Quaternion.Euler(fixedRot);
            pickupRB.MoveRotation(lookRot);
        }

    }

    private void OnDisable()
    {
        BreakConnection();
    }

    //Release the object
    public void BreakConnection()
    {
        if (pickupRB != null)
        {
            pickupRB.constraints = RigidbodyConstraints.None;
        }
        currentlyPickedUpObject = null;
        if (physicsObject != null)
        {
            physicsObject.pickedUp = false;
        }
        currentDist = 0;
        //Freeze
        if (pickupRB != null)
        {
            Component[] rigids = pickupRB.GetComponentsInChildren(typeof(Rigidbody), true);
            foreach (Rigidbody rig in rigids)
            {
                if (freeze)
                {
                    rig.isKinematic = true;
                }
                else
                {
                    rig.isKinematic = false;
                }
            }
            Rigidbody rigid = pickupRB.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                if (freeze)
                {
                    rigid.isKinematic = true;
                }
                else
                {
                    rigid.isKinematic = false;
                }
            }
            if (lookObject.layer == currentlyPickingUpLayer)
            {
                //Layer
                pickupRB.gameObject.layer = pickupLayer;
                Component[] transforms = pickupRB.GetComponentsInChildren(typeof(Transform), true);
                foreach (Transform transrights in transforms)
                {
                    transrights.gameObject.layer = pickupLayer;
                }
            }
        }
    }

    public void PickUpObject()
    {
        if (lookObject.layer == pickupLayer || lookObject.layer == currentlyPickingUpLayer)
        {
            lookObject = lookObject.transform.root.gameObject;
            holdingRotation = lookObject.transform.eulerAngles;
            physicsObject = lookObject.GetComponentInChildren<PhysicsObject>();
            currentlyPickedUpObject = lookObject;
            pickupRB = currentlyPickedUpObject.GetComponent<Rigidbody>();
            if (pickupRB == null)
            {
                pickupRB = currentlyPickedUpObject.GetComponentInChildren<Rigidbody>();
            }
            pickupRB.constraints = RigidbodyConstraints.FreezeRotation;
            if (physicsObject != null)
            {
                physicsObject.playerInteractions = this;
                StartCoroutine(physicsObject.PickUp());
            }
            pickupParent.transform.localEulerAngles = Vector3.zero;
            //Freeze
            Component[] rigids = pickupRB.GetComponentsInChildren(typeof(Rigidbody), true);
            foreach (Rigidbody rig in rigids)
            {
                rig.isKinematic = false;
            }
            Rigidbody rigid = pickupRB.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                rigid.isKinematic = false;
            }
            if (lookObject.layer == pickupLayer)
            {
                //Layer
                pickupRB.gameObject.layer = currentlyPickingUpLayer;
                Component[] transforms = pickupRB.GetComponentsInChildren(typeof(Transform), true);
                foreach (Transform transrights in transforms)
                {
                    transrights.gameObject.layer = currentlyPickingUpLayer;
                }
            }
        }
    }

}