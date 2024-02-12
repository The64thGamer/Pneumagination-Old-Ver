using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingTerminal : MonoBehaviour
{
    [SerializeField] ScrewTerminal.TerminalType terminalType;
    ScrewTerminal target;

    public bool MakeConnection(ScrewTerminal self, ScrewTerminal.TerminalType terminal)
    {
        if(terminal != terminalType)
        {
            return false;
        }
        if(target != null)
        {
            target.EndConnection(this);
        }
        target = self;
        return true;
    }

    public void Disconnect()
    {
        target = null;
    }

    public void DisconnectAndReconnectToMouse()
    {
        if(target == null)
        {
            return;
        }
        ScrewTerminal newtarget = target;
        target = null;
        newtarget.EndConnection(this);
        newtarget.StartConnection();
    }
}
