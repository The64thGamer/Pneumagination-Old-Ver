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
        SwitchMenu();
        document.rootVisualElement.Q<Button>("LoadMap").clicked += () => LoadMap();
        document.rootVisualElement.Q<Button>("NameGenerator").clicked += () => SwitchNameGen();
        document.rootVisualElement.Q<Button>("GenBusiness").clicked += () => GenerateName();
        document.rootVisualElement.Q<Button>("BackButton").clicked += () => SwitchMenu();
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

    void LoadMap()
    {
        loadingScene = true;
        SceneManager.LoadSceneAsync("Main Map");
    }

    void SwitchNameGen()
    {
        if(loadingScene)
        {
            return;
        }
        document.rootVisualElement.Q<VisualElement>("MainMenu").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("MenuGenerator").style.display = DisplayStyle.Flex;
    }

    void SwitchMenu()
    {
        if (loadingScene)
        {
            return;
        }
        document.rootVisualElement.Q<VisualElement>("MainMenu").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("MenuGenerator").style.display = DisplayStyle.None;
    }
}
