using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScrewTerminal : MonoBehaviour
{
    [SerializeField] GameObject wirePrefab;
    [SerializeField] TerminalType terminalType;
    bool currentlyConnecting;
    List<RingTerminal> targets = new List<RingTerminal>();
    List<LineRenderer> lineRenderers = new List<LineRenderer>(); 

    private void Update()
    {
        Vector3 startOffset = transform.position + new Vector3(0.15f, 0, +0.1f);

        if (currentlyConnecting)
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                lineRenderers[lineRenderers.Count - 1].SetPosition(0, startOffset);

                lineRenderers[lineRenderers.Count-1].SetPosition(1, hit.point + new Vector3(0, 0, -hit.point.z + 0.25f));

                if (Input.GetMouseButtonUp(0))
                {
                    RingTerminal ring = hit.collider.GetComponent<RingTerminal>();
                    if (ring != null && ring.MakeConnection(this, terminalType))
                    {
                        currentlyConnecting = false;
                        targets.Add(ring);
                    }
                    else
                    {
                        currentlyConnecting = false;
                        LineRenderer rend = lineRenderers[lineRenderers.Count-1];
                        lineRenderers.RemoveAt(lineRenderers.Count - 1);
                        Destroy(rend);
                    }
                }
            }
        }

        for (int i = 0; i < lineRenderers.Count + (currentlyConnecting ? -1 : 0); i++)
        {
            lineRenderers[i].SetPosition(0, startOffset);
            lineRenderers[i].SetPosition(1, targets[i].transform.position + new Vector3(-0.125f, 0, -targets[i].transform.position.z + 0.25f));
        }

    }

    public void StartConnection()
    {
        if (currentlyConnecting)
        {
            return;
        }

        currentlyConnecting = true;
        GameObject newPrefab = GameObject.Instantiate(wirePrefab,this.transform);
        newPrefab.transform.position = Vector3.zero;
        lineRenderers.Add(newPrefab.GetComponent<LineRenderer>());
    }

    public void EndConnection(RingTerminal ring)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == ring)
            {
                ring.Disconnect();
                targets.RemoveAt(i);
                LineRenderer rend = lineRenderers[i];
                lineRenderers.RemoveAt(i);
                Destroy(rend);
                break;
            }
        }
        currentlyConnecting = false;
    }


    public enum TerminalType
    {
        power,
        airlines,
    }
}
