using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PneumagiNode : MonoBehaviour
{
    [SerializeField]
    protected List<InputHolder> nodeInputs = new List<InputHolder>();
    protected List<OutputHolder> nodesToOutputTo = new List<OutputHolder>();
    Vector2 nodePosition;
    bool nodeVisibility;

    public void RecieveSignal(List<string> inputIDs, float value)
    {
        value = Mathf.Clamp01(value);

        for (int x = 0; x < inputIDs.Count; x++)
        {
            for (int y = 0; y < nodeInputs.Count; y++)
            {
                if (nodeInputs[y].inputID == inputIDs[x])
                {
                    nodeInputs[y].inputListener.Invoke(nodeInputs[y].inputID, value);
                    for (int z = 0; z < nodesToOutputTo.Count; z++)
                    {
                        if (nodeInputs[y].inputID == nodesToOutputTo[z].outputID)
                        {
                            SendSignal(nodesToOutputTo[z].outputID, value);
                            Color c = Color.white * value;
                            for (int i = 0; i < nodesToOutputTo[z].lineRenderers.Count; i++)
                            {
                                nodesToOutputTo[z].lineRenderers[i].SetColor("_Tint", c);
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }

    }

    public void SendSignal(string outputID, float value)
    {
        for (int i = 0; i < nodesToOutputTo.Count; i++)
        {
            if (outputID == nodesToOutputTo[i].outputID)
            {
                for (int e = 0; e < nodesToOutputTo[i].recieverNodes.Count; e++)
                {
                    nodesToOutputTo[i].recieverNodes[e].recieverNode.RecieveSignal(nodesToOutputTo[i].recieverNodes[e].inputIDs, value);
                }
                break;
            }
        }
    }

    public void SetNodePosition(Vector2 position)
    {
        nodePosition = position;
    }

    public void SetNodeVisibility(bool visibility)
    {
        nodeVisibility = visibility;
    }

    public void AddOutputs(string output, string nodeHashID, string[] recieverIDs)
    {
        for (int i = 0; i < nodesToOutputTo.Count; i++)
        {
            if (nodesToOutputTo[i].outputID == output)
            {
                bool foundNode = false;
                for (int x = 0; x < nodesToOutputTo[i].recieverNodes.Count; x++)
                {
                    if (nodesToOutputTo[i].recieverNodes[x].recieverNode.gameObject.name == nodeHashID)
                    {
                        nodesToOutputTo[i].recieverNodes[x].inputIDs.Concat(recieverIDs.ToList());
                        foundNode = true;
                        break;
                    }
                }
                if (!foundNode)
                {
                    RecieverNodeHolder recHolder = new RecieverNodeHolder();
                    recHolder.recieverNode = GameObject.Find(nodeHashID).GetComponentInChildren<PneumagiNode>();
                    recHolder.inputIDs = recieverIDs.ToList<string>();
                    nodesToOutputTo[i].recieverNodes.Add(recHolder);
                }
                break;
            }
        }
    }

    public CustomNodeData GenerateSaveData()
    {
        CustomNodeData data = new CustomNodeData();
        data.isVisible = nodeVisibility;
        data.position = nodePosition;
        data.outputs = new CustomNodeOutputData[nodesToOutputTo.Count];
        for (int i = 0; i < nodesToOutputTo.Count; i++)
        {
            data.outputs[i].outputID = nodesToOutputTo[i].outputID;
            data.outputs[i].recieverNodes = new CustomNodeOutputRecieverData[nodesToOutputTo[i].recieverNodes.Count];
            for (int e = 0; e < nodesToOutputTo[i].recieverNodes.Count; e++)
            {
                data.outputs[i].recieverNodes[e].nodeHashID = nodesToOutputTo[i].recieverNodes[e].recieverNode.gameObject.name;
                data.outputs[i].recieverNodes[e].recieverInputIDs = nodesToOutputTo[i].recieverNodes[e].inputIDs.ToArray();
            }
        }
        return data;
    }
}

[System.Serializable]
public class OutputHolder
{
    public string outputID;
    public List<Material> lineRenderers = new List<Material>();
    public List<RecieverNodeHolder> recieverNodes = new List<RecieverNodeHolder>();
}

[System.Serializable]
public class RecieverNodeHolder
{
    public PneumagiNode recieverNode;
    public List<string> inputIDs;
}

[System.Serializable]
public class InputHolder
{
    public UnityEvent<string, float> inputListener = new UnityEvent<string, float>();
    public string inputID;
}
