using System.Drawing;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DEAD_Interface))]
public class DEAD_Window_Interface : Editor
{
    DEAD_Interface dead_Interface;
    
    void OnEnable()
    {
        dead_Interface = target as DEAD_Interface;
    }

    public override void OnInspectorGUI()
    {
        int widthHeight = Mathf.CeilToInt(Mathf.Sqrt(dead_Interface.GetDTUArrayLength()));
        if (widthHeight > 0)
        {
            Texture2D texture = new Texture2D(widthHeight, widthHeight);
            texture.filterMode = FilterMode.Point;
            Color32[] textureColors = new Color32[widthHeight * widthHeight];
            int index = 0;
            while (index < textureColors.Length)
            {
                int row = widthHeight - 1 - (index % widthHeight);
                int column = Mathf.FloorToInt(index / widthHeight);
                int i = row + (column * widthHeight);

                textureColors[i] = UnityEngine.Color.white * ((dead_Interface.GetData((widthHeight * widthHeight) - index - 1) + 1) / 2);

                index++;
            }
            texture.SetPixels32(textureColors);
            texture.Apply();
            EditorGUILayout.LabelField("DTU Visualization", EditorStyles.boldLabel);
            GUILayout.Label("", GUILayout.Height(EditorGUIUtility.currentViewWidth * 0.9f), GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.9f));
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
        }
        serializedObject.Update();

        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
