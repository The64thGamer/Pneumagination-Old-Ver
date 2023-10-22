using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        var extensions = new[] {new ExtensionFilter("Audio Files", "wav", "aiff", "mp3","wma"),};
        string[] files = StandaloneFileBrowser.OpenFilePanel("Load Showtape Audio", "", extensions, false);
        if (files != null && files.Length != 0)
        {
            if (File.Exists(files[0]))
            {
                StartCoroutine(AudioDataCoroutine(files[0]));
            }
        }
    }

    IEnumerator AudioDataCoroutine(string path)
    {
        Debug.Log("Reading Audio");
        yield return null;
        byte[] filestream = File.ReadAllBytes(path);
        Debug.Log("Audio Read");
        yield return null;
        showtape.audioClips = new DEAD_ByteArray[] { new DEAD_ByteArray() { fileName = Path.GetFileName(path), array = filestream } };
        Debug.Log("Injected Into Showtape");
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
        string path = StandaloneFileBrowser.SaveFilePanel("Save Showtape File", "", "MyShowtape", new[] { new ExtensionFilter("Showtape Files", "showtape"), });
        if (path != "")
        {
            DEAD_Save_Load.SaveShowtape(path, showtape);
        }
    }

    void LoadFile()
    {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Load Showtape File", "", "showtape", false);
        if(files != null && files.Length != 0)
        {
            showtape = DEAD_Save_Load.LoadShowtape(files[0]);
        }

        name = showtape.name;
        author = showtape.author;
        description = showtape.description;
        animatronicsUsedFor = showtape.animatronicsUsedFor;
        endOfTapeTime = showtape.endOfTapeTime;
        filePath = showtape.filePath;
        timeCreated = showtape.timeCreated;
        timeLastUpdated = showtape.timeLastUpdated;

    }
}
