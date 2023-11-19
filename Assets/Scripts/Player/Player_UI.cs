using SimpleFileBrowser;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Player_UI : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;
    [SerializeField] DEAD_Interface deadInterface;
    [SerializeField] Data_Manager dataManager;
    [SerializeField] FirstPersonController controller;

    [Header("Animation Curves")]
    [SerializeField] AnimationCurve hotkeyPress;
    [SerializeField] AnimationCurve hotkeyRelease;
    [SerializeField] AnimationCurve uiMove;

    [Header("Data")]
    [SerializeField] HotKeyIcons[] hotkeyIcons = new HotKeyIcons[10];
    [SerializeField] int[] hotkeyDTUIndexes = new int[10];

    [Header("Recorded Inputs")]
    [SerializeField] List<DEAD_Signal_Data> signals = new List<DEAD_Signal_Data>();
    [SerializeField] List<DEAD_Command_Data> commands = new List<DEAD_Command_Data>();
    float[] dtuReplica;

    //UI Objects
    VisualElement[] hotBarVisualElements = new VisualElement[10];
    VisualElement showInfoPopup;
    VisualElement viewer;
    ProgressBar playbackBar;
    Label playbackTime;
    ProgressBar viewerPlaybackBar;
    Label viewerPlaybackTime;
    Label viewerShowtapeName;

    //UI Values
    float[] hotBarKeyScale = new float[10];
    float showInfoPopupPosition = 0;
    bool showInfoPopupMoving;
    float viewerPosition = 0;
    bool viewerMoving;

    //Objects
    [HideInInspector] DEAD_Showtape showtape;

    //Consts
    const float hotBarKeyAnimationSpeed = 8;
    const float hotBarKeyminSize = 0.8f;
    const float hotBarKeyYOffset = 10f;


    void OnEnable()
    {
        document.rootVisualElement.Q<Button>("LoadShow").clicked += () => LoadFile();
        document.rootVisualElement.Q<Button>("SaveShow").clicked += () => SaveFile();
        document.rootVisualElement.Q<Button>("ShowInfo").clicked += () => StartCoroutine(VisualizeShowInfoPopup(Convert.ToBoolean(showInfoPopupPosition)));
        document.rootVisualElement.Q<Button>("Shop").clicked += () => 
        {
            GameObject.Find("Data Manager").GetComponent<Data_Manager>().SaveAllFiles();
            SceneManager.LoadScene("Animatronic Creator"); 
        };
        document.rootVisualElement.Q<Button>("ClearAllData").clicked += () => CreateNewShowtape(false);
        document.rootVisualElement.Q<Button>("EditScreenBack").clicked += () => StartCoroutine(VisualizeShowInfoPopup(true));
        document.rootVisualElement.Q<Button>("BrowseAudio").clicked += () => InjectAudioData();
        document.rootVisualElement.Q<Button>("Play").clicked += () => SendCommand("Play");
        document.rootVisualElement.Q<Button>("Pause").clicked += () => SendCommand("Pause");
        document.rootVisualElement.Q<Button>("Rewind").clicked += () => SendCommand("Rewind");
        document.rootVisualElement.Q<TextField>("ShowtapeName").RegisterValueChangedCallback(UpdateShowtapeName);
        document.rootVisualElement.Q<TextField>("ShowtapeAuthor").RegisterValueChangedCallback(UpdateShowtapeAuthor);
        document.rootVisualElement.Q<TextField>("ShowtapeDescription").RegisterValueChangedCallback(UpdateShowDescription);

        if (deadInterface != null)
        {
            deadInterface.dtuSet.AddListener(DataSet);
            deadInterface.commandSetOnlyRecordables.AddListener(CommandSet);
        }
    }

    private void OnDisable()
    {
        if (deadInterface != null)
        {
            deadInterface.dtuSet.RemoveListener(DataSet);
            deadInterface.commandSetOnlyRecordables.RemoveListener(CommandSet);
        }
    }

    private void Start()
    {
        dataManager = GameObject.Find("Data Manager").GetComponent<Data_Manager>();

        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            hotBarVisualElements[i] = document.rootVisualElement.Q<VisualElement>("Hotbar" + i);
        }
        showInfoPopup = document.rootVisualElement.Q<VisualElement>("ShowInfoPopup");
        viewer = document.rootVisualElement.Q<VisualElement>("ViewerBar");
        playbackBar = document.rootVisualElement.Q<ProgressBar>("PlaybackBar");
        playbackTime = document.rootVisualElement.Q<Label>("PlaybackTime");
        viewerPlaybackBar = document.rootVisualElement.Q<ProgressBar>("ViewerPlaybackBar");
        viewerPlaybackTime = document.rootVisualElement.Q<Label>("ViewerPlaybackTime");
        viewerShowtapeName = document.rootVisualElement.Q<Label>("ViewerShowtapeName");
        if (deadInterface.GetShowtape(0) == null)
        {
            CreateNewShowtape(true);
        }
        dtuReplica = new float[deadInterface.GetDTUArrayLength()];

        UpdateHotbarIcons();

    }

    private void Update()
    {
        PressHotbarKey(0, Input.GetKey(KeyCode.Alpha1));
        PressHotbarKey(1, Input.GetKey(KeyCode.Alpha2));
        PressHotbarKey(2, Input.GetKey(KeyCode.Alpha3));
        PressHotbarKey(3, Input.GetKey(KeyCode.Alpha4));
        PressHotbarKey(4, Input.GetKey(KeyCode.Alpha5));
        PressHotbarKey(5, Input.GetKey(KeyCode.Alpha6));
        PressHotbarKey(6, Input.GetKey(KeyCode.Alpha7));
        PressHotbarKey(7, Input.GetKey(KeyCode.Alpha8));
        PressHotbarKey(8, Input.GetKey(KeyCode.Alpha9));
        PressHotbarKey(9, Input.GetKey(KeyCode.Alpha0));

        UpdateTabPositions();
        UpdatePlaybackBar();
        SendData();


        if (!controller.CheckifPlayerInMenu() && viewerPosition == 0)
        {
            StartCoroutine(VisualizeViewer(false));
        }
        if (controller.CheckifPlayerInMenu() && viewerPosition == 1)
        {
            StartCoroutine(VisualizeViewer(true));
        }
    }

    void SendData()
    {
        for (int i = 0; i < 10; i++)
        {
            int index = hotkeyDTUIndexes[i];
            int value = -1;
            switch (i)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.Alpha1)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha1)) { value = 0; }
                    break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.Alpha2)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha2)) { value = 0; }
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.Alpha3)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha3)) { value = 0; }
                    break;
                case 3:
                    if (Input.GetKeyDown(KeyCode.Alpha4)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha4)) { value = 0; }
                    break;
                case 4:
                    if (Input.GetKeyDown(KeyCode.Alpha5)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha5)) { value = 0; }
                    break;
                case 5:
                    if (Input.GetKeyDown(KeyCode.Alpha6)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha6)) { value = 0; }
                    break;
                case 6:
                    if (Input.GetKeyDown(KeyCode.Alpha7)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha7)) { value = 0; }
                    break;
                case 7:
                    if (Input.GetKeyDown(KeyCode.Alpha8)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha8)) { value = 0; }
                    break;
                case 8:
                    if (Input.GetKeyDown(KeyCode.Alpha9)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha9)) { value = 0; }
                    break;
                case 9:
                    if (Input.GetKeyDown(KeyCode.Alpha0)) { value = 1; }
                    if (Input.GetKeyUp(KeyCode.Alpha0)) { value = 0; }
                    break;
                default:
                    break;
            }
            if (value != -1)
            {
                deadInterface.SetData(index, value);
            }
        }
    }

    void SendCommand(string command)
    {
        if (!controller.CheckifPlayerInMenu())
        {
            return;
        }

        deadInterface.SendCommand(command, true);
        if (command == "Play")
        {
            ApplyRecordingToTape();
            StartCoroutine(UpdateShowMaxLength());
        }
        if (command == "Pause")
        {
            ApplyRecordingToTape();
        }
    }

    void UpdateShowtapeName(UnityEngine.UIElements.ChangeEvent<string> newName)
    {
        showtape.name = newName.newValue;
        UpdateShowtapeText();
    }
    void UpdateShowtapeAuthor(UnityEngine.UIElements.ChangeEvent<string> newName)
    {
        showtape.author = newName.newValue;
        UpdateShowtapeText();
    }
    void UpdateShowDescription(UnityEngine.UIElements.ChangeEvent<string> newName)
    {
        showtape.description = newName.newValue;
        UpdateShowtapeText();
    }

    IEnumerator UpdateShowMaxLength()
    {
        while (deadInterface.getLoadingState() == DEAD_Interface.LoadingState.loading)
        {
            yield return null;
        }
        AudioClip clip = deadInterface.GetAudioClip(0);
        if (clip != null)
        {
            showtape.endOfTapeTime = clip.length;
        }
    }

    void PressHotbarKey(int number, bool down)
    {
        hotBarKeyScale[number] = Mathf.Clamp01(hotBarKeyScale[number] + ((down ? hotBarKeyAnimationSpeed : -hotBarKeyAnimationSpeed) * Time.deltaTime));
        hotBarVisualElements[number].style.scale = Vector2.Lerp(Vector2.one, Vector2.one * hotBarKeyminSize, down ? hotkeyPress.Evaluate(hotBarKeyScale[number]) : hotkeyRelease.Evaluate(hotBarKeyScale[number]));
        hotBarVisualElements[number].style.translate = new StyleTranslate() { value = new Translate(0, Mathf.Lerp(0, hotBarKeyYOffset, down ? hotkeyPress.Evaluate(hotBarKeyScale[number]) : hotkeyRelease.Evaluate(hotBarKeyScale[number]))) };
    }

    void UpdatePlaybackBar()
    {
        playbackTime.text = TimeSpan.FromSeconds(deadInterface.GetCurrentTapeTime()).ToString(@"mm\:ss") + "/" + TimeSpan.FromSeconds(showtape.endOfTapeTime).ToString(@"mm\:ss");
        viewerPlaybackTime.text = playbackTime.text;
        playbackBar.highValue = showtape.endOfTapeTime;
        viewerPlaybackBar.highValue = playbackBar.highValue;
        playbackBar.value = deadInterface.GetCurrentTapeTime();
        viewerPlaybackBar.value = playbackBar.value;

    }

    void UpdateTabPositions()
    {
        showInfoPopup.style.translate = new StyleTranslate() { value = new Translate(0, Mathf.Lerp(830, 0, uiMove.Evaluate(showInfoPopupPosition))) };
        viewer.style.translate = new StyleTranslate() { value = new Translate(0, Mathf.Lerp(112, 0, uiMove.Evaluate(viewerPosition))) };
    }

    public void SetNewHotbarIcons(HotKeyIcons[] icons)
    {
        hotkeyIcons = icons;
        UpdateHotbarIcons();
    }

    void UpdateHotbarIcons()
    {
        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            VisualElement key = hotBarVisualElements[i].Q<VisualElement>("Icon");
            key.style.backgroundImage = hotkeyIcons[i].icon;
            key.style.scale = new Vector2(hotkeyIcons[i].flippedX ? -1 : 1, 1);
        }
    }

    void UpdateShowtapeText()
    {
        document.rootVisualElement.Q<TextField>("ShowtapeName").SetValueWithoutNotify(showtape.name);
        document.rootVisualElement.Q<TextField>("ShowtapeAuthor").SetValueWithoutNotify(showtape.author);
        document.rootVisualElement.Q<TextField>("ShowtapeDescription").SetValueWithoutNotify(showtape.description);
        viewerShowtapeName.text = showtape.name;
        Label fileName = document.rootVisualElement.Q<Label>("ShowAudioFileName");
        if (showtape.audioClips != null && showtape.audioClips.Length > 0 && showtape.audioClips[0].fileName != null)
        {
            if (showtape.audioClips[0].fileName == "")
            {
                fileName.text = "(Audio present with no name)";
            }
            else
            {
                fileName.text = showtape.audioClips[0].fileName;
            }
        }
        else
        {
            fileName.text = "No Show Audio";
        }
    }

    IEnumerator VisualizeShowInfoPopup(bool hide)
    {
        if (!showInfoPopupMoving && controller.CheckifPlayerInMenu())
        {
            showInfoPopupMoving = true;
            if (hide)
            {
                while (showInfoPopupPosition > 0)
                {
                    showInfoPopupPosition -= Time.deltaTime * 8;
                    yield return null;
                }
                showInfoPopupPosition = 0;
            }
            else
            {
                while (showInfoPopupPosition < 1)
                {
                    showInfoPopupPosition += Time.deltaTime * 8;
                    yield return null;
                }
                showInfoPopupPosition = 1;
            }
            showInfoPopupMoving = false;
        }
    }

    IEnumerator VisualizeViewer(bool hide)
    {
        if (!viewerMoving)
        {
            viewerMoving = true;
            if (hide)
            {
                while (viewerPosition > 0)
                {
                    viewerPosition -= Time.deltaTime * 8;
                    yield return null;
                }
                viewerPosition = 0;
            }
            else
            {
                while (viewerPosition < 1)
                {
                    viewerPosition += Time.deltaTime * 8;
                    yield return null;
                }
                viewerPosition = 1;
            }
            viewerMoving = false;
        }
    }

    void InjectAudioData()
    {
        if (!controller.CheckifPlayerInMenu() || FileBrowser.IsOpen)
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
                deadInterface.SetShowtape(0, showtape);
                UpdateShowtapeText();
            }
        }
    }

    void CreateNewShowtape(bool overridePlayerCheck)
    {
        if (!controller.CheckifPlayerInMenu() && !overridePlayerCheck)
        {
            return;
        }

        showtape = new DEAD_Showtape();
        showtape.name = "My Showtape";
        showtape.author = "Me";
        showtape.description = "A showtape.";
        showtape.timeCreated = new UDateTime() { dateTime = DateTime.Now };
        deadInterface.SetShowtape(0, showtape);
        signals = new List<DEAD_Signal_Data>();
        commands = new List<DEAD_Command_Data>();
        dtuReplica = new float[deadInterface.GetDTUArrayLength()];
        deadInterface.SetShowtape(0, showtape);
        UpdateShowtapeText();
    }

    void SaveFile()
    {
        if (!controller.CheckifPlayerInMenu() || FileBrowser.IsOpen)
        {
            return;
        }
        StartCoroutine(SaveCoroutine());

    }
    IEnumerator SaveCoroutine()
    {
        ApplyRecordingToTape();

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
        if (!controller.CheckifPlayerInMenu() || FileBrowser.IsOpen)
        {
            return;
        }
        StartCoroutine(LoadCoroutine());
    }

    IEnumerator LoadCoroutine()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Showtape Files", ".showtape"));
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load Showtape File", "Load");

        if (FileBrowser.Success && FileBrowser.Result != null && FileBrowser.Result.Length != 0)
        {
            showtape = DEAD_Save_Load.LoadShowtape(FileBrowser.Result[0]);
            deadInterface.SetShowtape(0, showtape);
            UpdateShowtapeText();
        }
    }

    void DataSet(int index, float time, float value)
    {
        if (index > dtuReplica.Length - 1)
        {
            return;
        }

        if (dtuReplica[index] != value)
        {
            dtuReplica[index] = value;
            signals.Add(new DEAD_Signal_Data() { dtuIndex = index, time = time, value = value });
        }
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

        if (showtape.layers == null || showtape.layers.Length == 0)
        {
            showtape.layers = new DEAD_Showtape_Layers[] { new DEAD_Showtape_Layers() };
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

        signals = new List<DEAD_Signal_Data>();
        commands = new List<DEAD_Command_Data>();
        dtuReplica = new float[deadInterface.GetDTUArrayLength()];
    }

    public bool CheckIfCanExitMenu()
    {
        if (showInfoPopupPosition > 0)
        {
            return false;
        }
        return true;
    }
}

[System.Serializable]
public struct HotKeyIcons
{
    public Texture2D icon;
    public bool flippedX;
}
