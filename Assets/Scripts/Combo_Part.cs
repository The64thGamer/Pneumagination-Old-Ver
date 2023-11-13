using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo_Part : MonoBehaviour
{
    public List<BendablePart> bendableParts;
    public ComboTag partTag;

    public enum ComboTag
    {
        none,
        body,
        leftArm,
        rightArm,
        head,
    }
}
[System.Serializable]
public class BendablePart
{
    public string bendName;
    public string boneName;
    public Quaternion minPosition;
    public Quaternion maxPosition;
}

[System.Serializable]
public class Combo_Part_SaveFile
{
    public uint id;
    public List<Combo_BendablePart_SaveFile> bendableSections;
}
[System.Serializable]
public class Combo_BendablePart_SaveFile
{
    public string uniqueBendName;
    [Range(0, 1)]
    public float bendValue;
}