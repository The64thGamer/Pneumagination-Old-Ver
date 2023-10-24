using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DEAD_Speaker : MonoBehaviour
{
    [SerializeField] DEAD_Interface deadInterface;
    [Range(0, 9)]
    [SerializeField] int defaultAudioSlot;
    [SerializeField] PanSetting defaultAudioChannel;
    [SerializeField] List<DEAD_SpeakerCommands> commands;

    AudioSource au;
    int activeAudioSlot;
    float savedAudioVolume;

    private void Start()
    {
        au = this.GetComponent<AudioSource>();

        switch (defaultAudioChannel)
        {
            case PanSetting.stereo:
                au.panStereo = 0;
                break;
            case PanSetting.leftChannel:
                au.panStereo = -1;
                break;
            case PanSetting.rightChannel:
                au.panStereo = 1;
                break;
            default:
                break;
        }
        activeAudioSlot = defaultAudioSlot;
        savedAudioVolume = au.volume;
    }

    private void OnEnable()
    {
        if (deadInterface != null)
        {
            deadInterface.commandSet.AddListener(CommandSet);
        }
    }

    private void OnDisable()
    {
        if (deadInterface != null)
        {
            deadInterface.commandSet.RemoveListener(CommandSet);
        }
    }

    void GetAndPlayAudio()
    {
        StartCoroutine(GetAudioCoroutine());  
    }


    IEnumerator GetAudioCoroutine()
    {
        while (deadInterface.getLoadingState() == DEAD_Interface.LoadingState.loading)
        {
            yield return null;
        }
        au.clip = deadInterface.GetAudioClip(activeAudioSlot);
        au.time = deadInterface.GetCurrentTapeTime();
        if (au.clip != null)
        {
            au.Play();
        }
    }

    void CommandSet(float time, string value)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].triggerString == value)
            {
                switch (commands[i].function)
                {
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.debug_Log:
                        Debug.Log("Command Sent To Speaker");
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.play_audio:
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.pause_audio:
                        if (au.clip != null)
                        {
                            au.Pause();
                        }
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.rewind_audio:
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.unmute_speaker:
                        au.volume = savedAudioVolume;
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.mute_speaker:
                        savedAudioVolume = au.volume;
                        au.volume = 0;
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_0:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_1:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_2:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_3:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_4:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_5:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_6:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_7:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_8:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_audio_slot_9:
                        activeAudioSlot = 0;
                        GetAndPlayAudio();
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_pan_stereo:
                        au.panStereo = 0;
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_pan_left_channel:
                        au.panStereo = -1;
                        break;
                    case DEAD_SpeakerCommands.DEAD_InterfaceFunctionList.set_pan_right_channel:
                        au.panStereo = 1;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

public enum PanSetting
{
    stereo,
    leftChannel,
    rightChannel,
}

[System.Serializable]
public class DEAD_SpeakerCommands
{
    public string triggerString;
    public DEAD_InterfaceFunctionList function;

    public enum DEAD_InterfaceFunctionList
    {
        debug_Log,
        play_audio,
        pause_audio,
        rewind_audio,
        unmute_speaker,
        mute_speaker,
        set_audio_slot_0,
        set_audio_slot_1,
        set_audio_slot_2,
        set_audio_slot_3,
        set_audio_slot_4,
        set_audio_slot_5,
        set_audio_slot_6,
        set_audio_slot_7,
        set_audio_slot_8,
        set_audio_slot_9,
        set_pan_stereo,
        set_pan_left_channel,
        set_pan_right_channel,
    }
}