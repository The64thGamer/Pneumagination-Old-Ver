using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class DEAD_GUI_File_Saver : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] bool createNewFile;
    [SerializeField] bool saveFile;
    [SerializeField] bool loadFile;
    [SerializeField] bool clearData;
    [SerializeField] bool loadAudioData;

    [Header("Move To Interface")]
    [SerializeField] DEAD_Interface deadInterface;
    [SerializeField] bool injectIntoInterface;
    [SerializeField] int showtapeSlot;

    [Header("File Parameters")]
    [SerializeField] string name;
    [SerializeField] string author;
    [SerializeField] string description;
    [SerializeField] string animatronicsUsedFor;
    [SerializeField] float endOfTapeTime;

    [Header("Read Only Info")]
    [SerializeField] string filePath;
    [SerializeField] UDateTime timeCreated;
    [SerializeField] UDateTime timeLastUpdated;

    //Showtape
    DEAD_Showtape showtape = new DEAD_Showtape();

    private void Update()
    {
        if(injectIntoInterface)
        {
            injectIntoInterface = false;
            InjectIntoInterface();
        }
        if (saveFile)
        {
            saveFile = false;
            SaveFile(false);
        }
        if (createNewFile)
        {
            createNewFile = false;
            SaveFile(true);
        }
        if (loadFile)
        {
            loadFile = false;
            LoadFile();
        }
        if (clearData)
        {
            clearData = false;
            ClearData();
        }
        if (loadAudioData)
        {
            loadAudioData = false;
            InjectAudioData();
        }
    }

    void InjectAudioData()
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }
        StartCoroutine(InjectCoroutine());
    }

    IEnumerator InjectCoroutine()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Audio Files", "wav", "aiff", "mp3", "wma"));
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load Showtape Audio", "Load");

        if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length != 0)
        {
            if (File.Exists(FileBrowser.Result[0]))
            {
                byte[] filestream = File.ReadAllBytes(FileBrowser.Result[0]);
                showtape.audioClips = new DEAD_ByteArray[] { new DEAD_ByteArray() { fileName = Path.GetFileName(FileBrowser.Result[0]), array = filestream } };
            }
        }
    }

    void InjectIntoInterface()
    {
        if(deadInterface == null)
        {
            return;
        }
        deadInterface.SetShowtape(showtapeSlot, showtape);
    }

    void ClearData()
    {
        showtape = new DEAD_Showtape();
        name = showtape.name;
        author = showtape.author;
        description = showtape.description;
        animatronicsUsedFor = showtape.animatronicsUsedFor;
        endOfTapeTime = showtape.endOfTapeTime;
        filePath = showtape.filePath;
        timeCreated = showtape.timeCreated;
        timeLastUpdated = showtape.timeLastUpdated;
    }

    void SaveFile(bool newFileDate)
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }
        //Preload data
        showtape.name = name;
        showtape.author = author;
        showtape.description = description;
        showtape.animatronicsUsedFor = animatronicsUsedFor;
        showtape.endOfTapeTime = endOfTapeTime;
        if (newFileDate)
        {
            showtape.timeCreated = new UDateTime() { dateTime = DateTime.Now };
        }
        showtape.timeLastUpdated = new UDateTime() { dateTime = DateTime.Now };

        //Save

        StartCoroutine(SaveCoroutine());
    }

    IEnumerator SaveCoroutine()
    {
        //Save
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Showtape Files", ".showtape"));
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "Save Showtape File", "Save");

        if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length != 0)
        {
            DEAD_Save_Load.SaveShowtape(FileBrowser.Result[0], showtape);
        }
    }

    void LoadFile()
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }

        StartCoroutine(LoadCoroutine());

        name = showtape.name;
        author = showtape.author;
        description = showtape.description;
        animatronicsUsedFor = showtape.animatronicsUsedFor;
        endOfTapeTime = showtape.endOfTapeTime;
        filePath = showtape.filePath;
        timeCreated = showtape.timeCreated;
        timeLastUpdated = showtape.timeLastUpdated;

    }


    IEnumerator LoadCoroutine()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Showtape Files", ".showtape"));
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load Showtape File", "Load");

        if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length != 0)
        {
            showtape = DEAD_Save_Load.LoadShowtape(FileBrowser.Result[0]);
        }
    }
}
