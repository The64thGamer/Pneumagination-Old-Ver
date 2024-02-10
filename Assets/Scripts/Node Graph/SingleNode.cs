using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class SingleNode : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text options;
    [SerializeField] SkinnedMeshRenderer skin;
    [SerializeField] BoxCollider box;
    [SerializeField] AnimationCurve smoothCurve;
    [SerializeField] AnimationCurve nailIn;
    [SerializeField] GameObject ringTerminal;
    [SerializeField] GameObject screwTerminal;

    Canvas canvas;

    Vector2 pickupOffset;
    bool dragging;
    float dragTimer;
    float dragTimerB;
    int optionCount;

    const float nailUpSpeed = 3;
    const float nailDownSpeed = 2;
    const float boxSpeed = 10;
    const float spacing = -2.89f;
    Vector3 constTerminalStartPos = new Vector3(-1,-7,0);

    void Start()
    {
        UpdateName(name);
        canvas = this.GetComponentInChildren<Canvas>();
        for (int i = 0; i < Random.Range(0, 20); i++)
        {
            AddOption("Option #" + i, Convert.ToBoolean(Random.Range(0, 2)), Convert.ToBoolean(Random.Range(0, 2)));

        }
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
        UpdateSize();
    }

    void UpdateSize()
    {
        options.ForceMeshUpdate();
        label.ForceMeshUpdate();

        float x = ((Mathf.Max(label.mesh.bounds.size.x, options.mesh.bounds.size.x)) / 10.0f) + 0.3f;
        float y = ((label.mesh.bounds.size.y + options.mesh.bounds.size.y) / 10.0f) + 0.3f;

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

    public void AddOption(string name, bool input, bool output)
    {
        options.text += name + "\n";
        UpdateSize();
        if(input)
        {
            GameObject inputGameObject = GameObject.Instantiate(ringTerminal,canvas.transform);
            inputGameObject.transform.localPosition = constTerminalStartPos + new Vector3(0,spacing * optionCount);
        }
        if (output)
        {
            GameObject inputGameObject = GameObject.Instantiate(screwTerminal, canvas.transform);
            inputGameObject.transform.localPosition = constTerminalStartPos + new Vector3(Mathf.Max(label.mesh.bounds.size.x, options.mesh.bounds.size.x) + 5f, spacing * optionCount,-1.25f);
        }
        optionCount++;
    }
}