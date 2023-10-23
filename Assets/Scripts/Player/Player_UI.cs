using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Player_UI : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] UIDocument document;

    [Header("Animation Curves")]
    [SerializeField] AnimationCurve hotkeyPress;
    [SerializeField] AnimationCurve hotkeyRelease;
    [SerializeField] AnimationCurve uiMove;

    [Header("Data")]
    [SerializeField] HotKeyIcons[] hotkeyIcons = new HotKeyIcons[10];

    float[] hotBarKeyScale = new float[10];
    VisualElement[] hotBarVisualElements = new VisualElement[10];

    //Consts
    const float hotBarKeyAnimationSpeed = 8;
    const float hotBarKeyminSize = 0.8f;
    const float hotBarKeyYOffset = 10f;

    void OnEnable()
    {
    }

    private void Start()
    {
        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            hotBarVisualElements[i] = document.rootVisualElement.Q<VisualElement>("Hotbar" + i);
        }

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
    }

    void PressHotbarKey(int number, bool down)
    {
        hotBarKeyScale[number] = Mathf.Clamp01(hotBarKeyScale[number] + ((down ? hotBarKeyAnimationSpeed : -hotBarKeyAnimationSpeed) * Time.deltaTime));
        hotBarVisualElements[number].style.scale = Vector2.Lerp(Vector2.one, Vector2.one * hotBarKeyminSize, down ? hotkeyPress.Evaluate(hotBarKeyScale[number]) : hotkeyRelease.Evaluate(hotBarKeyScale[number]));
        hotBarVisualElements[number].style.translate = new StyleTranslate() { value = new Translate() { y = Mathf.Lerp(0, hotBarKeyYOffset, down ? hotkeyPress.Evaluate(hotBarKeyScale[number]) : hotkeyRelease.Evaluate(hotBarKeyScale[number]))} };
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

    [System.Serializable]
    struct HotKeyIcons
    {
        public Texture2D icon;
        public bool flippedX;
    }

}
