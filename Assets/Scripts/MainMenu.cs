using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;

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
        document.rootVisualElement.Q<Button>("LoadFirstMap").clicked += () => LoadMap(0);
        document.rootVisualElement.Q<Button>("LoadSecondMap").clicked += () => LoadMap(1);
        document.rootVisualElement.Q<Button>("Settings").clicked += () => SwitchMenu(2);
        document.rootVisualElement.Q<Button>("Exit").clicked += () => Application.Quit();
        document.rootVisualElement.Q<Button>("NameGenerator").clicked += () => SwitchMenu(1);
        document.rootVisualElement.Q<Button>("GenBusiness").clicked += () => GenerateName();
        document.rootVisualElement.Q<Button>("BackButton").clicked += () => SwitchMenu(0);
        document.rootVisualElement.Q<Button>("BackFromSettings").clicked += () => SwitchMenu(0);
        document.rootVisualElement.Q<Toggle>("RandomNames").RegisterValueChangedCallback(ToggleRandomName);

        //Settings
        document.rootVisualElement.Q<DropdownField>("DLSS").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<DropdownField>("Renderer").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<VisualElement>("SampleCount").Q<SliderInt>("Slider").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<VisualElement>("Bounces").Q<SliderInt>("Slider").RegisterValueChangedCallback(SaveAllSettings);
        document.rootVisualElement.Q<VisualElement>("ReflectionProbes").Q<SliderInt>("Slider").RegisterValueChangedCallback(SaveAllSettings);

        int seed = Random.Range(int.MinValue, int.MaxValue);
        int age = Random.Range(0, 80);
        document.rootVisualElement.Q<TextField>("FirstName").value = Name_Generator.GenerateFirstName(seed, age);
        document.rootVisualElement.Q<TextField>("LastName").value = Name_Generator.GenerateLastName(seed);

        if(PlayerPrefs.GetInt(part1) == 0)
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

    void GenerateName()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        int age = Random.Range(0, 80);
        if(document.rootVisualElement.Q<TextField>("FirstName").value == "")
        {
            document.rootVisualElement.Q<TextField>("FirstName").value = "???";
        }
        if (document.rootVisualElement.Q<TextField>("LastName").value == "")
        {
            document.rootVisualElement.Q<TextField>("LastName").value = "???";
        }
        if (generateRandomName)
        {
            document.rootVisualElement.Q<TextField>("FirstName").value = Name_Generator.GenerateFirstName(seed, age);
            document.rootVisualElement.Q<TextField>("LastName").value = Name_Generator.GenerateLastName(seed);
        }
        document.rootVisualElement.Q<Label>("FinalName").text = "\"" + Name_Generator.GenerateLocationName(seed, document.rootVisualElement.Q<TextField>("FirstName").value, document.rootVisualElement.Q<TextField>("LastName").value) + "\"";
    }

    void LoadMap(int map)
    {
        loadingScene = true;
        switch (map)
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
        VisualElement menuGenerator = document.rootVisualElement.Q<VisualElement>("MenuGenerator");
        menuGenerator.style.display = DisplayStyle.None;
        VisualElement menuSettings = document.rootVisualElement.Q<VisualElement>("MenuSettings");
        menuSettings.style.display = DisplayStyle.None;

        switch (menu)
        {
            case 0:
                mainMenu.style.display = DisplayStyle.Flex;
                break;
            case 1:
                menuGenerator.style.display = DisplayStyle.Flex;
                break;
            case 2:
                menuSettings.style.display = DisplayStyle.Flex;
                break;
            default:
                break;
        }

    }
}
