using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DEAD_Interface : MonoBehaviour
{
    [SerializeField] float[] dataTransferUnit;

    public float GetData(int index)
    {
        if (index < 0 || index > dataTransferUnit.Length - 1 || dataTransferUnit == null)
        {
            return 0;
        }

        return dataTransferUnit[index];
    }
    public int GetDTUArrayLength()
    {
        if(dataTransferUnit == null)
        {
            return 0;
        }
        return dataTransferUnit.Length;
    }
}