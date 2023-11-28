using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DEAD_Light : MonoBehaviour
{
    [SerializeField] DEAD_Interface deadInterface;

    [Header("Signal")]
    public int dtuIndex;
    public bool invertedFlow;

    [Header("Light Properties")]
    public float intensity;

    [Header("Light Curves")]
    [SerializeField] AnimationCurve onCurve;
    [SerializeField] float turnOnTime;
    [SerializeField] AnimationCurve offCurve;
    [SerializeField] float turnOffTime;

    [Header("Looping")]
    [SerializeField] bool doesLoop;
    [SerializeField] AnimationCurve loopCurve;
    [SerializeField] float loopTime;

    Light light;

    bool currentState;
    float currentStateTimer;
    bool inLoopState;

    private void Start()
    {
        light = this.GetComponent<Light>();
    }

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

        float finalIntensity = light.intensity;
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
                    finalIntensity = onCurve.Evaluate(finalIntensity) * intensity;
                }
            }
            else
            {
                finalIntensity = offCurve.Evaluate(currentStateTimer / turnOffTime) * intensity;

            }
        }

        if (inLoopState && doesLoop)
        {
            finalIntensity = loopCurve.Evaluate(Mathf.Repeat(currentStateTimer / loopTime,1)) * intensity;
        }

        light.intensity = finalIntensity;
    }
}
