using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.Rendering;

public class Combo_Animatronic : PneumagiNode
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

    public void ReassignFullSaveFile(Combo_Animatronic_SaveFile file)
    {
        saveFile = file;
        RefreshAnimatronic();
    }

    public void RefreshAnimatronicCustomizations()
    {
        Combo_Part[] parts = this.GetComponentsInChildren<Combo_Part>();
        DEAD_Animatronic an;
        for (int e = 0; e < parts.Length; e++)
        {
            int id = SearchForComboPartID(Convert.ToUInt32(parts[e].name));

            for (int i = 0; i < saveFile.comboParts[id].bendableSections.Count; i++)
            {
                parts[e].SetBend(i, saveFile.comboParts[id].bendableSections[i]);
            }
            an = parts[e].GetComponent<DEAD_Animatronic>();

            if (saveFile.comboParts[id].actuatorDTUIndexes != null)
            {
                int count = an.GetActuatorCount();
                for (int i = 0; i < count; i++)
                {
                    an.SetDTUIndex(i, saveFile.comboParts[id].actuatorDTUIndexes[i]);
                }
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
                SetUpNewPart(transform, g, tempPartIds, i, i, 1);
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
                    SetUpNewPart(t, g, tempPartIds, index, i, .01f);
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
            DEAD_Actuator[] actuators = animatronicParts[i].GetActuatorInfoCopy();

            for (int e = 0; e < actuators.Length; e++)
            {
                InputHolder holder = new InputHolder { inputID = actuators[e].actuationName };
                holder.inputListener.AddListener(animatronicParts[i].SetActuatorTargetValue);
                nodeInputs.Add(holder);
            }
        }
    }

    void SetUpNewPart(Transform t, GameObject g, List<uint> tempPartIds, int index, int i, float scale)
    {
        g = GameObject.Instantiate(g, t);
        animatronicParts.Add(g.GetComponent<DEAD_Animatronic>());
        g.transform.localRotation = Quaternion.identity;
        g.transform.localPosition = Vector3.zero;
        g.transform.localScale = Vector3.one * scale;
        g.name = tempPartIds[i].ToString();

        //Part Customization
        Combo_Part c = g.GetComponent<Combo_Part>();
        int id = SearchForComboPartID(tempPartIds[index]);
        for (int e = 0; e < saveFile.comboParts[id].bendableSections.Count; e++)
        {
            c.SetBend(e, saveFile.comboParts[id].bendableSections[e]);
        }

        DEAD_Animatronic an = g.GetComponent<DEAD_Animatronic>();

        if (saveFile.comboParts[id].actuatorDTUIndexes != null)
        {
            int count = an.GetActuatorCount();
            for (int j = 0; j < count; j++)
            {
                an.SetDTUIndex(j, saveFile.comboParts[id].actuatorDTUIndexes[j]);
            }
        }

        tempPartIds.RemoveAt(index);
    }

    public Combo_Part_SaveFile SearchSaveFileID(uint id)
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