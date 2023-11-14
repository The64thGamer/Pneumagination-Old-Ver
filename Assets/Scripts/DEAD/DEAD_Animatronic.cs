using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class DEAD_Animatronic : MonoBehaviour
{
    [SerializeField] DEAD_Actuator[] deadActuators;
    [SerializeField] DEAD_Interface deadInterface;

    int[] animatorHashes;
    Animator animator;

    private void Start()
    {
        animator = this.GetComponent<Animator>();
        ResetHashes();
    }

    void ResetHashes()
    {
        animatorHashes = new int[deadActuators.Length];
        for (int i = 0; i < deadActuators.Length; i++)
        {
            animatorHashes[i] = Animator.StringToHash(deadActuators[i].actuationName + " ID(" + i + ")");
        }
    }

    private void Update()
    {
        if(deadInterface != null)
        {
            UpdateMovements();
        }
    }

    void UpdateMovements()
    {
        if(deadInterface == null)
        {
            return;
        }

        for (int i = 0; i < deadActuators.Length; i++)
        {
            animator.SetFloat(animatorHashes[i], CalculateAirflow(i));
        } 
    }

    float CalculateAirflow(int index)
    {
        float time = Time.deltaTime * deadInterface.GetPSI();
        float dtuData = deadInterface.GetData(deadActuators[index].dtuIndex);
        if (Convert.ToBoolean((int)dtuData))
        {
            time *= deadActuators[index].airflowExtension;
        }
        else
        {
            time *= deadActuators[index].airflowRetraction;
        }
        return Mathf.Lerp(animator.GetFloat(animatorHashes[index]), dtuData, time);
    }

    public int[] GetDTUIndexes()
    {
        int[] dtuIndexes = new int[deadActuators.Length];
        for (int i = 0; i < deadActuators.Length; i++)
        {
            dtuIndexes[i] = deadActuators[i].dtuIndex;
        }
        return dtuIndexes;
    }

    public DEAD_Actuator[] GetActuatorInfoCopy()
    {
        return deadActuators;
    }

    public void SetInterface(DEAD_Interface dead)
    {
        deadInterface = dead;
    }
}

[System.Serializable]
public struct DEAD_Actuator
{
    [Header("Info & Type")]
    public string actuationName;
    public AnimationClip animation;
    public int dtuIndex;
    [HideInInspector]public DEAD_Actuator_Type actuatorType;
    public bool invertedFlow;

    [Header("Lever Settings")]
    [HideInInspector] public Transform fulcrumBone;
    [HideInInspector] public Transform effortBone;
    [HideInInspector] public Transform leverStartBone;
    [HideInInspector] public Transform leverEndBone;
    [HideInInspector] public DEAD_Lever_Weight[] leverWeights;

    [Header("Pneumatic Settings")]
    [HideInInspector] public float boreDiameterInch;
    [HideInInspector] public float strokeLengthInch;
    [HideInInspector] public float rodDiameterInch;
    [Range(0, 1)]
    public float airflowExtension;
    [Range(0, 1)]
    public float airflowRetraction;
    [HideInInspector] public float airlineLengthFeet;


    [Header("Stepper Motor / Servo Settings")]
    [HideInInspector] public float minAngleDegrees;
    [HideInInspector] public float maxAngleDegrees;
    [HideInInspector] public AnimationCurve torqueChartNm;
    [HideInInspector] public AnimationCurve speedChartRpm;

}

public enum DEAD_Actuator_Type
{
    pneumatic,
    stepperMotor,
    servo,
}

[System.Serializable]
public struct DEAD_Lever_Weight
{
    public float weightLbs;
    [Range(0, 1)]
    public float weightPositionOnLever;
}

