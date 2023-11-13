using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo_Animatronic : MonoBehaviour
{
    [SerializeField] string playerGivenName;
    [SerializeField] UDateTime creationDate;
    [SerializeField] UDateTime lastCleanedDate;
    [SerializeField] List<uint> partIds;

    private void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        ClearAllParts();
        List<uint> tempPartIds = partIds;

        //First find the root body item
        for (int i = 0; i < tempPartIds.Count; i++)
        {
            GameObject g = Resources.Load<GameObject>("Animatronics/Prefabs/" + tempPartIds[i]);
            if (g.GetComponent<Combo_Part>().partTag == Combo_Part.ComboTag.body)
            {
                g = GameObject.Instantiate(g);
                g.transform.parent = transform;
                g.name = tempPartIds[i].ToString();
                tempPartIds.RemoveAt(i);
                break;
            }
        }
    }

    public void SaveData()
    {

    }

    void ClearAllParts()
    {
        foreach (GameObject child in transform)
        {
            Destroy(child);
        }
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