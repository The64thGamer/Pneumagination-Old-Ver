using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeGraph : MonoBehaviour
{
    [SerializeField] Transform backingPlane;
    [SerializeField] Camera mainCam;
    
    int zoomLevel = 10;
    Vector2 screenRes;

    const int minZoomLevel = 5;
    const int maxZoomLevel = 100;

    void Awake()
    {
        screenRes = new Vector2(Screen.width, Screen.height);
        UpdateBackingPlane();
    }

    void Update()
    {
        if (screenRes.x != Screen.width || screenRes.y != Screen.height)
        {
            screenRes.x = Screen.width;
            screenRes.y = Screen.height;
            UpdateBackingPlane();
        }

        if(Input.mousePositionDelta != Vector3.zero && Input.GetMouseButton(2))
        {
            mainCam.transform.position -= Input.mousePositionDelta * zoomLevel / Mathf.Min(screenRes.x, screenRes.y);
            UpdateBackingPlane();
        }

        if(Input.mouseScrollDelta != Vector2.zero)
        {
            zoomLevel = Mathf.Clamp(zoomLevel - (int)Input.mouseScrollDelta.y*5,minZoomLevel,maxZoomLevel);
            UpdateBackingPlane();
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit))
            {
                SingleNode node = hit.collider.GetComponent<SingleNode>();
                if(node != null)
                {
                    node.PickUp(hit.point - hit.collider.transform.position);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                ScrewTerminal terminal = hit.collider.GetComponent<ScrewTerminal>();
                if (terminal != null)
                {
                    terminal.StartConnection();
                }
            }
        }
    }

    void UpdateBackingPlane()
    {
        mainCam.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, -zoomLevel);
        float pos = (mainCam.nearClipPlane + zoomLevel);
        backingPlane.position = mainCam.transform.position + (mainCam.transform.forward * pos);
        float h = Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;
        backingPlane.localScale = new Vector3(h * mainCam.aspect, h, 1f);
    }
}
