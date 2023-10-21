using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class DEAD_Interface : MonoBehaviour
{
    [SerializeField] float psi = 40;
    [SerializeField] float[] dataTransferUnit;

    [Header("Showtapes")]
    [SerializeField] int activeShowtapeSlot;
    [SerializeField] DEAD_ShowtapeSlot[] showtapeSlots;

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

[System.Serializable]
public class DEAD_ShowtapeSlot
{
    public string triggerString;
    public DEAD_Showtape showtape;
}