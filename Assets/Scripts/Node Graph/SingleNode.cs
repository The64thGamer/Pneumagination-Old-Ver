using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SingleNode : MonoBehaviour
{
    RenderTexture rt;
    TMP_Text label;
    void Start()
    {
        UpdateName(name);
    }

    void UpdateName(string newName)
    {
        label.text = newName;
    }

}
