using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrewTerminal : MonoBehaviour
{
    [SerializeField] TerminalType terminalType;
    ConnectionState connection;
    RingTerminal target;
    LineRenderer rend; 

    void Start()
    {
        rend = this.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        switch (connection)
        {
            case ConnectionState.inProgress:
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    rend.SetPosition(1, hit.point + new Vector3(0, 0, -hit.point.z + 0.25f));

                    if (Input.GetMouseButtonUp(1))
                    {
                        RingTerminal ring = hit.collider.GetComponent<RingTerminal>();
                        if (ring != null && ring.MakeConnection(this,terminalType))
                        {
                            connection = ConnectionState.connected;
                            target = ring;
                        }
                        else
                        {
                            EndConnection();
                        }
                    }
                }
                break;
            case ConnectionState.connected:
                rend.SetPosition(0, transform.position + new Vector3(0.15f, 0, +0.1f));
                rend.SetPosition(1, target.transform.position + new Vector3(-0.125f, 0, -target.transform.position.z + 0.25f));
                break;
            default:
                break;
        }
    }

    public void StartConnection()
    {
        if(connection != ConnectionState.none)
        {
            return;
        }

        target = null;
        connection = ConnectionState.inProgress;
        rend.enabled = true;
        rend.SetPosition(0, transform.position + new Vector3(0.15f,0,+0.1f));
    }

    public void EndConnection()
    {
        if(target != null)
        {
            target.Disconnect();
            target = null;
        }
        connection = ConnectionState.none;
        rend.enabled = false;
    }

    public enum ConnectionState
    {
        none,
        inProgress,
        connected,
    }

    public enum TerminalType
    {
        power,
        airlines,
    }
}
