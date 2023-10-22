using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Video;

public static class DEAD_Save_Load 
{
    public static DEAD_Showtape LoadShowtape(string filePath)
    {
        DEAD_Showtape showtape = JsonUtility.FromJson<DEAD_Showtape>(DecompressString(ReadFile(filePath)));
        showtape.filePath = filePath;
        return showtape;
    }

    public static void SaveShowtape(string filePath, DEAD_Showtape showtape)
    {
        showtape.filePath = "";
        if(showtape.layers != null)
        {
            for (int i = 0; i < showtape.layers.Length; i++)
            {
                showtape.layers[i].signals = CompressSignals(showtape.layers[i].signals);
            }
        }
        WriteFile(filePath,CompressString(JsonUtility.ToJson(showtape)));
    }

    public static List<DEAD_Signal_Data> CompressSignals(List<DEAD_Signal_Data> data)
    {
        List<DEAD_Signal_Data> newData = new List<DEAD_Signal_Data>();

        //FindDTUSize
        int maxSize = 0;
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].dtuIndex > maxSize)
            {
                maxSize = data[i].dtuIndex;
            }
        }
        float[] tempDTU = new float[maxSize+1];

        //Compare
        for (int i = 0; i < data.Count; i++)
        {
            bool canBeAdded = true;
            if (data[i].value == tempDTU[data[i].dtuIndex])
            {
                canBeAdded = false;
            }
            if (canBeAdded)
            {
                tempDTU[data[i].dtuIndex] = data[i].value;
                newData.Add(data[i]);
            }
        }
        return newData;
    }

    static bool WriteFile(string path, string data)
    {
        bool retValue;
        try
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, data);
            retValue = true;
        }
        catch (System.Exception ex)
        {
            string ErrorMessages = "File Write Error\n" + ex.Message;
            retValue = false;
            Debug.LogError(ErrorMessages);
        }
        return retValue;
    }

    static string ReadFile(string path)
    {
        string text = "";
        if (File.Exists(path))
        {
            // Read entire text file content in one string
            text = File.ReadAllText(path);
        }
        return text;
    }

    static string CompressString(string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        memoryStream.Position = 0;

        var compressedData = new byte[memoryStream.Length];
        memoryStream.Read(compressedData, 0, compressedData.Length);

        var gZipBuffer = new byte[compressedData.Length + 4];
        Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
        return Convert.ToBase64String(gZipBuffer);
    }

    static string DecompressString(string compressedText)
    {
        byte[] gZipBuffer = Convert.FromBase64String(compressedText);
        using (var memoryStream = new MemoryStream())
        {
            int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];

            memoryStream.Position = 0;
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                gZipStream.Read(buffer, 0, buffer.Length);
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}

[System.Serializable]
public class DEAD_Showtape
{
    [Header("Info")]
    public string name;
    public string author;
    public string description;
    public string animatronicsUsedFor;
    public string filePath;
    public UDateTime timeCreated;
    public UDateTime timeLastUpdated;

    [Header("A/V")]
    public float endOfTapeTime;
    [HideInInspector] public DEAD_ByteArray[] audioClips;
    [HideInInspector] public DEAD_ByteArray[] videoClips;

    [Header("Signals")]
    public DEAD_Showtape_Layers[] layers;
    [HideInInspector] public DEAD_ByteArray[] additionalBytes;
}

[System.Serializable]
public struct DEAD_ByteArray
{
    public string fileName;
    [HideInInspector]public byte[] array;
}

[System.Serializable]
public struct DEAD_Showtape_Layers
{
    public string triggerString;
    [HideInInspector] public List<DEAD_Signal_Data> signals;
    [HideInInspector] public List<DEAD_Command_Data> commands;
}

[System.Serializable]
public struct DEAD_Signal_Data
{
    public float time;
    public int dtuIndex;
    public float value;
}

[System.Serializable]
public struct DEAD_Command_Data
{
    public float time;
    public string value;
}