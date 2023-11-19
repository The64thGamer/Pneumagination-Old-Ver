using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Combo_Animatronic : MonoBehaviour
{
    [SerializeField] List<DEAD_Animatronic> animatronicParts;
    [SerializeField] Combo_Animatronic_SaveFile saveFile = new Combo_Animatronic_SaveFile();
    [SerializeField] DEAD_Interface deadInterface;

    private void Start()
    {
        RefreshAnimatronic();
    }

    public Combo_Animatronic_SaveFile GetSaveFileData()
    {
        return saveFile;
    }

    public void InsertDeadInterface(DEAD_Interface inter)
    {
        deadInterface = inter;
    }

    public void ReassignPartsFromUI(List<UI_Part_Holder> tempParts)
    {
        saveFile.comboParts = new List<Combo_Part_SaveFile>();
        for (int i = 0; i < tempParts.Count; i++)
        {
            saveFile.comboParts.Add(new Combo_Part_SaveFile() { id = tempParts[i].id, bendableSections = new List<float>() });
        }
        RefreshAnimatronic();
    }

    public void ReassignFullSaveFile(Combo_Animatronic_SaveFile file)
    {
        saveFile = file;
        RefreshAnimatronic();
    }

    public void RefreshAnimatronicCustomizations()
    {
        Combo_Part[] parts = this.GetComponentsInChildren<Combo_Part>();
        for (int e = 0; e < parts.Length; e++)
        {
            int id = 0;
            for (int i = 0; i < saveFile.comboParts.Count; i++)
            {
                if (Convert.ToInt32(parts[e].name) == saveFile.comboParts[i].id)
                {
                    id = i;
                    break;
                }
            }
            for (int i = 0; i < parts[e].bendableParts.Count; i++)
            {
                parts[e].SetBend(i, saveFile.comboParts[id].bendableSections[i]);
            }

        }
    }

    public void RefreshAnimatronic()
    {
        ClearAllParts();
        List<uint> tempPartIds = new List<uint>();
        for (int i = 0; i < saveFile.comboParts.Count; i++)
        {
            tempPartIds.Add(saveFile.comboParts[i].id);
        }

        //First find the root body item
        for (int i = 0; i < tempPartIds.Count; i++)
        {
            GameObject g = Resources.Load<GameObject>("Animatronics/Prefabs/" + tempPartIds[i]);
            if (g.GetComponent<Combo_Part>().partTag == Combo_Part.ComboTag.body)
            {
                g = GameObject.Instantiate(g, transform);
                animatronicParts.Add(g.GetComponent<DEAD_Animatronic>());
                g.transform.localPosition = Vector3.zero;
                g.transform.localRotation = Quaternion.identity;
                g.name = tempPartIds[i].ToString();
                tempPartIds.RemoveAt(i);
                break;
            }
        }

        int freezePrevention = (tempPartIds.Count + 1) * 2;
        int index = 0;
        while (tempPartIds.Count > 0 && freezePrevention > 0)
        {
            GameObject g = Resources.Load<GameObject>("Animatronics/Prefabs/" + tempPartIds[index]);
            string boneName = g.GetComponent<Combo_Part>().connectingBone;
            for (int i = 0; i < animatronicParts.Count; i++)
            {
                Transform t = RecursiveFindChild(animatronicParts[i].transform, boneName);
                if (t != null)
                {
                    g = GameObject.Instantiate(g, t);
                    animatronicParts.Add(g.GetComponent<DEAD_Animatronic>());
                    g.transform.localRotation = Quaternion.identity;
                    g.transform.localPosition = Vector3.zero;
                    g.transform.localScale = Vector3.one / 100;
                    g.name = tempPartIds[i].ToString();

                    //Part Customization
                    Combo_Part c = g.GetComponent<Combo_Part>();
                    int partId = SearchForComboPartID(tempPartIds[index]);
                    for (int e = 0; e < c.bendableParts.Count; e++)
                    {
                        if (saveFile.comboParts[partId].bendableSections.Count - 1 < e)
                        {
                            saveFile.comboParts[partId].bendableSections.Add(0);
                        }
                        c.SetBend(e, saveFile.comboParts[partId].bendableSections[e]);
                    }

                    tempPartIds.RemoveAt(index);
                    break;
                }
            }
            if (tempPartIds.Count > 0)
            {
                index = (index++) % tempPartIds.Count;
            }
            freezePrevention--;
        }
        if (freezePrevention < 0)
        {
            Debug.LogError("Some part of the animatronic couldn't find its bone");
        }
        for (int i = 0; i < animatronicParts.Count; i++)
        {
            animatronicParts[i].SetInterface(deadInterface);
        }
    }

    public Combo_Part_SaveFile SearchID(uint id)
    {
        return saveFile.comboParts[SearchForComboPartID(id)];
    }

    int SearchForComboPartID(uint id)
    {
        for (int i = 0; i < saveFile.comboParts.Count; i++)
        {
            if (saveFile.comboParts[i].id == id)
            {
                return i;
            }
        }
        return 0;
    }

    Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    public string GetName()
    {
        return saveFile.playerGivenName;
    }

    public void SetName(string newName)
    {
        saveFile.playerGivenName = newName;
    }

    public int GetNumberOfMovements()
    {
        DEAD_Animatronic[] parts = this.GetComponentsInChildren<DEAD_Animatronic>();
        int totalMovements = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            totalMovements += parts[i].GetActuatorInfoCopy().Length;
        }
        return totalMovements;
    }

    void ClearAllParts()
    {
        animatronicParts = new List<DEAD_Animatronic>();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


}

[System.Serializable]
public class Combo_Animatronic_SaveFile
{
    public int objectHash;
    public bool yetToBePlaced;
    public string playerGivenName;
    public UDateTime creationDate;
    public UDateTime lastCleanedDate;
    public Vector3 position;
    public Quaternion rotation;
    public List<Combo_Part_SaveFile> comboParts;
}