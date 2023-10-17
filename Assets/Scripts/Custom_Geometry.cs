using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Custom_Geometry : MonoBehaviour
{
    [SerializeField] MaterialType materialType;
    [SerializeField] MaterialLocation materialLocation;
    [SerializeField][Range(0, 1)] float grime;
    [SerializeField][Range(0, 1)] float rain;
    [SerializeField][Range(0, 1)] float snow;

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
    }
    public enum MaterialLocation
    {
        exterior,
        interior,
    }
}
