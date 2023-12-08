using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomSunPosition : MonoBehaviour
{
    [SerializeField] float timeMultiplier = 15;
    [SerializeField] Light sun;
    [SerializeField] Light moon;
    float timeOfDay;
    float randomPitch;
    bool nightMode;

    void Start()
    {
        timeOfDay = Random.Range(0.21f * 86400, 0.58f * 86400);
        randomPitch = Random.Range(0.0f, 360.0f);
    }

    private void Update()
    {
        timeOfDay = (timeOfDay + Time.deltaTime * timeMultiplier) % 86400;
        float time = ((timeOfDay / 86400) * 360f) - 90;
        float altTime = time;
        while(altTime < 0)
        {
            altTime = 360 + altTime;
        }
        altTime = altTime % 360;
        if (altTime > 186 && altTime < 354 && !nightMode)
        {
            nightMode = true;
            sun.shadows = LightShadows.None;
            moon.shadows = LightShadows.Soft;
        }
        if (altTime <= 186 || altTime >= 354 && nightMode)
        {
            nightMode = false;
            moon.shadows = LightShadows.None;
            sun.shadows = LightShadows.Soft;
        }

        sun.transform.localRotation = Quaternion.Euler(time, randomPitch, 0);
        moon.transform.localRotation = Quaternion.Euler(time+180, randomPitch, 0);
    }

}
