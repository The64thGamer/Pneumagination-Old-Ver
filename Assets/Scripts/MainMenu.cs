using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class MainMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset worldButton;
    [SerializeField] SaveFileData saveFileData;

    AudioSource au;

    bool generateRandomName = true;
    bool loadingScene;

    //Const
    const string part1 = "Startup: SettingsPart1";
    const string dlssMode = "Settings: DLSS";
    const string lightingMode = "Settings: Lighting";
    const string reflectionsMode = "Settings: Reflections";
    const string sampleCount = "Settings: Sample Count";
    const string bounceCount = "Settings: Bounce Count";
    const string probeTimer = "Settings: Probe Timer";

    void OnEnable()
    {
        au = this.GetComponent<AudioSource>();
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        SwitchMenu(0);

        RegisterButtonHovers(document.rootVisualElement);



        //Main Menu
        document.rootVisualElement.Q<Button>("LoadWorlds").clicked += () => SwitchMenu(1);
        document.rootVisualElement.Q<Button>("Settings").clicked += () => SwitchMenu(2);
        document.rootVisualElement.Q<Button>("Manual").clicked += () => Process.Start("explorer.exe", (Application.dataPath + "/StreamingAssets/").Replace(@"/", @"\"));
        document.rootVisualElement.Q<Button>("Exit").clicked += () => Application.Quit();


        //Save Files Menu
        document.rootVisualElement.Q<Button>("CreateWorld").clicked += () => SwitchMenu(3);
        document.rootVisualElement.Q<Button>("BackFromWorlds").clicked += () => SwitchMenu(0);

        //Create World Menu
        document.rootVisualElement.Q<Button>("FirstNameRandom").clicked += () => GenerateFirstName();
        document.rootVisualElement.Q<Button>("LastNameRandom").clicked += () => GenerateLastName();
        document.rootVisualElement.Q<Button>("SeedRandom").clicked += () => GenerateRandomSeed();
        document.rootVisualElement.Q<Button>("StoreNameRandom").clicked += () => GenerateStoreName();
        document.rootVisualElement.Q<Button>("BackFromCreateWorlds").clicked += () => SwitchMenu(1);
        document.rootVisualElement.Q<Button>("StartWorld").clicked += () => CreateWorld();

        //Settings Menu
        document.rootVisualElement.Q<DropdownField>("DLSS").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<DropdownField>("Renderer").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<VisualElement>("SampleCount").Q<SliderInt>("Slider").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<VisualElement>("Bounces").Q<SliderInt>("Slider").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<VisualElement>("ReflectionProbes").Q<SliderInt>("Slider").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<Button>("BackFromSettings").clicked += () => SwitchMenu(0);


        if (PlayerPrefs.GetInt(part1) == 0)
        {
            FirstTimeBootingSettings();
        }
        LoadAllSettings();
    }

    void FirstTimeBootingSettings()
    {
        PlayerPrefs.SetInt(part1, 1);

        PlayerPrefs.SetInt(dlssMode, 3);
        PlayerPrefs.SetInt(lightingMode, 2);
        PlayerPrefs.SetInt(reflectionsMode, 0);
        PlayerPrefs.SetInt(sampleCount, 2);
        PlayerPrefs.SetInt(bounceCount, 1);
        PlayerPrefs.SetInt(probeTimer, 30);
    }
    void SaveAllSettings(UnityEngine.UIElements.ChangeEvent<int> toggle)
    {
        SaveAllSettings();
    }
    void SaveAllSettings(UnityEngine.UIElements.ChangeEvent<string> toggle)
    {
        SaveAllSettings();
    }

    void SaveAllSettings()
    {
        PlayerPrefs.SetInt(dlssMode, document.rootVisualElement.Q<DropdownField>("DLSS").index);
        PlayerPrefs.SetInt(lightingMode, document.rootVisualElement.Q<DropdownField>("LightingMode").index);
        PlayerPrefs.SetInt(reflectionsMode, document.rootVisualElement.Q<DropdownField>("ReflectionsMode").index);
        PlayerPrefs.SetInt(sampleCount, document.rootVisualElement.Q<VisualElement>("SampleCount").Q<SliderInt>("Slider").value);
        PlayerPrefs.SetInt(bounceCount, document.rootVisualElement.Q<VisualElement>("Bounces").Q<SliderInt>("Slider").value);
        PlayerPrefs.SetInt(probeTimer, document.rootVisualElement.Q<VisualElement>("ReflectionProbes").Q<SliderInt>("Slider").value);
    }

    void LoadAllSettings()
    {
        int dlssLoad = PlayerPrefs.GetInt(dlssMode);
        int lightingLoad = PlayerPrefs.GetInt(lightingMode);
        int reflectionsLoad = PlayerPrefs.GetInt(reflectionsMode);
        int sampleLoad = PlayerPrefs.GetInt(sampleCount);
        int bounceLoad = PlayerPrefs.GetInt(bounceCount);
        int probeLoad = PlayerPrefs.GetInt(probeTimer);
        document.rootVisualElement.Q<DropdownField>("DLSS").index = dlssLoad;
        document.rootVisualElement.Q<DropdownField>("LightingMode").index = lightingLoad;
        document.rootVisualElement.Q<DropdownField>("ReflectionsMode").index = reflectionsLoad;
        document.rootVisualElement.Q<VisualElement>("SampleCount").Q<SliderInt>("Slider").value = (sampleLoad);
        document.rootVisualElement.Q<VisualElement>("Bounces").Q<SliderInt>("Slider").value = (bounceLoad);
        document.rootVisualElement.Q<VisualElement>("ReflectionProbes").Q<SliderInt>("Slider").value = (probeLoad);
    }

    void ToggleRandomName(UnityEngine.UIElements.ChangeEvent<bool> toggle)
    {
        generateRandomName = toggle.newValue;
    }


    void GenerateFirstName()
    {
        document.rootVisualElement.Q<TextField>("FirstName").value = Name_Generator.GenerateFirstName(UnityEngine.Random.Range(int.MinValue, int.MaxValue), UnityEngine.Random.Range(0, 80));
    }

    void GenerateLastName()
    {
        document.rootVisualElement.Q<TextField>("LastName").value = Name_Generator.GenerateLastName(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    void GenerateStoreName()
    {
        document.rootVisualElement.Q<TextField>("StoreName").value = Name_Generator.GenerateLocationName(UnityEngine.Random.Range(int.MinValue, int.MaxValue), document.rootVisualElement.Q<TextField>("FirstName").value, document.rootVisualElement.Q<TextField>("LastName").value);
    }

    void GenerateRandomSeed()
    {
        document.rootVisualElement.Q<TextField>("Seed").value = UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString();
    }

    void CreateWorld()
    {
        saveFileData = new SaveFileData();
        saveFileData.firstName = document.rootVisualElement.Q<TextField>("FirstName").value;
        saveFileData.lastName = document.rootVisualElement.Q<TextField>("LastName").value;
        saveFileData.timeElapsed = 0;
        saveFileData.timeFileStarted = new UDateTime();
        saveFileData.timeFileStarted.dateTime = Name_Generator.GenerateRandomDateRange(new DateTime(1979, 1, 1), new DateTime(1979, 12, 31));
        int num;
        if (int.TryParse(document.rootVisualElement.Q<TextField>("Seed").value, out num))
        {
            saveFileData.worldSeed = num;
        }
        else
        {
            saveFileData.worldSeed = Animator.StringToHash(document.rootVisualElement.Q<TextField>("Seed").value);
        }
        PlayerPrefs.SetString("CreateWorldName", document.rootVisualElement.Q<TextField>("StoreName").value);
        if (document.rootVisualElement.Q<Toggle>("UseHardMode").value)
        {
            saveFileData.currentMap = 1;
        }
        saveFileData.money = 100000;

        //Update this when theres more save files
        PlayerPrefs.SetInt("CurrentSaveFile", PlayerPrefs.GetInt("SaveFilesLoaded"));

        if (DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml", saveFileData.SerializeToXML()))
        {
            LoadMap();
        }
    }


    void LoadMap()
    {
        loadingScene = true;
        SceneManager.LoadSceneAsync(SaveFileData.GetMap(saveFileData.currentMap));
    }

    void SwitchMenu(int menu)
    {
        if (loadingScene)
        {
            return;
        }

        VisualElement mainMenu = document.rootVisualElement.Q<VisualElement>("MainMenu");
        mainMenu.style.display = DisplayStyle.None;
        VisualElement menuWorlds = document.rootVisualElement.Q<VisualElement>("MenuSaveFiles");
        menuWorlds.style.display = DisplayStyle.None;
        VisualElement menuCreateWorld = document.rootVisualElement.Q<VisualElement>("MenuCreateWorld");
        menuCreateWorld.style.display = DisplayStyle.None;
        VisualElement menuSettings = document.rootVisualElement.Q<VisualElement>("MenuSettings");
        menuSettings.style.display = DisplayStyle.None;

        switch (menu)
        {
            case 0:
                mainMenu.style.display = DisplayStyle.Flex;
                break;
            case 1:
                menuWorlds.style.display = DisplayStyle.Flex;
                AddNewWorldButtons();
                break;
            case 2:
                menuSettings.style.display = DisplayStyle.Flex;
                break;
            case 3:
                menuCreateWorld.style.display = DisplayStyle.Flex;
                GenerateRandomSeed();
                break;
            default:
                break;
        }

    }

    void AddNewWorldButtons()
    {
        VisualElement visList = document.rootVisualElement.Q<VisualElement>("WorldContainer");

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

        int index = 0;
        while (true)
        {
            SaveFileData data = FindData(index);
            if (data == null)
            {
                break;
            }
            TemplateContainer myUI = worldButton.Instantiate();
            int e = index;
            myUI.Q<Label>("WorldName").text = data.firstName + " " + data.lastName + "'s World";
            myUI.Q<Label>("WorldInfo").text = "Seed: " + data.worldSeed;
            myUI.Q<Button>("Button").clicked += () => StartLoadedSave(e);
            myUI.Q<Button>("DuplicateButton").clicked += () => DuplicateSave(e);
            myUI.Q<Button>("DeleteButton").clicked += () => DeleteSave(e);
            RegisterButtonHovers(myUI);
            visList.Add(myUI);
            index++;
        }
        PlayerPrefs.SetInt("SaveFilesLoaded", index);

    }

    void DuplicateSave(int slot)
    {
        string saveFilePath = Application.persistentDataPath + "/Saves/Save" + slot + "/SaveFile.xml";

        if (!File.Exists(saveFilePath))
        {
            return;
        }
        else
        {
            saveFileData = saveFileData.DeserializeFromXML(File.ReadAllText(saveFilePath));
        }

        if (DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("SaveFilesLoaded") + "/SaveFile.xml", saveFileData.SerializeToXML()))
        {
            AddNewWorldButtons();
        }
    }

    void DeleteSave(int slot)
    {
        string saveFilePath = Application.persistentDataPath + "/Saves/Save"; ;

        if (!Directory.Exists(saveFilePath + slot.ToString()))
        {
            Debug.Log("File Directory Doesn't exist");
            return;
        }
        Directory.Delete(saveFilePath + slot.ToString(), true);

        int slotLength = PlayerPrefs.GetInt("SaveFilesLoaded");
        if (slot != slotLength - 1)
        {
            for (int i = slot + 1; i < slotLength; i++)
            {
                Directory.Move(saveFilePath + i.ToString(), saveFilePath + (i - 1).ToString());
            }
        }

        AddNewWorldButtons();

    }

    void StartLoadedSave(int slot)
    {
        PlayerPrefs.SetInt("CurrentSaveFile", slot);

        string saveFilePath = Application.persistentDataPath + "/Saves/Save" + slot + "/SaveFile.xml";

        if (!File.Exists(saveFilePath))
        {
            return;
        }
        else
        {
            saveFileData = saveFileData.DeserializeFromXML(File.ReadAllText(saveFilePath));
        }

        if (saveFileData != null)
        {
            LoadMap();
        }
    }

    SaveFileData FindData(int index)
    {
        string saveFilePath = Application.persistentDataPath + "/Saves/Save" + index + "/SaveFile.xml";

        if (!File.Exists(saveFilePath))
        {
            return null;
        }
        else
        {
            saveFileData = saveFileData.DeserializeFromXML(File.ReadAllText(saveFilePath));
        }
        return saveFileData;
    }

    void RegisterButtonHovers(VisualElement element)
    {
        List<Button> buttons = element.Query<Button>().ToList();
        for (int i = 0; i < buttons.Count; i++)
        {
            Button b = buttons[i];
            b.RegisterCallback<MouseEnterEvent>(evt =>
            {
                au.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu/Pencil Stroke " + Random.Range(0, 21)), 0.5f);
                b.style.borderTopWidth = 8;
                b.style.borderRightWidth = 8;
                b.style.borderLeftWidth = 8;
                b.style.borderBottomWidth = 8;
            });
            b.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                b.style.borderTopWidth = 4;
                b.style.borderRightWidth = 4;
                b.style.borderLeftWidth = 4;
                b.style.borderBottomWidth = 4;
            });
            b.clicked += () => au.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu/Pen Flick"), 0.75f);
        }
    }
}
