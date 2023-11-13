using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo_Creator : MonoBehaviour
{
    [SerializeField] Creator_Company[] companies;

    int currentCompany;

    private void Start()
    {
        currentCompany = PlayerPrefs.GetInt("Game: Current Company");
    }

    void UpdateBodies()
    {

    }
}


[System.Serializable]
public class Creator_Company
{
    public Creator_Part[] parts;
}

[System.Serializable]
public class Creator_Part
{
    public uint partId;
    public Creator_Part[] childIds;
}