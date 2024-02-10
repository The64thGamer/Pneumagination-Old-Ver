using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SingleNode : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] SkinnedMeshRenderer skin;

    void Start()
    {
        UpdateName(name);
        UpdateSize(2, Random.Range(2,6));
    }

    void UpdateName(string newName)
    {
        label.text = newName;
    }

    void UpdateSize(int x, int y)
    {
        skin.SetBlendShapeWeight(0, x);
        skin.SetBlendShapeWeight(1, y);
    }

}
