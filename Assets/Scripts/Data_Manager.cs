using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;

public class Data_Manager : MonoBehaviour
{
    [SerializeField][Range(0, 1)] float rainPercent = 0;
    [SerializeField][Range(0, 1)] float snowPercent = 0;
    [SerializeField] Vector3 mapAnimatronicPlacementSpot;
    [SerializeField] float worldFlyingSphereSize;
    [SerializeField] SaveFileData saveFileData;
    [SerializeField] MapData mapData;
    [SerializeField] bool retryMap;

    Transform trueTraceScene;

    //Consts
    const int pickupLayer = 10;

    private void Awake()
    {
        trueTraceScene = GameObject.Find("Scene").transform;
        string saveFilePath = Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml";

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("You entered a map without a save file");
            SceneManager.LoadScene(0);
        }
        else
        {
            saveFileData = saveFileData.DeserializeFromXML(File.ReadAllText(saveFilePath));
        }

        if (saveFileData == null)
        {
            Debug.LogError("You entered a map with a corrupt save file");
            SceneManager.LoadScene(0);
        }

        string mapSavePath = Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml";
        if (!File.Exists(mapSavePath))
        {
            GenerateNewRandomMapSaveData();
        }
        else
        {
            mapData = mapData.DeserializeFromXML(File.ReadAllText(mapSavePath));
        }
        ApplySaveFileValuesToScene();
    }

    private void Update()
    {
        if (retryMap)
        {
            retryMap = false;
            GenerateNewRandomMapSaveData();
            ApplySaveFileValuesToScene();
        }
        saveFileData.timeElapsed += Time.deltaTime;
    }

    private void OnApplicationQuit()
    {
        SaveAllFiles();
    }

    public void SaveAllFiles()
    {
        //Check nulls
        if (mapData.animatronics == null) { mapData.animatronics = new List<Combo_Animatronic_SaveFile>(); }
        if (mapData.brushData == null) { mapData.brushData = new List<CustomBrushData>(); }
        if (mapData.propData == null) { mapData.propData = new List<CustomPropData>(); }

        GameObject g;
        for (int i = 0; i < mapData.animatronics.Count; i++)
        {
            g = GameObject.Find(mapData.animatronics[i].objectHash.ToString());
            if (g != null)
            {
                mapData.animatronics[i].position = g.transform.position;
                mapData.animatronics[i].rotation = g.transform.rotation;
            }
        }
        for (int i = 0; i < mapData.propData.Count; i++)
        {
            g = GameObject.Find(mapData.propData[i].objectHash.ToString());
            if (g != null)
            {
                MeshRenderer rend = g.GetComponentInChildren<MeshRenderer>();
                Material[] mats = rend.materials;
                if (mats.Length > 0)
                {
                    mapData.propData[i].position = g.transform.position;
                    mapData.propData[i].rotation = g.transform.rotation;
                    mapData.propData[i].colorA = mats[0].GetColor("_Palette_1");
                    mapData.propData[i].colorB = mats[0].GetColor("_Palette_2");
                    mapData.propData[i].colorC = mats[0].GetColor("_Palette_3");
                    mapData.propData[i].colorD = mats[0].GetColor("_Palette_4");
                }
            }
        }
        for (int i = 0; i < mapData.brushData.Count; i++)
        {
            g = GameObject.Find(mapData.brushData[i].objectHash.ToString());
            if (g != null)
            {
                MeshFilter r = g.GetComponent<MeshFilter>();
                if (r != null)
                {
                    mapData.brushData[i].vertices = r.mesh.vertices;
                    mapData.brushData[i].position = g.transform.position;
                    mapData.brushData[i].rotation = g.transform.rotation;

                    MeshRenderer mr = g.GetComponent<MeshRenderer>();
                    Material[] mats = mr.materials;
                    mapData.brushData[i].materials = new uint[mats.Length];
                    for (int e = 0; e < mats.Length; e++)
                    {
                        uint matname = 0;
                        string originalName = mats[e].name;
                        if (originalName.Contains(" (Instance)"))
                        {
                            originalName = originalName.Replace(" (Instance)", "").Trim();
                        }
                        uint.TryParse(originalName, out matname);
                        mapData.brushData[i].materials[e] = matname;
                    }
                }
            }
        }
        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml", saveFileData.SerializeToXML());
        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
    }

    public int GetSeed()
    {
        return saveFileData.worldSeed;
    }

    void ApplySaveFileValuesToScene()
    {
        bool addedNewGeoData = false;
        //Brushes
        if (mapData.brushData != null)
        {
            for (int i = 0; i < mapData.brushData.Count; i++)
            {
                //Setup
                GameObject brush = GameObject.Instantiate(Resources.Load<GameObject>("Brushes/Cube"));
                brush.transform.position = mapData.brushData[i].position;
                brush.transform.rotation = mapData.brushData[i].rotation;
                brush.name = mapData.brushData[i].objectHash.ToString();
                brush.transform.parent = trueTraceScene;

                MeshFilter filter = brush.GetComponent<MeshFilter>();
                filter.mesh.vertices = mapData.brushData[i].vertices;
                filter.mesh.RecalculateBounds();

                MeshCollider collider = brush.GetComponent<MeshCollider>();
                collider.sharedMesh = null;
                collider.sharedMesh = filter.mesh;

                if (mapData.brushData[i].materials != null)
                {
                    MeshRenderer renderer = brush.GetComponent<MeshRenderer>();
                    Material[] materials = renderer.materials;
                    for (int e = 0; e < mapData.brushData[i].materials.Length; e++)
                    {
                        if (mapData.brushData[i].materials[e] != 0)
                        {
                            materials[e] = Resources.Load<Material>("Materials/" + mapData.brushData[i].materials[e]);
                        }
                    }
                    renderer.materials = materials;
                }
            }
        }

        //Brushes
        if (mapData.propData != null)
        {
            for (int i = 0; i < mapData.propData.Count; i++)
            {
                //Setup
                GameObject prop = GameObject.Instantiate(Resources.Load<GameObject>("Props/" + mapData.propData[i].index));
                prop.transform.position = mapData.propData[i].position;
                prop.transform.rotation = mapData.propData[i].rotation;
                prop.transform.parent = trueTraceScene;
                prop.name = mapData.propData[i].objectHash.ToString();
                prop.gameObject.layer = pickupLayer;
                Component[] transforms = prop.GetComponentsInChildren(typeof(Transform), true);
                foreach (Transform transrights in transforms)
                {
                    transrights.gameObject.layer = pickupLayer;
                }

                MeshRenderer[] rend = prop.GetComponentsInChildren<MeshRenderer>();
                for (int e = 0; e < rend.Length; e++)
                {
                    Material[] mats = rend[e].materials;
                    for (int f = 0; f < mats.Length; f++)
                    {
                        mats[f].SetColor("_Palette_1", mapData.propData[i].colorA);
                        mats[f].SetColor("_Palette_2", mapData.propData[i].colorB);
                        mats[f].SetColor("_Palette_3", mapData.propData[i].colorC);
                        mats[f].SetColor("_Palette_4", mapData.propData[i].colorD);
                    }
                    rend[e].SetMaterials(mats.ToList());
                }
            }
        }

        //Animatronics
        int currentDTUIndex = 0;
        if (mapData.animatronics != null)
        {
            for (int i = 0; i < mapData.animatronics.Count; i++)
            {
                //Hacky DTU index system
                for (int e = 0; e < mapData.animatronics[i].comboParts.Count; e++)
                {
                    for (int j = 0; j < mapData.animatronics[i].comboParts[e].actuatorDTUIndexes.Count; j++)
                    {
                        mapData.animatronics[i].comboParts[e].actuatorDTUIndexes[j] = currentDTUIndex;
                        currentDTUIndex++;
                    }
                }
                //Setup
                GameObject animatronic = GameObject.Instantiate(new GameObject());
                animatronic.name = mapData.animatronics[i].objectHash.ToString();
                if (mapData.animatronics[i].yetToBePlaced)
                {
                    animatronic.transform.position = mapAnimatronicPlacementSpot;
                    mapData.animatronics[i].yetToBePlaced = false;
                    addedNewGeoData = true;
                }
                else
                {
                    animatronic.transform.position = mapData.animatronics[i].position;
                    animatronic.transform.rotation = mapData.animatronics[i].rotation;
                }
                animatronic.transform.parent = trueTraceScene;
                animatronic.AddComponent<Rigidbody>();
                animatronic.AddComponent<PhysicsObject>();
                Combo_Animatronic combo = animatronic.AddComponent<Combo_Animatronic>();
                combo.InsertDeadInterface(this.GetComponent<DEAD_Interface>());
                combo.ReassignFullSaveFile(mapData.animatronics[i]);
            }
        }
        if (addedNewGeoData)
        {
            DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
        }
    }

    void GenerateNewRandomMapSaveData()
    {
        mapData = new MapData();
        mapData.mapHistory = new List<MapHistory>();
        mapData.propData = new List<CustomPropData>();
        mapData.brushData = new List<CustomBrushData>();

        mapData.mapHistory.Add(new MapHistory()
        {
            change = PlayerPrefs.GetString("CreateWorldName"),
            eventType = MapHistory.HistoricalEvent.locationNameChange,
            time = saveFileData.timeFileStarted
        });
        Color[] palette = Name_Generator.GetRandomPalette(saveFileData.worldSeed);
        mapData.startingColorPrimary = palette[0];
        mapData.startingColorSecondary = palette[1];
        mapData.startingColorTertiary = palette[2];
        mapData.startingColorLight = palette[3];
        mapData.startingColorDark = palette[4];

        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
    }

    public float GetCurrentRainValue()
    {
        return rainPercent;
    }

    public float GetCurrentSnowValue()
    {
        return snowPercent;
    }

    public float GetWorldFlyingSphereSize()
    {
        return worldFlyingSphereSize;
    }

    public GameObject GenerateNewBrush(BrushType brushType, Vector3 position)
    {
        string hash = Random.Range(int.MinValue, int.MaxValue).ToString();
        GameObject brush = GameObject.Instantiate(Resources.Load<GameObject>("Brushes/Cube"));
        brush.name = hash;
        brush.transform.position = position;
        brush.transform.parent = trueTraceScene;

        CustomBrushData data = new CustomBrushData()
        {
            brushType = BrushType.block,
            objectHash = hash,
            colorA = Color.white,
            colorB = Color.white,
            colorC = Color.white,
            colorD = Color.white,
            materials = new uint[0],
            vertices = brush.GetComponent<MeshFilter>().mesh.vertices,
            position = brush.transform.position,
            rotation = brush.transform.rotation,
        };
        mapData.brushData.Add(data);
        return brush;
    }

    public GameObject GenerateNewProp(int index, Vector3 position)
    {
        string hash = Random.Range(int.MinValue, int.MaxValue).ToString();
        GameObject brush = GameObject.Instantiate(Resources.Load<GameObject>("Props/" + index));
        brush.name = hash;
        brush.transform.position = position;
        CustomPropData data = new CustomPropData()
        {
            index = index,
            objectHash = hash,
            colorA = Color.white,
            colorB = Color.white,
            colorC = Color.white,
            colorD = Color.white,
            position = brush.transform.position,
            rotation = brush.transform.rotation,
        };
        mapData.propData.Add(data);
        return brush;
    }

    public void RemoveBrushSaveData(string hashCode)
    {
        for (int i = 0; i < mapData.brushData.Count; i++)
        {
            if (mapData.brushData[i].objectHash == hashCode)
            {
                mapData.brushData.RemoveAt(i);
                break;
            }
        }
    }

    public void RemovePropSaveData(string hashCode)
    {
        for (int i = 0; i < mapData.propData.Count; i++)
        {
            if (mapData.propData[i].objectHash == hashCode)
            {
                mapData.propData.RemoveAt(i);
                break;
            }
        }
    }

    public void AssignNewProp(CustomPropData data)
    {
        mapData.propData.Add(data);
    }
}

