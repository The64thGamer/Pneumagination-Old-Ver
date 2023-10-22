using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEAD_Recorder : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] DEAD_Interface deadInterface;

    [Header("Actions")]
    [SerializeField] bool applyRecordingToTape;
    [SerializeField] bool saveTapeToFile;

    [Header("Data")]
    [SerializeField] List<DEAD_Signal_Data> signals = new List<DEAD_Signal_Data>();
    [SerializeField] List<DEAD_Command_Data> commands = new List<DEAD_Command_Data>();

    private void Update()
    {
        if (applyRecordingToTape)
        {
            applyRecordingToTape = false;
            ApplyRecordingToTape();
        }
        if (saveTapeToFile)
        {
            saveTapeToFile = false;
            SaveTapeToFile();
        }
    }

    void DataSet(int index, float time, float value)
    {
        signals.Add(new DEAD_Signal_Data() { dtuIndex = index, time = time, value = value });
    }

    void CommandSet(float time, string value)
    {
        commands.Add(new DEAD_Command_Data() { time = time, value = value });
    }

    void ApplyRecordingToTape()
    {
        if (deadInterface == null)
        {
            return;
        }

        DEAD_Showtape showtape = deadInterface.GetShowtape(0);

        if (showtape != null)
        {
            if (showtape.layers == null || showtape.layers.Length == 0)
            {
                showtape.layers = new DEAD_Showtape_Layers[] { new DEAD_Showtape_Layers()};
            }

            //Signals
            if (showtape.layers[0].signals == null)
            {
                showtape.layers[0].signals = new List<DEAD_Signal_Data>();
            }
            for (int i = 0; i < signals.Count; i++)
            {
                showtape.layers[0].signals.Add(signals[i]);
            }
            showtape.layers[0].signals.Sort((x, y) => x.time.CompareTo(y.time));

            //Commands
            if (showtape.layers[0].commands == null)
            {
                showtape.layers[0].commands = new List<DEAD_Command_Data>();
            }
            for (int i = 0; i < commands.Count; i++)
            {
                showtape.layers[0].commands.Add(commands[i]);
            }
            showtape.layers[0].commands.Sort((x, y) => x.time.CompareTo(y.time));
        }
    }

    void SaveTapeToFile()
    {
        DEAD_Showtape showtape = deadInterface.GetShowtape(0);

        if (showtape != null)
        {
            //Preload data
            showtape.timeLastUpdated.dateTime = DateTime.Now;

            //Save
            string path = StandaloneFileBrowser.SaveFilePanel("Save Showtape File", "", "MyShowtape", new[] { new ExtensionFilter("Showtape Files", "showtape"), });
            if (path != "")
            {
                DEAD_Save_Load.SaveShowtape(path, showtape);
            }
        }
    }

    private void OnEnable()
    {
        if (deadInterface != null)
        {
            deadInterface.dtuSet.AddListener(DataSet);
            deadInterface.commandSet.AddListener(CommandSet);
        }
    }

    private void OnDisable()
    {
        if (deadInterface != null)
        {
            deadInterface.dtuSet.RemoveListener(DataSet);
            deadInterface.commandSet.RemoveListener(CommandSet);
        }
    }
}
