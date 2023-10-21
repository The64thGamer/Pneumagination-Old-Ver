using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DEAD_Interface : MonoBehaviour
{
    [SerializeField] float psi = 40;
    [SerializeField] float[] dataTransferUnit;

    public float GetData(int index)
    {
        if (index < 0 || index > dataTransferUnit.Length - 1 || dataTransferUnit == null)
        {
            return 0;
        }

        return dataTransferUnit[index];
    }

    public void SetData(int index, float value)
    {
        if (index < 0 || index > dataTransferUnit.Length - 1 || dataTransferUnit == null)
        {
            return;
        }

        dataTransferUnit[index] = value;
    }

    public int GetDTUArrayLength()
    {
        if(dataTransferUnit == null)
        {
            return 0;
        }
        return dataTransferUnit.Length;
    }

    public float GetPSI()
    {
        return Mathf.Max(0,psi);
    }
}