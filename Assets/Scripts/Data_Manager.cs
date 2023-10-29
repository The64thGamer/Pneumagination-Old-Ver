using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Manager : MonoBehaviour
{
    [SerializeField] [Range(0,1)] float rainPercent = 0;
    [SerializeField][Range(0, 1)] float snowPercent = 0;
    [SerializeField] float worldFlyingSphereSize;
    [SerializeField] MapData mapData;

    public float GetCurrentRainValue()
    {
        return rainPercent;
    }

    public float GetCurrentSnowValue()
    {
        return snowPercent;
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


[System.Serializable]
public struct MapData
{
    public List<MapHistory> mapHistory;
    public List<CustomGeometryData> geometryData;
}

[System.Serializable]
public struct MapHistory
{
    public string change;
    public UDateTime time;
    public HistoricalEvent eventType;

    public enum HistoricalEvent
    {
        locationNameChange,
        locationOwnerChange,
        buildingChange,
        closure,
        reopening,
    }
}

[System.Serializable]
public struct CustomGeometryData
{
    public string key;
    public int material;
    public Color color;
    public float grime;
}