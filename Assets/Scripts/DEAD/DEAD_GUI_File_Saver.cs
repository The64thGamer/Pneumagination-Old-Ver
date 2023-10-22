using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEAD_GUI_File_Saver : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] bool saveFile;
    [SerializeField] bool loadFile;
    [SerializeField] bool clearData;

    [Header("Move To Interface")]
    [SerializeField] DEAD_Interface deadInterface;
    [SerializeField] bool injectIntoInterface;
    [SerializeField] int showtapeSlot;

    [Header("File")]
    [SerializeField] DEAD_Showtape showtape;

    private void Update()
    {
        if(injectIntoInterface)
        {
            injectIntoInterface = false;
            InjectIntoInterface();
        }
        if(saveFile)
        {
            saveFile = false;
            SaveFile();
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
    }

    void InjectIntoInterface()
    {
        if(deadInterface == null)
        {
            return;
        }
        
    }

    void ClearData()
    {
        showtape = new DEAD_Showtape();
    }

    void SaveFile()
    {
        //Preload data
        showtape.timeCreated.dateTime = DateTime.Now;
        showtape.timeLastUpdated.dateTime = DateTime.Now;

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
        if(files != null || files.Length != 0)
        {
            showtape = DEAD_Save_Load.LoadShowtape(files[0]);
        }
    }
}
