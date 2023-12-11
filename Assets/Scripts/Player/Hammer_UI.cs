using NUnit.Framework;
using NUnit.Framework.Internal;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Hammer_UI : MonoBehaviour
{
    [SerializeField] FirstPersonController fpc;
    [SerializeField] PlayerInteractions playerInteractions;
    [SerializeField] UIDocument document;
    [SerializeField] LayerMask pointerMask;
    [SerializeField] LayerMask propMask;
    [SerializeField] Color[] paintColors;


    //Data
    Data_Manager dataManager;
    VisualElement[] hotBarVisualElements = new VisualElement[10];
    MeshFilter currentMesh;
    MeshCollider currentCollider;
    Texture2D texture;
    List<Vector2> previousDrawnPixels;
    List<int> currentVertexes = new List<int>();
    Color lineColor;

    //States
    bool isSelected;
    int currentMode;
    int currentMaterial;
    int currentPropLayerSelected;
    int currentPropColor;

    //Consts
    const int maxMaterialSlots = 11;
    const float minVertexDistance = 0.1f;
    const float minLineDistance = 0.1f;
    const float maxPickingDistance = 10.0f;
    Color highlightColor = new Color(1, 0.86666666666f, 0);
    Color selectColor = new Color(0, 1, 0.29803921568f);
    Color paperTextColor = new Color(0.11764705882352941f, 0.12941176470588237f, 0.11764705882352941f);
    Color paperBackColor = new Color(0.9490196078431372f, 0.9372549019607843f, 0.8941176470588236f);


    private void Start()
    {
        playerInteractions.enabled = false;
        dataManager = GameObject.Find("Data Manager").GetComponent<Data_Manager>();
        previousDrawnPixels = new List<Vector2>();
        texture = new Texture2D(Screen.width, Screen.height);
        //Clear texture
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }
        document.rootVisualElement.Q<VisualElement>("Vis").style.backgroundImage = texture;
        ApplyRenderState(false);
        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            hotBarVisualElements[i] = document.rootVisualElement.Q<VisualElement>("Hotbar" + i);
        }
        PressHotbarKey(0, true);
        document.rootVisualElement.Q<VisualElement>("Paint").style.unityBackgroundImageTintColor = paintColors[currentPropColor];
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

        switch (currentMode)
        {
            case 0:
                playerInteractions.enabled = false;
                if (Input.GetMouseButtonDown(0))
                {
                    CreateNewBrush();
                }
                break;
            case 1:
                playerInteractions.enabled = true;
                ApplyRenderState(isSelected);
                if (Input.GetMouseButtonDown(0))
                {
                    ApplyRenderState(!isSelected);
                }
                if (Input.GetMouseButtonDown(1))
                {
                    ApplyRenderState(!isSelected);

                    if (isSelected && currentMesh != null)
                    {
                        currentVertexes = new List<int>();
                        for (int i = 0; i < currentMesh.mesh.triangles.Length; i++)
                        {
                            currentVertexes.Add(currentMesh.mesh.triangles[i]);
                        }
                        ApplyRenderState(isSelected);
                    }
                }
                SelectPoint();
                break;
            case 2:
                playerInteractions.enabled = false;
                if (Input.GetMouseButtonDown(0))
                {
                    ApplyMaterialToBrushFace();
                }
                if (Input.GetMouseButtonDown(1))
                {
                    currentMaterial = (currentMaterial + 1) % maxMaterialSlots;
                }
                break;
            case 3:
                playerInteractions.enabled = false;
                if (Input.GetMouseButtonDown(0))
                {
                    PaintProp();
                }
                if (Input.GetMouseButtonDown(1))
                {
                    currentPropLayerSelected = (currentPropLayerSelected + 1) % 4;
                }
                if (Input.GetMouseButtonDown(2))
                {
                    currentPropColor = (currentPropColor + 1) % paintColors.Length;
                    document.rootVisualElement.Q<VisualElement>("Paint").style.unityBackgroundImageTintColor = paintColors[currentPropColor];
                }
                break;
            case 4:
                playerInteractions.enabled = false;
                SelectPoint();
                ApplyRenderState(isSelected);

                if (currentMesh != null)
                {
                    if (currentVertexes.Count < currentMesh.mesh.triangles.Length)
                    {
                        currentVertexes = new List<int>();
                        for (int i = 0; i < currentMesh.mesh.triangles.Length; i++)
                        {
                            currentVertexes.Add(currentMesh.mesh.triangles[i]);
                        }
                        ApplyRenderState(isSelected);
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    ApplyRenderState(!isSelected);
                }
                if (isSelected && currentVertexes.Count > 0)
                {
                    dataManager.RemoveBrushSaveData(currentMesh.name);
                    Destroy(currentMesh.gameObject);
                    DisableSelection();
                }
                break;
            default:
                break;
        }


        if (isSelected && currentVertexes.Count > 0)
        {
            Vector3 translate = Vector3.zero;

            if (Input.mouseScrollDelta.y != 0)
            {
                Vector3 cameraForward = Camera.main.transform.forward;
                float dotX = Vector3.Dot(cameraForward, Vector3.right);
                float dotY = Vector3.Dot(cameraForward, Vector3.up);
                float dotZ = Vector3.Dot(cameraForward, Vector3.forward);

                float maxDot = Mathf.Max(Mathf.Abs(dotX), Mathf.Abs(dotY), Mathf.Abs(dotZ));

                if (Mathf.Abs(dotX) == maxDot)
                {
                    translate = Vector3.right * Mathf.Sign(dotX) * Mathf.Sign(Input.mouseScrollDelta.y) * 0.5f;
                }
                else if (Mathf.Abs(dotY) == maxDot)
                {
                    translate = Vector3.up * Mathf.Sign(dotY) * Mathf.Sign(Input.mouseScrollDelta.y) * 0.5f;
                }
                else
                {
                    translate = Vector3.forward * Mathf.Sign(dotZ) * Mathf.Sign(Input.mouseScrollDelta.y) * 0.5f;
                }
            }
            if (translate != Vector3.zero)
            {
                Vector3[] vertices = currentMesh.mesh.vertices;
                List<int> newVertexes = currentVertexes.ToList<int>();

                for (int i = 0; i < vertices.Length; i++)
                {
                    for (int e = 0; e < newVertexes.Count; e++)
                    {
                        if (vertices[i].Equals(vertices[newVertexes[e]]) && i != newVertexes[e])
                        {
                            newVertexes.Add(i);
                        }
                    }
                }
                newVertexes = newVertexes.Distinct().ToList();

                for (int i = 0; i < newVertexes.Count; i++)
                {
                    vertices[newVertexes[i]] += currentMesh.transform.InverseTransformPoint(currentMesh.transform.position + translate);
                }

                //Double check Size
                Vector3 minSize = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 maxSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i].x > maxSize.x) { maxSize.x = vertices[i].x; }
                    if (vertices[i].y > maxSize.y) { maxSize.y = vertices[i].y; }
                    if (vertices[i].z > maxSize.z) { maxSize.z = vertices[i].z; }

                    if (vertices[i].x < minSize.x) { minSize.x = vertices[i].x; }
                    if (vertices[i].y < minSize.y) { minSize.y = vertices[i].y; }
                    if (vertices[i].z < minSize.z) { minSize.z = vertices[i].z; }
                }
                if (!Mathf.Approximately(0, Math.Abs(minSize.x - maxSize.x)) && !Mathf.Approximately(0, Math.Abs(minSize.y - maxSize.y)) && !Mathf.Approximately(0, Math.Abs(minSize.z - maxSize.z)))
                {
                    //Apply
                    currentMesh.mesh.vertices = vertices;
                    currentMesh.mesh.RecalculateBounds();
                    currentCollider.sharedMesh = null;
                    currentCollider.sharedMesh = currentMesh.mesh;
                }
            }
        }
    }
    void SelectPoint()
    {
        if (isSelected)
        {
            return;
        }

        RaycastHit hit;
        Ray ray = new Ray() { origin = Camera.main.transform.position, direction = Camera.main.transform.forward };

        if (Physics.Raycast(ray, out hit, maxPickingDistance, pointerMask))
        {
            MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();

            if (meshFilter != null && hit.transform.tag == "Buildable Block")
            {
                currentMesh = meshFilter;
                currentCollider = hit.collider.GetComponent<MeshCollider>();
                //Find closest Vertex
                int closestVertex = -1;
                float pointCloseness = float.MaxValue;
                for (int i = 0; i < meshFilter.mesh.vertices.Length; i++)
                {
                    float distance = Vector3.Distance(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[i]), hit.point);
                    if (distance < pointCloseness)
                    {
                        pointCloseness = distance;
                        closestVertex = i;
                    }
                }

                if (pointCloseness <= minVertexDistance)
                {
                    currentVertexes = new List<int> { closestVertex };
                }
                else
                {
                    int closestEdgea = -1;
                    int closestEdgeb = -1;
                    int closestEdgec = -1;
                    pointCloseness = float.MaxValue;

                    //Edge 0
                    float distance = Vector3.Distance(NearestPointOnFiniteLine(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[hit.triangleIndex * 3]]), hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[hit.triangleIndex * 3 + 1]]), hit.point), hit.point);
                    if (distance < pointCloseness)
                    {
                        pointCloseness = distance;
                        closestEdgea = hit.triangleIndex * 3;
                        closestEdgeb = hit.triangleIndex * 3 + 1;
                        closestEdgec = hit.triangleIndex * 3 + 2;
                    }
                    //Edge 1
                    distance = Vector3.Distance(NearestPointOnFiniteLine(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[hit.triangleIndex * 3 + 1]]), hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[hit.triangleIndex * 3 + 2]]), hit.point), hit.point);
                    if (distance < pointCloseness)
                    {
                        pointCloseness = distance;
                        closestEdgea = hit.triangleIndex * 3 + 1;
                        closestEdgeb = hit.triangleIndex * 3 + 2;
                        closestEdgec = hit.triangleIndex * 3;
                    }
                    //Edge 2
                    distance = Vector3.Distance(NearestPointOnFiniteLine(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[hit.triangleIndex * 3 + 2]]), hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[hit.triangleIndex * 3]]), hit.point), hit.point);
                    if (distance < pointCloseness)
                    {
                        pointCloseness = distance;
                        closestEdgea = hit.triangleIndex * 3 + 2;
                        closestEdgeb = hit.triangleIndex * 3;
                        closestEdgec = hit.triangleIndex * 3 + 1;
                    }

                    if (pointCloseness <= minLineDistance)
                    {
                        currentVertexes = new List<int>
                        {
                            meshFilter.mesh.triangles[closestEdgea],
                            meshFilter.mesh.triangles[closestEdgeb]
                        };
                    }
                    else
                    {
                        Plane plane = new Plane(
                            meshFilter.mesh.vertices[meshFilter.mesh.triangles[closestEdgea]],
                            meshFilter.mesh.vertices[meshFilter.mesh.triangles[closestEdgeb]],
                            meshFilter.mesh.vertices[meshFilter.mesh.triangles[closestEdgec]]
                            );
                        Vector3 norm = plane.normal;

                        currentVertexes = new List<int>{
                            meshFilter.mesh.triangles[closestEdgea],
                            meshFilter.mesh.triangles[closestEdgeb],
                            meshFilter.mesh.triangles[closestEdgec] };

                        for (int i = 0; i < meshFilter.mesh.triangles.Length; i = i + 3)
                        {
                            plane = new Plane(
                                meshFilter.mesh.vertices[meshFilter.mesh.triangles[i]],
                                meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 1]],
                                meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 2]]
                                );
                            if (plane.normal == norm)
                            {
                                currentVertexes.Add(meshFilter.mesh.triangles[i]);
                                currentVertexes.Add(meshFilter.mesh.triangles[i + 1]);
                                currentVertexes.Add(meshFilter.mesh.triangles[i + 2]);
                            }
                        }
                        ApplyRenderState(false);
                    }
                }
            }
        }
        else
        {
            DisableSelection();
        }
    }

    private void LateUpdate()
    {
        RenderLines();
    }

    void ApplyRenderState(bool on)
    {
        if (currentMode == 4)
        {
            lineColor = Color.red;
        }
        else
        {
            lineColor = highlightColor;
        }

        if (currentMesh == null)
        {
            isSelected = false;
            currentVertexes = new List<int>();
            return;
        }

        isSelected = on;
        fpc.SetFOV(!on);
        if (on)
        {
            lineColor = selectColor;
        }
    }

    void RenderLines()
    {
        //Clear texture
        for (int i = 0; i < previousDrawnPixels.Count; i++)
        {
            texture.SetPixel((int)previousDrawnPixels[i].x, (int)previousDrawnPixels[i].y, Color.clear);
        }
        previousDrawnPixels.Clear();
        if (currentMesh == null)
        {
            texture.Apply();
            return;
        }

        if (currentVertexes.Count == 1)
        {
            Vector3 screenPosa = Camera.main.WorldToScreenPoint(currentMesh.transform.TransformPoint(currentMesh.mesh.vertices[currentVertexes[0]]));
            DrawCircle(lineColor, (int)screenPosa.x, (int)screenPosa.y, 5);
        }
        else
        {
            for (int i = 0; i < currentVertexes.Count; i++)
            {
                if (i + 1 >= currentVertexes.Count)
                {
                    break;
                }
                Vector3 screenPosa = Camera.main.WorldToScreenPoint(currentMesh.transform.TransformPoint(currentMesh.mesh.vertices[currentVertexes[i]]));
                Vector3 screenPosb = Camera.main.WorldToScreenPoint(currentMesh.transform.TransformPoint(currentMesh.mesh.vertices[currentVertexes[i + 1]]));
                if (screenPosa.z > 0 && screenPosb.z > 0)
                {
                    DrawLine(screenPosa, screenPosb, lineColor);
                }
            }
        }
        texture.Apply();

    }
    void DrawCircle(Color color, int x, int y, int radius = 3)
    {
        float rSquared = radius * radius;

        for (int u = x - radius; u < x + radius + 1; u++)
        {
            for (int v = y - radius; v < y + radius + 1; v++)
            {
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                {
                    if (u > 0 && u < texture.width && v > 0 && v < texture.height)
                    {
                        texture.SetPixel(u, v, color);
                        previousDrawnPixels.Add(new Vector2(u, v));
                    }
                }
            }
        }
    }
    void DrawLine(Vector2 start, Vector2 end, Color32 color)
    {
        int w = (int)(end.x - start.x);
        int h = (int)(end.y - start.y);
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            if (start.x > 0 && start.x < texture.width && start.y > 0 && start.y < texture.height)
            {
                texture.SetPixel((int)start.x, (int)start.y, color);
                previousDrawnPixels.Add(new Vector2(start.x, start.y));
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                start.x += dx1;
                start.y += dy1;
            }
            else
            {
                start.x += dx2;
                start.y += dy2;
            }
        }
    }

    Vector3 NearestPointOnFiniteLine(Vector3 origin, Vector3 end, Vector3 point)
    {
        Vector3 line_direction = end - origin;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - origin, line_direction), 0f, line_length);
        return origin + line_direction * project_length;
    }

    void CreateNewBrush()
    {
        RaycastHit hit;
        Ray ray = new Ray() { origin = Camera.main.transform.position, direction = Camera.main.transform.forward };

        if (Physics.Raycast(ray, out hit, maxPickingDistance, pointerMask))
        {
            Vector3 objectPos = new Vector3(Mathf.Round(hit.point.x * 2) / 2, Mathf.Round(hit.point.y * 2) / 2, Mathf.Round(hit.point.z * 2) / 2);
            dataManager.GenerateNewBrush(BrushType.block, objectPos);
        }
    }

    void DisableSelection()
    {
        currentMesh = null;
        currentCollider = null;
        currentVertexes = new List<int>();
        isSelected = false;
    }

    void PressHotbarKey(int number, bool down)
    {
        if (!down)
        {
            return;
        }
        for (int i = 0; i < hotBarVisualElements.Length; i++)
        {
            hotBarVisualElements[i].Q<VisualElement>("Icon").style.backgroundColor = paperBackColor;
            hotBarVisualElements[i].Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = paperTextColor;
            hotBarVisualElements[i].Q<Label>().style.color = paperTextColor;
            hotBarVisualElements[i].Q<Label>().style.unityTextOutlineColor = paperBackColor;
        }
        hotBarVisualElements[number].Q<VisualElement>("Icon").style.backgroundColor = paperTextColor;
        hotBarVisualElements[number].Q<VisualElement>("Icon").style.unityBackgroundImageTintColor = paperBackColor;
        hotBarVisualElements[number].Q<Label>().style.color = paperBackColor;
        hotBarVisualElements[number].Q<Label>().style.unityTextOutlineColor = paperTextColor;
        currentMode = number;
        DisableSelection();
    }

    int GetSubMeshIndex(int triangleIndex)
    {
        int triangleCounter = 0;
        for (int subMeshIndex = 0; subMeshIndex < currentMesh.mesh.subMeshCount; subMeshIndex++)
        {
            var indexCount = currentMesh.mesh.GetSubMesh(subMeshIndex).indexCount;
            triangleCounter += indexCount / 3;
            if (triangleIndex < triangleCounter)
            {
                return subMeshIndex;
            }
        }
        Debug.LogError($"Failed to find triangle with index {triangleIndex} in mesh '{currentMesh.mesh.name}'. Total triangle count: {triangleCounter}", currentMesh.mesh);
        return 0;
    }

    void ApplyMaterialToBrushFace()
    {
        RaycastHit hit;
        Ray ray = new Ray() { origin = Camera.main.transform.position, direction = Camera.main.transform.forward };

        if (Physics.Raycast(ray, out hit, maxPickingDistance, pointerMask))
        {
            MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();

            if (meshFilter != null && hit.transform.tag == "Buildable Block")
            {
                currentMesh = meshFilter;
                MeshRenderer rend = currentMesh.GetComponent<MeshRenderer>();
                Material[] materials = rend.materials;
                materials[GetSubMeshIndex(hit.triangleIndex)] = Resources.Load<Material>("Materials/" + currentMaterial);
                rend.materials = materials;
            }
        }
    }

    void PaintProp()
    {
        RaycastHit hit;
        Ray ray = new Ray() { origin = Camera.main.transform.position, direction = Camera.main.transform.forward };

        if (Physics.Raycast(ray, out hit, maxPickingDistance, pointerMask))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Pickup"))
            {
                string chosenPalette = "";
                switch (currentPropLayerSelected)
                {
                    case 0:
                        chosenPalette = "_Palette_1";
                        break;
                    case 1:
                        chosenPalette = "_Palette_2";
                        break;
                    case 2:
                        chosenPalette = "_Palette_3";
                        break;
                    case 3:
                        chosenPalette = "_Palette_4";
                        break;
                    default:
                        break;
                }

                MeshRenderer[] rend = hit.collider.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < rend.Length; i++)
                {
                    Material[] mats = rend[i].materials;
                    for (int e = 0; e < mats.Length; e++)
                    {
                        mats[e].SetColor(chosenPalette, paintColors[currentPropColor]);
                    }
                    rend[i].SetMaterials(mats.ToList());
                }

            }
        }
    }
}
