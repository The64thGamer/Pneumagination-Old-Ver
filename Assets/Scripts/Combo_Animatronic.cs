using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo_Animatronic : MonoBehaviour
{
    public string playerGivenName;
    public UDateTime creationDate;
    public UDateTime lastCleanedDate;

    public void LoadData()
    {

    }

    public void SaveData()
    {

    }


}

[System.Serializable]
public class Combo_Animatronic_SaveFile
{
    public string playerGivenName;
    public UDateTime creationDate;
    public UDateTime lastCleanedDate;
    public Vector3 position;
    public Quaternion rotation;
    public List<Combo_Part_SaveFile> comboParts;
}