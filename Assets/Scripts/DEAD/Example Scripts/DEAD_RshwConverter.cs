using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;

public class DEAD_RshwConverter : MonoBehaviour
{
    [Header("WARNING:\n .rshw and equivalent files use the deprecated\n " +
        "BinaryFormatter library, which is now deemed as a\n" +
        " vulnerable format for attacks. Make sure you\n" +
        " are loading these from a trusted source.\n")]
    [SerializeField] DEAD_Interface deadInterface;
    [SerializeField] bool convertRshwAndInjectIntoInterface;
    [SerializeField] bool saveFile;

    private void Update()
    {
        if (convertRshwAndInjectIntoInterface)
        {
            convertRshwAndInjectIntoInterface = false;
            ConvertRshw();
        }
        if (saveFile)
        {
            saveFile = false;
            SaveFile(false);
        }
    }

    void SaveFile(bool newFileDate)
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }


        StartCoroutine(SaveCoroutine(newFileDate));
    }

    IEnumerator SaveCoroutine(bool newFileDate)
    {
        DEAD_Showtape showtape = deadInterface.GetShowtape(0);
        //Preload data
        if (newFileDate)
        {
            showtape.timeCreated = new UDateTime() { dateTime = DateTime.Now };
        }
        showtape.timeLastUpdated = new UDateTime() { dateTime = DateTime.Now };
        //Save
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Showtape Files", ".showtape"));
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "Save Showtape File", "Save");

        if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length != 0)
        {
            DEAD_Save_Load.SaveShowtape(FileBrowser.Result[0], showtape);
        }
    }

    void ConvertRshw()
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }
        StartCoroutine(ConvertRshwCoroutine());
    }
    IEnumerator ConvertRshwCoroutine()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("RR Engine Showtapes", ".cshw", ".sshw", ".rshw", ".nshw", ".fshw", ".tshw", ".mshw"));
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load RR Engine Showtape File", "Load");

        if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length != 0)
        {
            rshwFormat oldShow;
            oldShow = rshwFormat.ReadFromFile(FileBrowser.Result[0]);

            DEAD_Showtape showtape = new DEAD_Showtape();

            string[] combined = Path.GetFileNameWithoutExtension(FileBrowser.Result[0]).Split(new string[] { " - " }, StringSplitOptions.None);
            showtape.name = combined[0];
            if (combined.Length > 1)
            {
                showtape.author = combined[1];
            }

            if (oldShow.audioData != null && oldShow.audioData.Length > 0)
            {
                showtape.audioClips = new DEAD_ByteArray[1];
                showtape.audioClips[0] = new DEAD_ByteArray() { array = oldShow.audioData };
            }
            if (oldShow.videoData != null && oldShow.videoData.Length > 0)
            {
                showtape.videoClips = new DEAD_ByteArray[1];
                showtape.videoClips[0] = new DEAD_ByteArray() { array = oldShow.videoData };
            }

            showtape.layers = new DEAD_Showtape_Layers[1];
            showtape.layers[0] = new DEAD_Showtape_Layers() { commands = new List<DEAD_Command_Data>(), signals = new List<DEAD_Signal_Data>() };
            int time = 0;
            List<int> previousIndexes = new List<int>();
            List<int> currentIndexes = new List<int>();
            for (int i = 0; i < oldShow.signalData.Length; i++)
            {
                if (oldShow.signalData[i] == 0)
                {
                    time++;
                    for (int e = 0; e < previousIndexes.Count; e++)
                    {
                        bool foundIt = false;
                        for (int j = 0; j < currentIndexes.Count; j++)
                        {
                            if (previousIndexes[e] == currentIndexes[j])
                            {
                                foundIt = true;
                            }
                        }
                        if (!foundIt)
                        {
                            showtape.layers[0].signals.Add(new DEAD_Signal_Data() { dtuIndex = previousIndexes[e], time = time / 60.0f, value = 0 });
                        }
                    }
                    previousIndexes = currentIndexes;
                    currentIndexes = new List<int>();
                }
                else
                {
                    int index = oldShow.signalData[i] - 1;
                    if (oldShow.signalData[i] >= 151)
                    {
                        index -= 22;
                    }
                    currentIndexes.Add(index);
                    showtape.layers[0].signals.Add(new DEAD_Signal_Data() { dtuIndex = index, time = time / 60.0f, value = 1 });
                }
            }
            showtape.layers[0].signals = DEAD_Save_Load.CompressSignals(showtape.layers[0].signals);
            deadInterface.SetShowtape(0, showtape);
        }
    }
}


[System.Serializable]
public class rshwFormat
{
    public byte[] audioData { get; set; }
    public int[] signalData { get; set; }
    public byte[] videoData { get; set; }

    public void Save(string filePath)
    {
        var formatter = new BinaryFormatter();
        using (var stream = File.Open(filePath, FileMode.Create))
            formatter.Serialize(stream, this);
    }
    public static rshwFormat ReadFromFile(string filepath)
    {
        var formatter = new BinaryFormatter();
        using (var stream = File.OpenRead(filepath))
            if (stream.Length != 0)
            {
                stream.Position = 0;
                try
                {
                    return (rshwFormat)formatter.Deserialize(stream);
                }
                catch (System.Exception)
                {
                    return null;
                    throw;
                }

            }
            else
            {
                return null;
            }
    }
}