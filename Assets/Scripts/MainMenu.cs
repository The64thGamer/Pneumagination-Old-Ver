using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset worldButton;
    [SerializeField] SaveFileData saveFileData;

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
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        SwitchMenu(0);

        //Main Menu
        document.rootVisualElement.Q<Button>("LoadWorlds").clicked += () => SwitchMenu(1); ;
        document.rootVisualElement.Q<Button>("Settings").clicked += () => SwitchMenu(2);
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
        document.rootVisualElement.Q<TextField>("FirstName").value = Name_Generator.GenerateFirstName(Random.Range(int.MinValue, int.MaxValue), Random.Range(0, 80));
    }

    void GenerateLastName()
    {
        document.rootVisualElement.Q<TextField>("LastName").value = Name_Generator.GenerateLastName(Random.Range(int.MinValue, int.MaxValue));
    }

    void GenerateStoreName()
    {
        document.rootVisualElement.Q<TextField>("StoreName").value = Name_Generator.GenerateLocationName(Random.Range(int.MinValue, int.MaxValue), document.rootVisualElement.Q<TextField>("FirstName").value, document.rootVisualElement.Q<TextField>("LastName").value);
    }

    void GenerateRandomSeed()
    {
        document.rootVisualElement.Q<TextField>("Seed").value = Random.Range(int.MinValue, int.MaxValue).ToString();
    }

    void CreateWorld()
    {
        saveFileData = new SaveFileData();
        saveFileData.firstName = document.rootVisualElement.Q<TextField>("FirstName").value;
        saveFileData.lastName = document.rootVisualElement.Q<TextField>("LastName").value;
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
        saveFileData.money = 1000;

        //Update this when theres more save files
        PlayerPrefs.SetInt("CurrentSaveFile", PlayerPrefs.GetInt("SaveFilesLoaded"));

        if (WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml", saveFileData.SerializeToXML()))
        {
            LoadMap();
        }
    }

    bool WriteFile(string path, string data)
    {
        bool retValue;
        try
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, data);
            retValue = true;
        }
        catch (System.Exception ex)
        {
            string ErrorMessages = "File Write Error\n" + ex.Message;
            retValue = false;
            Debug.LogError(ErrorMessages);
        }
        return retValue;
    }
    void LoadMap()
    {
        loadingScene = true;
        switch (saveFileData.currentMap)
        {
            case 0:
                SceneManager.LoadSceneAsync("Fast Food Place");
                break;
            case 1:
                SceneManager.LoadSceneAsync("Drive Thru");
                break;
            default:
                break;
        }
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
            if(data == null)
            {
                break;
            }
            TemplateContainer myUI = worldButton.Instantiate();
            int e = index;
            myUI.Q<Label>("WorldName").text = data.firstName + " " + data.lastName +"'s World";
            myUI.Q<Label>("WorldInfo").text = "Seed: " + data.worldSeed;
            myUI.Q<Button>("Button").clicked += () => StartLoadedSave(e);
            myUI.Q<Button>("DuplicateButton").clicked += () => DuplicateSave(e);

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

        if (WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("SaveFilesLoaded") + "/SaveFile.xml", saveFileData.SerializeToXML()))
        {
            AddNewWorldButtons();
        }
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
}
