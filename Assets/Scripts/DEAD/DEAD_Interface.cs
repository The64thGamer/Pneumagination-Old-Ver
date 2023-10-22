using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using static DEAD_InterfaceCommands;

public class DEAD_Interface : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] float tapeRewindSpeed = 1.5f;
    [SerializeField] float psi = 40;
    [SerializeField] float[] dataTransferUnit;
    [SerializeField] public bool autoRewind;

    [Header("Showtapes")]
    [SerializeField] int activeShowtapeSlot;
    [SerializeField] DEAD_ShowtapeSlot[] showtapeSlots;

    [Header("Commands")]
    [SerializeField] DEAD_InterfaceCommands[] commands;

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

    public void SendCommand(string name)
    {
        for (int i = 0; i < commands.Length; i++)
        {
            if(name == commands[i].triggerString)
            {
                ExecuteFunction(commands[i].function);
            }
        }
    }

    public void ExecuteFunction(DEAD_InterfaceFunctionList function)
    {
        switch (function)
        {
            case DEAD_InterfaceFunctionList.debug_Log:
                Debug.Log("Command Sent");
                break;
            case DEAD_InterfaceFunctionList.play_Showtape:
                break;
            case DEAD_InterfaceFunctionList.pause_Showtape:
                break;
            case DEAD_InterfaceFunctionList.rewind_Showtape:
                break;
            default:
                break;
        }
    }
}

[System.Serializable]
public class DEAD_ShowtapeSlot
{
    [Header("Info")]
    public string triggerString;
    public float currentTimeElapsed;

    [Header("Loaded Data")]
    public AudioClip[] audio;
    public VideoClip[] video;

    [Header("Tape")]
    public DEAD_Showtape showtape;
}

[System.Serializable]
public class DEAD_InterfaceCommands
{
    public string triggerString;
    public DEAD_InterfaceFunctionList function;

    public enum DEAD_InterfaceFunctionList
    {
        debug_Log,
        play_Showtape,
        pause_Showtape,
        rewind_Showtape,
        enable_Auto_Rewind,
        disable_Auto_Rewind,
        set_Active_Tape_0,
        set_Active_Tape_1,
        set_Active_Tape_2,
        set_Active_Tape_3,
        set_Active_Tape_4,
        set_Active_Tape_5,
        set_Active_Tape_6,
        set_Active_Tape_7,
        set_Active_Tape_8,
        set_Active_Tape_9,
    }
}