using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class DEAD_Animatronic : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] DEAD_Actuator[] deadActuators;

    DEAD_Interface deadInterface;
    Animator animator;

    void ResetHashes()
    {

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
}

[System.Serializable]
public struct DEAD_Actuator
{
    [Header("Info & Type")]
    public string actuationName;
    public AnimationClip animation;
    public int dtuIndex;
    public DEAD_Actuator_Type actuatorType;
    public bool invertedFlow;

    [Header("Lever Settings")]
    public Transform fulcrumBone;
    public Transform effortBone;
    public Transform leverStartBone;
    public Transform leverEndBone;
    public DEAD_Lever_Weight[] leverWeights;

    [Header("Pneumatic Settings")]
    public float boreDiameterInch;
    public float strokeLengthInch;
    public float rodDiameterInch;
    [Range(0, 1)]
    public float airflowExtension;
    [Range(0, 1)]
    public float airflowRetraction;
    public float airlineLengthFeet;


    [Header("Stepper Motor / Servo Settings")]
    public float minAngleDegrees;
    public float maxAngleDegrees;
    public AnimationCurve torqueChartNm;
    public AnimationCurve speedChartRpm;

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

