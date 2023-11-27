using NUnit.Framework;
using NUnit.Framework.Internal;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hammer_UI : MonoBehaviour
{
    [SerializeField] LineRenderer lineRend;
    [SerializeField] FirstPersonController fpc;
 
    MeshFilter currentMesh;
    MeshCollider currentCollider;
    List<int> currentVertexes = new List<int>();
    bool isSelected;
    const float minVertexDistance = 0.1f;
    const float minLineDistance = 0.1f;
    const float edgeUIWidth = 0.025f;
    const float vertexUIWidth = 0.1f;
    const float maxPickingDistance = 10.0f;
    Color highlightColor = new Color(1, 0.86666666666f, 0);
    Color selectColor = new Color(0, 1, 0.29803921568f);

    private void Start()
    {
        RenderLines(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RenderLines(!isSelected);
        }
        if (Input.GetMouseButtonDown(1))
        {
            RenderLines(!isSelected);

            if (isSelected && currentMesh != null)
            {
                currentVertexes = new List<int>();
                for (int i = 0; i < currentMesh.mesh.vertices.Length; i++)
                {
                    currentVertexes.Add(i);
                }
                RenderLines(isSelected);
            }
        }
        if (!isSelected)
        {
            SelectPoint();
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

                currentMesh.mesh.vertices = vertices;
                currentMesh.mesh.RecalculateBounds();
                currentCollider.sharedMesh = null;
                currentCollider.sharedMesh = currentMesh.mesh;

                lineRend.positionCount = Mathf.Max(2,currentVertexes.Count);
                for (int i = 0; i < Mathf.Max(2, currentVertexes.Count); i++)
                {
                    int index = Mathf.Min(i, currentVertexes.Count - 1);
                    lineRend.SetPosition(i, currentMesh.transform.TransformPoint(currentMesh.mesh.vertices[currentVertexes[index]]));
                }
            }
        }
    }
    void SelectPoint()
    {
        RaycastHit hit;
        Ray ray = new Ray() { origin = Camera.main.transform.position, direction = Camera.main.transform.forward };

        if (Physics.Raycast(ray, out hit, maxPickingDistance))
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

                    lineRend.positionCount = 2;
                    lineRend.SetPositions(new Vector3[] {
                            hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[closestVertex]),
                            hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[closestVertex]),
                        });
                    currentVertexes = new List<int>{ closestVertex };
                    lineRend.startColor = highlightColor;
                    lineRend.endColor = highlightColor;
                }
                else
                {
                    int closestEdgea = -1;
                    int closestEdgeb = -1;
                    int closestEdgec = -1;
                    pointCloseness = float.MaxValue;
                    for (int i = 0; i < meshFilter.mesh.triangles.Length; i = i + 3)
                    {
                        //Edge 0
                        float distance = Vector3.Distance(NearestPointOnFiniteLine(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i]]), hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 1]]), hit.point), hit.point);
                        if (distance < pointCloseness)
                        {
                            pointCloseness = distance;
                            closestEdgea = i;
                            closestEdgeb = i + 1;
                            closestEdgec = i + 2;
                        }
                        //Edge 1
                        distance = Vector3.Distance(NearestPointOnFiniteLine(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 1]]), hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 2]]), hit.point), hit.point);
                        if (distance < pointCloseness)
                        {
                            pointCloseness = distance;
                            closestEdgea = i + 1;
                            closestEdgeb = i + 2;
                            closestEdgec = i;
                        }
                        //Edge 2
                        distance = Vector3.Distance(NearestPointOnFiniteLine(hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 2]]), hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i]]), hit.point), hit.point);
                        if (distance < pointCloseness)
                        {
                            pointCloseness = distance;
                            closestEdgea = i + 2;
                            closestEdgeb = i;
                            closestEdgec = i + 1;
                        }
                    }
                    if (pointCloseness <= minLineDistance)
                    {
                        lineRend.startWidth = edgeUIWidth;
                        lineRend.endWidth = edgeUIWidth;
                        lineRend.positionCount = 2;
                        lineRend.SetPositions(new Vector3[] {
                            hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[closestEdgea]]),
                            hit.collider.transform.TransformPoint(meshFilter.mesh.vertices[meshFilter.mesh.triangles[closestEdgeb]]),
                        });
                        currentVertexes = new List<int> { meshFilter.mesh.triangles[closestEdgea], meshFilter.mesh.triangles[closestEdgeb] };
                        lineRend.startColor = highlightColor;
                        lineRend.endColor = highlightColor;
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
                                meshFilter.mesh.vertices[meshFilter.mesh.triangles[i+1]],
                                meshFilter.mesh.vertices[meshFilter.mesh.triangles[i+2]]
                                );
                            if(plane.normal == norm)
                            {
                                currentVertexes.Add(meshFilter.mesh.triangles[i]);
                                currentVertexes.Add(meshFilter.mesh.triangles[i+1]);
                                currentVertexes.Add(meshFilter.mesh.triangles[i+2]);
                            }
                        }
                        RenderLines(false);
                    }
                }
            }
        }
        else
        {
            currentMesh = null;
            currentCollider = null;
            currentVertexes = new List<int>();
            lineRend.positionCount = 0;
        }
    }
    void RenderLines(bool on)
    {
        if(currentMesh == null)
        {
            isSelected = false;
            lineRend.positionCount = 0;
            return;
        }

        isSelected = on;
        fpc.SetFOV(!on);
        if (on)
        {
            lineRend.startColor = selectColor;
            lineRend.endColor = selectColor;
        }
        else
        {
            lineRend.startColor = highlightColor;
            lineRend.endColor = highlightColor;
        }

        bool selection;
        if(currentMesh.mesh.vertices[currentVertexes[0]].Equals(currentMesh.mesh.vertices[currentVertexes[1]]))
        {
            selection = true;
        }
        else
        {
            selection = false;
        }

        if (selection)
        {
            lineRend.startWidth = vertexUIWidth;
            lineRend.endWidth = vertexUIWidth;
        }
        else
        {
            lineRend.startWidth = edgeUIWidth;
            lineRend.endWidth = edgeUIWidth;
        }

        lineRend.positionCount = currentVertexes.Count;
        for (int i = 0; i < currentVertexes.Count; i++)
        {
            lineRend.SetPosition(i, currentMesh.transform.TransformPoint(currentMesh.mesh.vertices[currentVertexes[i]]));
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

}
