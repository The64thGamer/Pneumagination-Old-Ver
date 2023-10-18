using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Manager : MonoBehaviour
{
    [SerializeField] [Range(0,1)] float rainPercent = 0;
    [SerializeField][Range(0, 1)] float snowPercent = 0;

    public float GetCurrentRainValue()
    {
        return rainPercent;
    }

    public float GetCurrentSnowValue()
    {
        return snowPercent;
    }
}

[System.Serializable]
public struct DateTimeFloat
{
    public UDateTime time;
    public float value;
}