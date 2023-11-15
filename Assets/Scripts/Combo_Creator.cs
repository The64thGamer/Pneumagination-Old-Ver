using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Combo_Creator : MonoBehaviour
{
    [SerializeField] Creator_Company[] companies;
    [Header("Menu")]
    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset comboBox;
    [SerializeField] VisualTreeAsset comboButton;
    [SerializeField] SaveFileData saveFileData;
    [SerializeField] List<UI_Part_Holder> tempParts = new List<UI_Part_Holder>();
    [SerializeField] Combo_Animatronic comboAnimatronic;

    //TotalCost
    //TotalMoney
    //BackButton

    int currentCompany;
    bool inPreview = false;

    private void Start()
    {
        document.rootVisualElement.Q<VisualElement>("ItemPreview").style.opacity = 0;

        currentCompany = PlayerPrefs.GetInt("Game: Current Company");
        SwitchMenu(0);
    }

    void SwitchMenu(int menu)
    {
        VisualElement visList = document.rootVisualElement.Q<VisualElement>("ScrollBox");

        //Clear old children
        List<VisualElement> children = new List<VisualElement>();
        foreach (var child in visList.Children())
        {
            children.Add(child);
        }
        for (int i = 0; i < children.Count; i++)
        {
            visList.Remove(children[i]);
        }
        List<Creator_Part> parts = null;
        Combo_Part.ComboTag tag = Combo_Part.ComboTag.none;
        switch (menu)
        {
            case 0:
                parts = GetPartsOfTag(Combo_Part.ComboTag.body);
                tag = Combo_Part.ComboTag.body;
                break;
            default:
                break;
        }
        for (int i = 0; i < parts.Count; i++)
        {
            Combo_Part combo = Resources.Load<GameObject>("Animatronics/Prefabs/" + companies[currentCompany].parts[i].partId).GetComponent<Combo_Part>();
            uint price = combo.price;
            string comboName = combo.partName;
            Texture2D tex = Resources.Load<Texture2D>("Animatronics/Icons/" + companies[currentCompany].parts[i].partId);

            VisualElement currentButton = comboButton.Instantiate();
            currentButton.Q<VisualElement>("Icon").style.backgroundImage = tex;
            currentButton.Q<Label>("Price").text = "$" + price.ToString();
            Button visButton = currentButton.Q<Button>("Button");
            uint partID = parts[i].partId;
            visButton.clicked += () => AddPart(tag, partID);
            visButton.RegisterCallback<MouseOverEvent>((type) =>
            {
                visButton.style.scale = Vector2.one * 0.95f;
                document.rootVisualElement.Q<VisualElement>("ItemPreview").style.opacity = 1;
                FakeAddPart(tag, partID);
                document.rootVisualElement.Q<Label>("ItemPrice").text = "$" + price.ToString();
                document.rootVisualElement.Q<VisualElement>("PreviewIcon").style.backgroundImage = tex;

                string movements = "";

                DEAD_Actuator[] actuators = combo.GetComponent<DEAD_Animatronic>().GetActuatorInfoCopy();

                document.rootVisualElement.Q<Label>("ItemName").text = combo.partName;

                for (int i = 0; i < actuators.Length; i++)
                {
                    movements += actuators[i].actuationName + "\n";
                }

                document.rootVisualElement.Q<Label>("Movements").text = movements;


            });
            visButton.RegisterCallback<MouseOutEvent>((type) =>
            {
                visButton.style.scale = Vector2.one;
                document.rootVisualElement.Q<VisualElement>("ItemPreview").style.opacity = 0;
                FakeRemovePart();
            });
            visList.Add(currentButton);
        }
    }


    void FakeAddPart(Combo_Part.ComboTag tag, uint id)
    {
        List<UI_Part_Holder> fakeparts = new List<UI_Part_Holder>(tempParts);
        for (int i = 0; i < fakeparts.Count; i++)
        {
            if (fakeparts[i].tag == tag)
            {
                fakeparts.RemoveAt(i);
            }
        }
        fakeparts.Add(new UI_Part_Holder() { id = id, tag = tag });
        if (!fakeparts.All(tempParts.Contains))
        {
            comboAnimatronic.ReassignPartsFromUI(fakeparts);
            inPreview = true;
        }
    }

    void FakeRemovePart()
    {
        if (inPreview)
        {
            comboAnimatronic.ReassignPartsFromUI(tempParts);
        }
    }

    List<Creator_Part> GetPartsOfTag(Combo_Part.ComboTag tag)
    {
        List<Creator_Part> parts = new List<Creator_Part>();

        for (int i = 0; i < companies[currentCompany].parts.Length; i++)
        {
            Combo_Part combo = Resources.Load<GameObject>("Animatronics/Prefabs/" + companies[currentCompany].parts[i].partId).GetComponent<Combo_Part>();
            if (combo.partTag == tag)
            {
                parts.Add(companies[currentCompany].parts[i]);
            }
        }

        return parts;
    }

    void AddPart(Combo_Part.ComboTag tag, uint id)
    {
        for (int i = 0; i < tempParts.Count; i++)
        {
            if (tempParts[i].tag == tag)
            {
                tempParts.RemoveAt(i);
            }
        }
        tempParts.Add(new UI_Part_Holder() { id = id, tag = tag });
        comboAnimatronic.ReassignPartsFromUI(tempParts);
    }
}


[System.Serializable]
public class UI_Part_Holder
{
    public Combo_Part.ComboTag tag;
    public uint id;
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