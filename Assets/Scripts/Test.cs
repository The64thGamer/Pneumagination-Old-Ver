using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;

    bool generateRandomName = true;

    bool loadingScene;

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

        int seed = Random.Range(int.MinValue, int.MaxValue);
        int age = Random.Range(0, 80);
        document.rootVisualElement.Q<TextField>("FirstName").value = Name_Generator.GenerateFirstName(seed, age);
        document.rootVisualElement.Q<TextField>("LastName").value = Name_Generator.GenerateLastName(seed);
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