[System.Serializable]
public struct DateTimeFloat
{
    public UDateTime time;
    public float value;
}

[System.Serializable, XmlRoot("Save File Data")]
public class SaveFileData
{
    public string firstName;
    public string lastName;
    public int money;
    public int worldSeed;
    public int currentMap;
    public UDateTime timeFileStarted;
    public float timeElapsed;
    public string SerializeToXML()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SaveFileData));
        using (System.IO.StringWriter writer = new System.IO.StringWriter())
        {
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }

    public SaveFileData DeserializeFromXML(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SaveFileData));
        using (System.IO.StringReader reader = new System.IO.StringReader(xml))
        {
            return (SaveFileData)serializer.Deserialize(reader);
        }
    }

    public static string GetMap(int id)
    {
        switch (id)
        {
            case 0:
                return "Blank Scene";
            default:
                return "";
        }
    }
}

[System.Serializable]
public class MapData
{
    public List<MapHistory> mapHistory;
    public List<CustomPropData> propData;
    public List<CustomBrushData> brushData;
    public List<Combo_Animatronic_SaveFile> animatronics;

    public Color startingColorPrimary;
    public Color startingColorSecondary;
    public Color startingColorTertiary;
    public Color startingColorLight;
    public Color startingColorDark;

    public string SerializeToXML()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(MapData));
        using (System.IO.StringWriter writer = new System.IO.StringWriter())
        {
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }

    public MapData DeserializeFromXML(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(MapData));
        using (System.IO.StringReader reader = new System.IO.StringReader(xml))
        {
            return (MapData)serializer.Deserialize(reader);
        }
    }
}

[System.Serializable]
public class MapHistory
{
    public string change;
    public UDateTime time;
    public HistoricalEvent eventType;

    public enum HistoricalEvent
    {
        locationNameChange,
        locationOwnerChange,
        buildingChange,
        closure,
        reopening,
    }
}

[System.Serializable]
public class CustomPropData
{
    public string objectHash;
    public int index;
    public Color colorA;
    public Color colorB;
    public Color colorC;
    public Color colorD;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class CustomBrushData
{
    public BrushType brushType;
    public string objectHash;
    public Color colorA;
    public Color colorB;
    public Color colorC;
    public Color colorD;
    public uint[] materials;
    public Vector3[] vertices;
    public Vector3 position;
    public Quaternion rotation;
}

public enum BrushType
{
    block,
}