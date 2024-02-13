using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PneumagiNode : MonoBehaviour
{
    [SerializeField] List<InputHolder> nodeInputs = new List<InputHolder>();

    List<OutputHolder> nodesToOutputTo = new List<OutputHolder>();


    public void RecieveSignal(List<string> inputIDs, float value)
    {
        value = Mathf.Clamp01(value);

        for (int x = 0; x < inputIDs.Count; x++)
        {
            for (int y = 0; y < nodeInputs.Count; y++)
            {
                if (nodeInputs[y].inputID == inputIDs[x])
                {
                    nodeInputs[y].inputListener.Invoke(value);
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
            if(outputID == nodesToOutputTo[i].outputID)
            {
                for (int e = 0; e < nodesToOutputTo[i].recieverNodes.Count; e++)
                {
                    nodesToOutputTo[i].recieverNodes[e].recieverNode.RecieveSignal(nodesToOutputTo[i].recieverNodes[e].inputIDs, value);
                }
                break;
            }
        }
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
    public UnityEvent<float> inputListener;
    public string inputID;
}
