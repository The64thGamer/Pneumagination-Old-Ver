using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Header("Random Generator")]
    public bool generateRandomName;
    public int age;
    public int seed;

    [Header("Outputs")]
    public string placeName;
    public string firstName;
    public string lastName;

    int oldSeed;
    int oldage;


    private void Update()
    {
        if (seed != oldSeed || age != oldage)
        {
            oldSeed = seed;
            oldage = age;
            if(generateRandomName)
            {
                firstName = Name_Generator.GenerateFirstName(seed, age);
                lastName = Name_Generator.GenerateLastName(seed);
            }
            placeName = Name_Generator.GenerateLocationName(seed, firstName,lastName);
        }
    }
}
