using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(BoxCollider))]
public class NodePickup : MonoBehaviour
{
    BoxCollider box;
    [SerializeField] AnimationCurve smoothCurve;
    Vector2 pickupOffset;
    float dragTimer;
    bool dragging;
    const float boxSpeed = 10;
    Camera mainCam;

    private void Start()
    {
        box = this.GetComponent<BoxCollider>();
        mainCam = transform.parent.parent.GetComponentInChildren<Camera>();
    }
    private void LateUpdate()
    {
        if (dragging)
        {
            dragTimer = Mathf.Min(1, dragTimer + (Time.deltaTime * boxSpeed));

            RaycastHit hit;
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, 10000, LayerMask.GetMask("NodeUI")))
            {
                transform.position = new Vector3(hit.point.x - pickupOffset.x, hit.point.y - pickupOffset.y, transform.position.z);
            }
            if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
            }

        }
        else
        {
            box.enabled = true;
            dragTimer = Mathf.Max(0, dragTimer - (Time.deltaTime * boxSpeed));
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(0.3f, -0.4f, smoothCurve.Evaluate(dragTimer)));
    }
    public void PickUp(Vector2 offset)
    {
        box.enabled = false;
        pickupOffset = offset;
        dragging = true;
    }

    public bool IsDragging()
    {
        return dragging;
    }

    public Camera GetCamera()
    {
        if(mainCam == null)
        {
            mainCam = transform.parent.parent.GetComponentInChildren<Camera>();
        }
        return mainCam;
    }
}
