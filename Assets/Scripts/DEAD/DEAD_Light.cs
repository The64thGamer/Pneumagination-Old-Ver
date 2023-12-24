using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DEAD_Light : MonoBehaviour
{
    [SerializeField] DEAD_Interface deadInterface;

    [Header("Objects")]
    [SerializeField] List<Light> lights;
    [SerializeField] List<Renderer> emissiveMeshes;

    [Header("Signal")]
    [SerializeField] int dtuIndex;
    [SerializeField] bool invertedFlow;

    [Header("Light Properties")]
    [SerializeField] float lightIntensity = 1;
    [SerializeField] Color lightColor = Color.white;
    [SerializeField] float emissionIntensity = 1;
    [SerializeField] Color emissionTint = Color.white;

    [Header("Light Curves")]
    [SerializeField] AnimationCurve onCurve;
    [SerializeField] float turnOnTime;
    [SerializeField] AnimationCurve offCurve;
    [SerializeField] float turnOffTime;

    [Header("Looping")]
    [SerializeField] bool doesLoop;
    [SerializeField] AnimationCurve loopCurve;
    [SerializeField] float loopTime;


    bool currentState;
    float currentStateTimer;
    bool inLoopState;

    private void Update()
    {
        if (deadInterface != null)
        {
            UpdateLight();
        }
    }

    void UpdateLight()
    {
        float dtuData = deadInterface.GetData(dtuIndex);
        if(invertedFlow)
        {
            dtuData = 1 - dtuData;
        }

        currentStateTimer += Time.deltaTime;
        bool dtuDataBool = Convert.ToBoolean((int)dtuData);
        
        if(dtuDataBool != currentState)
        {
            currentState = dtuDataBool;
            currentStateTimer = 0;
            inLoopState = false;
        }

        float finalIntensity = 0;
        if (!inLoopState)
        {
            if (currentState)
            {
                finalIntensity = currentStateTimer / turnOnTime;
                if (finalIntensity > 1 && doesLoop)
                {
                    inLoopState = true;
                    currentStateTimer = 0;
                }
                else
                {
                    finalIntensity = onCurve.Evaluate(finalIntensity);
                }
            }
            else
            {
                finalIntensity = offCurve.Evaluate(currentStateTimer / turnOffTime);

            }
        }

        if (inLoopState && doesLoop)
        {
            finalIntensity = loopCurve.Evaluate(Mathf.Repeat(currentStateTimer / loopTime,1));
        }

        for (int i = 0; i < lights.Count; i++)
        {
            lights[i].color = lightColor;
            lights[i].intensity = finalIntensity * lightIntensity;
        }
        for (int i = 0; i < emissiveMeshes.Count; i++)
        {
            for (int e = 0; e < emissiveMeshes[i].materials.Length; e++)
            {
                emissiveMeshes[i].materials[e].SetColor("_EmissiveColor", emissionTint * (finalIntensity * emissionIntensity)); 
            }
        }
    }
}
