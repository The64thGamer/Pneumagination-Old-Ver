using NUnit.Framework;
using System;
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
    int currentMenu;

    private void Start()
    {
        document.rootVisualElement.Q<VisualElement>("ItemPreview").style.opacity = 0;

        document.rootVisualElement.Q<Button>("CategoryLeft").clicked += () => AddMenu(-1, true);
        document.rootVisualElement.Q<Button>("CategoryRight").clicked += () => AddMenu(1, false);


        currentCompany = PlayerPrefs.GetInt("Game: Current Company");
        SwitchMenu(0,false);

    }

    void AddMenu(int add, bool iterateBackward)
    {
        SwitchMenu(currentMenu + add, iterateBackward);
    }

    void SwitchMenu(int menu, bool iterateBackward)
    {
        currentMenu = menu;
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
        bool check = false;
        while (!check)
        {
            switch (currentMenu)
            {
                case 0:
                    parts = GetPartsOfTag(Combo_Part.ComboTag.body);
                    tag = Combo_Part.ComboTag.body;
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Choose Body";
                    break;
                case 1:
                    parts = GetPartsOfTag(Combo_Part.ComboTag.rightArm);
                    tag = Combo_Part.ComboTag.rightArm;
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Choose Rt. Arm";
                    break;
                default:
                    break;
            }
            if (parts != null && parts.Count > 0)
            {
                check = true;
            }
            else
            {
                if (iterateBackward)
                {
                    currentMenu--;
                    if (currentMenu < 0)
                    {
                        currentMenu = 0;
                    }
                }
                else
                {
                    currentMenu++;
                    //100 just an arbitrary number
                    if (currentMenu > 100)
                    {
                        currentMenu = 0;
                    }
                }
            }
        }

        if(currentMenu == 0)
        {
            document.rootVisualElement.Q<Button>("CategoryLeft").style.visibility = Visibility.Hidden;
            if(tempParts.Count == 0)
            {
                document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Hidden;

            }
        }
        else
        {
            document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Visible;
            document.rootVisualElement.Q<Button>("CategoryLeft").style.visibility = Visibility.Visible;
        }


        for (int i = 0; i < parts.Count; i++)
        {
            Combo_Part combo = Resources.Load<GameObject>("Animatronics/Prefabs/" + parts[i].partId).GetComponent<Combo_Part>();
            uint price = combo.price;
            string comboName = combo.partName;
            Texture2D tex = Resources.Load<Texture2D>("Animatronics/Icons/" + parts[i].partId);

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
        tempParts.Sort();
        fakeparts.Sort();
        if (!tempParts.SequenceEqual(fakeparts))
        {
            comboAnimatronic.ReassignPartsFromUI(fakeparts);
            inPreview = true;
        }
    }

    void FakeRemovePart()
    {
        if (inPreview)
        {
            inPreview = false;
            comboAnimatronic.ReassignPartsFromUI(tempParts);
        }
    }

    List<Creator_Part> GetPartsOfTag(Combo_Part.ComboTag tag)
    {
        List<Creator_Part> parts = new List<Creator_Part>();

        if (tag == Combo_Part.ComboTag.body)
        {
            for (int i = 0; i < companies[currentCompany].parts.Length; i++)
            {
                Combo_Part combo = Resources.Load<GameObject>("Animatronics/Prefabs/" + companies[currentCompany].parts[i].partId).GetComponent<Combo_Part>();
                if (combo.partTag == tag)
                {
                    parts.Add(companies[currentCompany].parts[i]);
                }
            }
        }
        else
        {
            List<Creator_Part> bodyParts = GetPartsOfTag(Combo_Part.ComboTag.body);

            //Find used body part
            for (int i = 0; i < bodyParts.Count; i++)
            {
                for (int e = 0; e < tempParts.Count; e++)
                {
                    if (bodyParts[i].partId == tempParts[e].id)
                    {
                        parts = RecursiveFindParts(bodyParts[i],tag);
                        return parts;
                    }
                }
            }
        }
        return parts;
    }

    List<Creator_Part> RecursiveFindParts(Creator_Part part, Combo_Part.ComboTag tag)
    {
        List<Creator_Part> finalParts = new List<Creator_Part>();

        Combo_Part combo = Resources.Load<GameObject>("Animatronics/Prefabs/" + part.partId).GetComponent<Combo_Part>();
        if (combo.partTag == tag)
        {
            finalParts.Add(part);
        }

        if (part.childIds == null || part.childIds.Length == 0)
        {
            return finalParts;
        }
        for (int i = 0; i < part.childIds.Length; i++)
        {
            List<Creator_Part> childParts = RecursiveFindParts(part.childIds[i],tag);
            if (childParts != null)
            {
                finalParts.AddRange(childParts);
            }
        }
        return finalParts;
    }

    void AddPart(Combo_Part.ComboTag tag, uint id)
    {
        List<UI_Part_Holder> fakeparts = new List<UI_Part_Holder>(tempParts);

        for (int i = 0; i < tempParts.Count; i++)
        {
            if (tempParts[i].tag == tag)
            {
                tempParts.RemoveAt(i);
            }
        }
        tempParts.Add(new UI_Part_Holder() { id = id, tag = tag });


        tempParts.Sort();
        fakeparts.Sort();
        if (!tempParts.SequenceEqual(fakeparts))
        {
            inPreview = false;
            comboAnimatronic.ReassignPartsFromUI(tempParts);
        }

        if (currentMenu == 0)
        {
            if (tempParts.Count != 0)
            {
                document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Visible;
            }
            else
            {
                document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Hidden;

            }
        }
    }
}


[System.Serializable]
public class UI_Part_Holder
{
    public Combo_Part.ComboTag tag;
    public uint id;
    public override bool Equals(object obj)
    {
        return Equals(obj as UI_Part_Holder);
    }
    public bool Equals(UI_Part_Holder other)
    {
        if (other == null)
            return false;

        // Check for equality based on specific properties
        return id == other.id;
    }
    public override int GetHashCode()
    {
        // Generate a hash code based on the properties used in Equals method
        return HashCode.Combine(id);
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