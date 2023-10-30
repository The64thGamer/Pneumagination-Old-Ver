using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Custom_Geometry : MonoBehaviour
{
    [Header("Static Data")]
    [SerializeField] MaterialType materialType;
    [SerializeField] MaterialLocation materialLocation;
    [SerializeField] MaterialColorType materialColorType;

    [Header("Game Decided Data")]
    [SerializeField][Range(0, 1)] float grime;

    [Header("Player Decided Data")]
    [SerializeField] int materialNumber;
    [SerializeField] Color color = Color.white; 
    float rain;
    float snow;
    float oldGrime = -1;
    float oldRain = -1;
    float oldSnow = -1;
    Color oldColor;

    MeshRenderer meshRenderer;
    Data_Manager dataManager;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        dataManager = GameObject.Find("Data Manager").GetComponent<Data_Manager>();
    }

    public string GetKey()
    {
        return name;
    }

    public Color GetColor()
    {
        return color;
    }
    public void SetGrime(float value)
    {
        grime = value;
    }
    public void SetMaterial(int number)
    {
        materialNumber = number;
    }

    public int GetMaterial()
    {
        return materialNumber;
    }

    public MaterialColorType GetMaterialColorType()
    {
        return materialColorType;
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
    }

    private void Update()
    {
        if(oldColor != color)
        {
            oldColor = color;
            meshRenderer.material.SetColor("_Color", color);
        }

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
        ceiling,
        concrete,
        plaster,
        plastic,
        earth,
        vinyl,
        metal,
    }
    public enum MaterialLocation
    {
        exterior,
        interior,
    }

    public enum MaterialColorType
    {
        primary,
        secondary,
        tertiary,
        light,
        dark,
    }
}
