using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Manager : MonoBehaviour
{
    [SerializeField] [Range(0,1)] float rainPercent = 0;
    [SerializeField][Range(0, 1)] float snowPercent = 0;
    [SerializeField] Vector3 worldFlyingPosition;
    [SerializeField] float worldFlyingSphereSize;

    public float GetCurrentRainValue()
    {
        return rainPercent;
    }

    public float GetCurrentSnowValue()
    {
        return snowPercent;
    }

    public Vector3 GetWorldFlyingPosition()
    {
        return worldFlyingPosition;
    }

    public float GetWorldFlyingSphereSize()
    {
        return worldFlyingSphereSize;
    }
}

[System.Serializable]
public struct DateTimeFloat
{
    public UDateTime time;
    public float value;
}