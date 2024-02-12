using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NodePickup : MonoBehaviour
{
    [SerializeField] protected BoxCollider box;
    [SerializeField] protected AnimationCurve smoothCurve;
    Vector2 pickupOffset;
    protected float dragTimer;
    protected bool dragging;
    protected const float boxSpeed = 10;

    private void LateUpdate()
    {
        if (dragging)
        {
            dragTimer = Mathf.Min(1, dragTimer + (Time.deltaTime * boxSpeed));

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
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
}
