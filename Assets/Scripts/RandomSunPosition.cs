using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomSunPosition : MonoBehaviour
{
    [SerializeField] float timeMultiplier = 15;
    float timeOfDay;
    float randomPitch;

    void Start()
    {
        timeOfDay = Random.Range(0.21f * 86400, 0.58f * 86400);
        randomPitch = Random.Range(0.0f, 360.0f);
    }

    private void Update()
    {
        timeOfDay = (timeOfDay + Time.deltaTime * timeMultiplier) % 86400;
        transform.localRotation = Quaternion.Euler(((timeOfDay / 86400) * 360f) - 90, randomPitch, 0);
    }

}
