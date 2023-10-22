using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using static DEAD_InterfaceCommands;

[RequireComponent(typeof(NAudioImporter))]
public class DEAD_Interface : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] float tapeRewindSpeed = 1.5f;
    [SerializeField] float tapePlaySpeed = 1.0f;
    [SerializeField] float fallbackPsi = 40;
    [SerializeField] float[] dataTransferUnit;

    [Header("Showtapes")]
    [SerializeField] int activeShowtapeSlot;
    [SerializeField] DEAD_ShowtapeSlot[] showtapeSlots;

    [Header("Commands")]
    [SerializeField] DEAD_InterfaceCommands[] commands;

    //Values
    bool playingShowtape;
    bool autoRewind = true;
    LoadingState loadingState;

    //Events
    [HideInInspector] public DEAD_DTUEvent dtuSet = new DEAD_DTUEvent();
    [HideInInspector] public DEAD_CommandEvent commandSet = new DEAD_CommandEvent();

    public enum LoadingState
    {
        ready,
        loading,
    }

    void Update()
    {
        RunShowtape();
    }

    void RunShowtape()
    {
        //Back out early
        if(showtapeSlots == null || showtapeSlots.Length == 0)
        {
            playingShowtape = false;
            return;
        }

        //Double check erroneous settings
        activeShowtapeSlot = Mathf.Clamp(activeShowtapeSlot,0, showtapeSlots.Length);

        //Extra Checks
        if(showtapeSlots[activeShowtapeSlot].showtape == null || !showtapeSlots[activeShowtapeSlot].nonBlankShowtape)
        {
            playingShowtape = false;
            return;
        }

        //Playback
        if (playingShowtape)
        {
            //Time Elapsed
            float oldTime = showtapeSlots[activeShowtapeSlot].currentTimeElapsed;
            showtapeSlots[activeShowtapeSlot].currentTimeElapsed += Time.deltaTime;
            if(showtapeSlots[activeShowtapeSlot].currentTimeElapsed > showtapeSlots[activeShowtapeSlot].showtape.endOfTapeTime)
            {
                showtapeSlots[activeShowtapeSlot].currentTimeElapsed = showtapeSlots[activeShowtapeSlot].showtape.endOfTapeTime;
            }
            float newTime = showtapeSlots[activeShowtapeSlot].currentTimeElapsed;

            //More Double Checks to Back Out
            if (showtapeSlots[activeShowtapeSlot].showtape.layers != null && showtapeSlots[activeShowtapeSlot].showtape.layers.Length != 0)
            {
                if (showtapeSlots[activeShowtapeSlot].showtape.layers[showtapeSlots[activeShowtapeSlot].activeLayer] != null)
                {
                    //Find Current Signals
                    List<DEAD_Signal_Data> signals = showtapeSlots[activeShowtapeSlot].showtape.layers[showtapeSlots[activeShowtapeSlot].activeLayer].signals;
                    if (signals != null)
                    {
                        int found = -1;

                        if (signals.Count == 1)
                        {
                            found = 0;
                        }
                        else if (signals.Count > 1)
                        {                            
                            //(Non)Binary Search
                            int left = 0;
                            int right = signals.Count - 1;

                            while (left <= right)
                            {
                                int mid = left + (right - left) / 2;

                                // Check if the target is present at mid
                                if (Mathf.FloorToInt(signals[mid].time) == Mathf.FloorToInt(oldTime))
                                {
                                    found = mid;
                                }

                                // If the target is greater, ignore left half
                                if (Mathf.FloorToInt(signals[mid].time) < Mathf.FloorToInt(oldTime))
                                {
                                    left = mid + 1;
                                }
                                // If the target is smaller, ignore right half
                                else
                                {
                                    right = mid - 1;
                                }
                            }

                        }

                        //Apply all signals since last frame
                        if (found != -1)
                        {
                            while (found < signals.Count)
                            {
                                if(signals[found].time > newTime)
                                {
                                    break;
                                }
                                if (signals[found].time > oldTime)
                                {
                                    dataTransferUnit[signals[found].dtuIndex] = signals[found].value;
                                }
                                found++;
                            }
                        }
                    }

                    //Find Current Commands
                    List<DEAD_Command_Data> commands = showtapeSlots[activeShowtapeSlot].showtape.layers[showtapeSlots[activeShowtapeSlot].activeLayer].commands;
                    if (commands != null)
                    {
                        int found = -1;

                        if (commands.Count == 1)
                        {
                            found = 0;
                        }
                        else if (commands.Count > 1)
                        {
                            //(Non)Binary Search
                            int left = 0;
                            int right = signals.Count - 1;

                            while (left <= right)
                            {
                                int mid = left + (right - left) / 2;

                                // Check if the target is present at mid
                                if (Mathf.FloorToInt(commands[mid].time) == Mathf.FloorToInt(oldTime))
                                {
                                    found = mid;
                                }

                                // If the target is greater, ignore left half
                                if (Mathf.FloorToInt(commands[mid].time) < Mathf.FloorToInt(oldTime))
                                {
                                    left = mid + 1;
                                }
                                // If the target is smaller, ignore right half
                                else
                                {
                                    right = mid - 1;
                                }
                            }
                        }

                        //Apply all commands since last frame
                        if (found != -1)
                        {
                            while (found < commands.Count)
                            {
                                if(commands[found].time > newTime)
                                {
                                    break;
                                }
                                if (commands[found].time > oldTime)
                                {
                                    SendCommand(commands[found].value,false);
                                }
                                found++;
                            }
                        }
                    }
                }
            }
        }
    }

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
        if (dataTransferUnit == null || index < 0 || index > dataTransferUnit.Length - 1)
        {
            return;
        }

        dataTransferUnit[index] = value;
        if (playingShowtape && showtapeSlots != null && activeShowtapeSlot < showtapeSlots.Length && showtapeSlots[activeShowtapeSlot] != null && showtapeSlots[activeShowtapeSlot].nonBlankShowtape)
        {
            dtuSet.Invoke(index, showtapeSlots[activeShowtapeSlot].currentTimeElapsed, value);
        }
    }


    public void SetShowtape(int index, DEAD_Showtape showtape)
    {
        if(showtapeSlots == null || index < 0 || index >showtapeSlots.Length)
        {
            return;
        }
        SetShowtape(index, showtape, showtapeSlots[index].triggerString);
    }

    public void SetShowtape(int index, DEAD_Showtape showtape, string triggerString)
    {
        if (showtapeSlots == null || index < 0 || index > showtapeSlots.Length)
        {
            return;
        }
        if(playingShowtape)
        {
            playingShowtape = false;
        }
        showtapeSlots[index].showtape = showtape;
        showtapeSlots[index].currentTimeElapsed = 0;
        showtapeSlots[index].triggerString = triggerString;
        showtapeSlots[index].nonBlankShowtape = true;
        if (showtapeSlots[index].showtape.audioClips != null)
        {
            showtapeSlots[index].audio = new AudioClip[showtapeSlots[index].showtape.audioClips.Length];
            StartCoroutine(ImportAudio(index));
        }
    }

    public DEAD_Showtape GetShowtape(int index)
    {
        if (showtapeSlots == null || index < 0 || index > showtapeSlots.Length || !showtapeSlots[index].nonBlankShowtape)
        {
            return null;
        }
        return showtapeSlots[index].showtape;
    }

    IEnumerator ImportAudio(int index)
    {
        loadingState = LoadingState.loading;
        NAudioImporter importer = this.GetComponent<NAudioImporter>();
        for (int i = 0; i < showtapeSlots[index].showtape.audioClips.Length; i++)
        {
            importer.Import(showtapeSlots[index].showtape.audioClips[i]);
            while (!importer.isInitialized && !importer.isError)
            {
                yield return null;
            }

            if (importer.isError)
            {
                Debug.LogError(importer.error);
            }

            showtapeSlots[index].audio[i] = importer.audioClip;
        }
        loadingState = LoadingState.ready;
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
        return Mathf.Max(0,fallbackPsi);
    }

    public void SendCommand(string name, bool nonRecordableCommand)
    {
        for (int i = 0; i < commands.Length; i++)
        {
            if(name == commands[i].triggerString)
            {
                ExecuteFunction(commands[i].function);
            }
        }

        if (playingShowtape && !nonRecordableCommand && showtapeSlots != null && activeShowtapeSlot < showtapeSlots.Length && showtapeSlots[activeShowtapeSlot] != null && showtapeSlots[activeShowtapeSlot].nonBlankShowtape)
        {
            commandSet.Invoke(showtapeSlots[activeShowtapeSlot].currentTimeElapsed, name);
        }
    }

    public void ExecuteFunction(DEAD_InterfaceFunctionList function)
    {
        if(loadingState == LoadingState.loading)
        {
            Debug.Log("Didn't execute function, currently in loading state");
            return;
        }

        switch (function)
        {
            case DEAD_InterfaceFunctionList.debug_Log:
                Debug.Log("Command Sent");
                break;
            case DEAD_InterfaceFunctionList.play_Showtape:
                playingShowtape = true;
                break;
            case DEAD_InterfaceFunctionList.pause_Showtape:
                playingShowtape = false;
                break;
            case DEAD_InterfaceFunctionList.rewind_Showtape:
                break;
            case DEAD_InterfaceFunctionList.enable_Auto_Rewind:
                autoRewind = true;
                break;
            case DEAD_InterfaceFunctionList.disable_Auto_Rewind:
                autoRewind = false;
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_0:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_1:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_2:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_3:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_4:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_5:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_6:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_7:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_8:
                break;
            case DEAD_InterfaceFunctionList.set_Active_Tape_9:
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
    [HideInInspector] public bool nonBlankShowtape;
    public string triggerString;
    public float currentTimeElapsed;

    [Header("Loaded Data")]
    public AudioClip[] audio;
    public VideoClip[] video;

    [Header("Tape")]
    public int activeLayer;
    public DEAD_Showtape showtape;
}

[System.Serializable]
public class DEAD_DTUEvent : UnityEvent<int,float,float>
{
}

[System.Serializable]
public class DEAD_CommandEvent : UnityEvent<float, string>
{
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