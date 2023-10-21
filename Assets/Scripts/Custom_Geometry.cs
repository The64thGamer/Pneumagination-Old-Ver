using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Custom_Geometry : MonoBehaviour
{
    [SerializeField] MaterialType materialType;
    [SerializeField] MaterialLocation materialLocation;
    [SerializeField][Range(0, 1)] float grime;
    float rain;
    float snow;
    float oldGrime = -1;
    float oldRain = -1;
    float oldSnow = -1;

    MeshRenderer meshRenderer;
    Data_Manager dataManager;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        dataManager = GameObject.Find("Data Manager").GetComponent<Data_Manager>();
    }

    private void Update()
    {
        if (materialLocation == MaterialLocation.exterior)
        {
            rain = dataManager.GetCurrentRainValue();
            snow = dataManager.GetCurrentSnowValue();
            if (oldRain != rain)
            {
                oldRain = rain;
                meshRenderer.material.SetFloat("_Rain", rain);
            }
            if (oldSnow != snow)
            {
                oldSnow = snow;
                meshRenderer.material.SetFloat("_Snow", snow);
            }
        }

        if (oldGrime != grime)
        {
            oldGrime = grime;
            meshRenderer.material.SetFloat("_Grime", grime);
        }
    }

    public enum MaterialType
    {
        wallpaper,
        bricks,
        slantedRoof,
        flooring,
        road,
        sidewalk,
        ceiling,
        concrete,
        plaster,
        plastic,
        earth,
    }
    public enum MaterialLocation
    {
        exterior,
        interior,
    }
}
