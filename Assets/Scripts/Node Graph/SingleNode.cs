using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NodePickup))]
[RequireComponent(typeof(BoxCollider))]
public class SingleNode : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text options;
    [SerializeField] SkinnedMeshRenderer skin;

    [SerializeField] AnimationCurve nailIn;
    [SerializeField] AnimationCurve smoothCurve;
    [SerializeField] GameObject ringTerminal;
    [SerializeField] GameObject screwTerminal;

    Canvas canvas;
    BoxCollider box;
    NodePickup pickup;

    float dragTimerB;
    int optionCount;

    const float nailUpSpeed = 3;
    const float nailDownSpeed = 2;
    const float spacing = -2.89f;
    const float evenLineNumberSpacingFix = 5f;
    Vector3 constTerminalStartPos = new Vector3(-1, -7, 0);

    void Start()
    {
        canvas = this.GetComponentInChildren<Canvas>();
        pickup = this.GetComponent<NodePickup>();
        box = this.GetComponent<BoxCollider>();
        UpdateName(name);
        for (int i = 0; i < Random.Range(0, 20); i++)
        {
            AddOption("Option #" + i, Convert.ToBoolean(Random.Range(0, 2)), Convert.ToBoolean(Random.Range(0, 2)));
        }
    }
    private void LateUpdate()
    {
        if (pickup.IsDragging())
        {
            dragTimerB = Mathf.Min(1, dragTimerB + (Time.deltaTime * nailUpSpeed));
            skin.SetBlendShapeWeight(2, smoothCurve.Evaluate(dragTimerB) * 100);
        }
        else
        {
            dragTimerB = Mathf.Max(0, dragTimerB - (Time.deltaTime * nailDownSpeed));
            skin.SetBlendShapeWeight(2, nailIn.Evaluate(dragTimerB) * 100);
        }
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
        float y = ((label.mesh.bounds.size.y + options.mesh.bounds.size.y + (optionCount % 2 == 0 && optionCount != 0 ? evenLineNumberSpacingFix : 0)) / 10.0f) + 0.25f;

        skin.SetBlendShapeWeight(0, x);
        skin.SetBlendShapeWeight(1, y);
        box.size = new Vector3(x / 100.0f, y / 100.0f, 0.003f);
        box.center = new Vector3(-x / 200.0f, -y / 200.0f, 0);
    }


    public void AddOption(string name, bool input, bool output)
    {
        options.text += name + "\n";
        if (input)
        {
            GameObject inputGameObject = GameObject.Instantiate(ringTerminal, canvas.transform);
            inputGameObject.transform.localPosition = constTerminalStartPos + new Vector3(0, spacing * optionCount);
        }
        if (output)
        {
            GameObject inputGameObject = GameObject.Instantiate(screwTerminal, canvas.transform);
            inputGameObject.transform.localPosition = constTerminalStartPos + new Vector3(Mathf.Max(label.mesh.bounds.size.x, options.mesh.bounds.size.x) + 5f, spacing * optionCount, -1.25f);
        }
        optionCount++;
        UpdateSize();
    }
}