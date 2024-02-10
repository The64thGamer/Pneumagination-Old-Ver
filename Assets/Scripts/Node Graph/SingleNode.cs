using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SingleNode : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] SkinnedMeshRenderer skin;
    [SerializeField] BoxCollider box;
    [SerializeField] AnimationCurve smoothCurve;
    [SerializeField] AnimationCurve nailIn;

    Vector2 pickupOffset;
    bool dragging;
    float dragTimer;
    float dragTimerB;

    const float nailUpSpeed = 3;
    const float nailDownSpeed = 2;
    const float boxSpeed = 10;

    void Start()
    {
        UpdateName(name);
        UpdateSize(2, Random.Range(2, 6));
    }

    private void LateUpdate()
    {
        if (dragging)
        {
            dragTimer = Mathf.Min(1, dragTimer + (Time.deltaTime * nailUpSpeed));
            dragTimerB = Mathf.Min(1, dragTimerB + (Time.deltaTime * boxSpeed));
            skin.SetBlendShapeWeight(2, smoothCurve.Evaluate(dragTimer)*100);

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
            dragTimer = Mathf.Max(0, dragTimer - (Time.deltaTime * nailDownSpeed));
            dragTimerB = Mathf.Max(0, dragTimerB - (Time.deltaTime * boxSpeed)); 
            skin.SetBlendShapeWeight(2, nailIn.Evaluate(dragTimer)*100);
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(0.3f, -0.4f, smoothCurve.Evaluate(dragTimerB)));
    }

    void UpdateName(string newName)
    {
        label.text = newName;
    }

    void UpdateSize(int x, int y)
    {
        skin.SetBlendShapeWeight(0, x);
        skin.SetBlendShapeWeight(1, y);
        box.size = new Vector3(x / 100.0f, y / 100.0f, 0.003f);
        box.center = new Vector3(-x / 200.0f, -y / 200.0f, 0);
    }

    public void PickUp(Vector2 offset)
    {
        box.enabled = false;
        pickupOffset = offset;
        dragging = true;
    }

}
