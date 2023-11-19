using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Combo_Creator : MonoBehaviour
{
    [SerializeField] Creator_Company[] companies;
    [Header("Menu")]
    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset comboButton;
    [SerializeField] VisualTreeAsset comboSlider;
    [SerializeField] VisualTreeAsset comboPurchase;
    [SerializeField] SaveFileData saveFileData;
    [SerializeField] MapData mapData;

    [SerializeField] List<UI_Part_Holder> tempParts = new List<UI_Part_Holder>();
    [SerializeField] Combo_Animatronic comboAnimatronic;

    int currentCompany;
    bool inPreview = false;
    int currentMenu;

    //Consts
    const int maxPossibleAnimatronicMovements = 20;

    private void Start()
    {
        string saveFilePath = Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml";

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("You entered the creator without a save file");
            SceneManager.LoadScene(0);
        }
        else
        {
            saveFileData = saveFileData.DeserializeFromXML(File.ReadAllText(saveFilePath));
        }

        if (saveFileData == null)
        {
            Debug.LogError("You entered a map with a corrupt save file");
            SceneManager.LoadScene(0);
        }

        string mapSavePath = Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml";
        if (!File.Exists(mapSavePath))
        {
            Debug.LogError("You entered the creator without a destination map save file");
        }
        else
        {
            mapData = mapData.DeserializeFromXML(File.ReadAllText(mapSavePath));
        }

        document.rootVisualElement.Q<VisualElement>("ItemPreview").style.opacity = 0;

        document.rootVisualElement.Q<Button>("CategoryLeft").clicked += () => AddMenu(-1, true);
        document.rootVisualElement.Q<Button>("CategoryRight").clicked += () => AddMenu(1, false);
        comboAnimatronic.SetName("???");

        document.rootVisualElement.Q<Label>("TotalCost").text = "$" + GetTotalCost().ToString();
        currentCompany = PlayerPrefs.GetInt("Game: Current Company");
        SwitchMenu(0,false);
        UpdateCost();
    }

    void UpdateCost()
    {
        uint cost = GetTotalCost();
        Label costLabel = document.rootVisualElement.Q<Label>("TotalCost");
        costLabel.text = "$" + cost.ToString();

        if(cost > saveFileData.money)
        {
            costLabel.style.color = Color.red;
        }
        else
        {
            //This needs to be fixed to not be exactly black
            costLabel.style.color = new Color(0.11764705882352941f, 0.12941176470588237f, 0.11764705882352941f,1);
        }
    }

    uint GetTotalCost()
    {
        uint price = 0;
        for (int i = 0; i < tempParts.Count; i++)
        {
            Combo_Part combo = Resources.Load<GameObject>("Animatronics/Prefabs/" + tempParts[i].id).GetComponent<Combo_Part>();
            price += combo.price;
        }
        return price;
    }

    void AddMenu(int add, bool iterateBackward)
    {
        SwitchMenu(currentMenu + add, iterateBackward);
    }

    void SwitchMenu(int menu, bool iterateBackward)
    {
        currentMenu = menu;
        VisualElement visList = document.rootVisualElement.Q<VisualElement>("ScrollBox");
        document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Visible;
        document.rootVisualElement.Q<Button>("CategoryLeft").style.visibility = Visibility.Visible;


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
        //Find which page to display
        while (!check)
        {
            switch (currentMenu)
            {
                case 0:
                    document.rootVisualElement.Q<Button>("CategoryLeft").style.visibility = Visibility.Hidden;
                    if (tempParts.Count == 0)
                    {
                        document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Hidden;

                    }
                    parts = GetPartsOfTag(Combo_Part.ComboTag.body);
                    tag = Combo_Part.ComboTag.body;
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Choose Body";
                    break;
                case 1:
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Customize Body";
                    tag = Combo_Part.ComboTag.body;
                    break;
                case 2:
                    parts = GetPartsOfTag(Combo_Part.ComboTag.rightArm);
                    tag = Combo_Part.ComboTag.rightArm;
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Choose Rt. Arm";
                    break;
                case 3:
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Customize Rt. Arm";
                    tag = Combo_Part.ComboTag.rightArm;
                    break;
                case 100:
                    document.rootVisualElement.Q<Label>("CategoryLabel").text = "Purchase";
                    document.rootVisualElement.Q<Button>("CategoryRight").style.visibility = Visibility.Hidden;

                    VisualElement currentPurchase = comboPurchase.Instantiate();
                    TextField textField = currentPurchase.Q<TextField>("CharName");
                    textField.value = comboAnimatronic.GetName();
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        comboAnimatronic.SetName(evt.newValue);
                    });

                    Button buyButton = currentPurchase.Q<Button>("Purchase");
                    buyButton.text = "Purchase ($" + GetTotalCost().ToString() + ")";
                    buyButton.clicked += () => SaveAnimatronicToFile();

                    currentPurchase.Q<Label>("BotInfo").text = "Total Movements: " + comboAnimatronic.GetNumberOfMovements().ToString();
                    visList.Add(currentPurchase);
                    check = true;
                    break;
                default:
                    break;
            }
            if (currentMenu % 2 == 0)
            {
                //Iterate for Parts List
                if (parts != null && parts.Count > 0)
                {
                    check = true;
                    //Create item boxes
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
                            currentMenu = 100;
                        }
                    }
                }
            }
            else
            {
                //Iterate for parts settings
                for (int i = 0; i < tempParts.Count; i++)
                {
                    if (tempParts[i].tag == tag)
                    {
                        Combo_Part_SaveFile combo = comboAnimatronic.GetComponent<Combo_Animatronic>().SearchID(tempParts[i].id);
                        if(combo.bendableSections != null && combo.bendableSections.Count > 0)
                        {
                            check = true;
                            //Create item boxes
                            for (int e = 0; e < combo.bendableSections.Count; e++)
                            {
                                float slider = combo.bendableSections[e];

                                VisualElement currentButton = comboSlider.Instantiate();
                                Slider sl = currentButton.Q<Slider>("Slider");
                                sl.value = slider;
                                sl.label = "Bend #" + (e+1);

                                int index = e;
                                sl.RegisterValueChangedCallback(evt =>
                                {
                                    combo.bendableSections[index] = evt.newValue;
                                    comboAnimatronic.RefreshAnimatronicCustomizations();
                                });

                                visList.Add(currentButton);
                            }
                            break;
                        }
                    }
                }
                if(!check)
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
                            currentMenu = 100;
                        }
                    }
                }
            }
        }
    }

    void SaveAnimatronicToFile()
    {
        int cost = (int)GetTotalCost();
        if (cost > saveFileData.money)
        {
            return;
        }

        saveFileData.money -= cost;
        if(mapData.animatronics == null)
        {
            mapData.animatronics = new List<Combo_Animatronic_SaveFile>();
        }
        Combo_Animatronic_SaveFile save = comboAnimatronic.GetSaveFileData();
        save.creationDate = saveFileData.timeFileStarted.dateTime.AddSeconds(saveFileData.timeElapsed);
        save.lastCleanedDate = save.creationDate;
        save.yetToBePlaced = true;
        save.objectHash = Random.Range(int.MinValue, int.MaxValue);
        mapData.animatronics.Add(save);

        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml", saveFileData.SerializeToXML());

        SceneManager.LoadScene(SaveFileData.GetMap(saveFileData.currentMap));
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
        List<int> randoList = new List<int>();
        for (int i = 0; i < maxPossibleAnimatronicMovements; i++)
        {
            randoList.Add(Random.Range(0, 127));
        }
        fakeparts.Add(new UI_Part_Holder() { id = id, tag = tag, DTUIndexes = randoList });

        if (!tempParts.OrderBy(x => x.id).SequenceEqual(fakeparts.OrderBy(x => x.id)))
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

        if (tag == Combo_Part.ComboTag.body)
        {
            tempParts = new List<UI_Part_Holder>();
        }
        else
        {
            for (int i = 0; i < tempParts.Count; i++)
            {
                if (tempParts[i].tag == tag)
                {
                    tempParts.RemoveAt(i);
                }
            }
        }
        List<int> randoList = new List<int>();
        for (int i = 0; i < maxPossibleAnimatronicMovements; i++)
        {
            randoList.Add(Random.Range(0, 127));
        }
        tempParts.Add(new UI_Part_Holder() { id = id, tag = tag, DTUIndexes = randoList });

        if (!tempParts.OrderBy(x => x.id).SequenceEqual(fakeparts.OrderBy(x => x.id)))
        {
            if (inPreview)
            {
                inPreview = false;
            }
            else
            {
                comboAnimatronic.ReassignPartsFromUI(tempParts);
            }

            UpdateCost();

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
}


[System.Serializable]
public class UI_Part_Holder
{
    public Combo_Part.ComboTag tag;
    public uint id;
    public List<int> DTUIndexes;
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