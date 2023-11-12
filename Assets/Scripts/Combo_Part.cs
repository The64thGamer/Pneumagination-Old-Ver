using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo_Part : MonoBehaviour
{
    public string resourcesPath;
    public List<BendablePart> bendableParts;
    public ComboTag partTag;
    public ComboTag parentTag;
    public List<ComboTag> childTags;

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
    [Tooltip("Make name unique from other bendable parts this can be used with. This name is tagged in save files with the corresponding bend value.")]
    public string uniqueBendName;
    public string boneName;
    public Quaternion minPosition;
    public Quaternion maxPosition;
}

[System.Serializable]
public class Combo_Part_SaveFile
{
    public string resourcesPath;
    public List<Combo_BendablePart_SaveFile> bendableParts;
}
[System.Serializable]
public class Combo_BendablePart_SaveFile
{
    public string uniqueBendName;
    [Range(0, 1)]
    public float bendValue;
}