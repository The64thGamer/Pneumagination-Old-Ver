using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEAD_DTU_Tester : MonoBehaviour
{
    [SerializeField] DEAD_Interface dead_Interface;
    [Header("Use NumKeys to test, indexes are tested DTU bits.")]
    [SerializeField] bool overrideDTU;
    [SerializeField] int index1;
    [SerializeField] int index2;
    [SerializeField] int index3;
    [SerializeField] int index4;
    [SerializeField] int index5;
    [SerializeField] int index6;
    [SerializeField] int index7;
    [SerializeField] int index8;
    [SerializeField] int index9;
    [SerializeField] int index0;

    [Header("Send Commands")]
    [SerializeField] string command;
    [SerializeField] bool sendCommand;
    [SerializeField] bool nonRecordableCommand;


    private void Update()
    {
        if(dead_Interface != null)
        {
            if(sendCommand)
            {
                sendCommand = false;
                dead_Interface.SendCommand(command,nonRecordableCommand);
            }
            if (overrideDTU)
            {
                for (int i = 0; i < 10; i++)
                {
                    int index = 0;
                    bool value = false;
                    switch (i)
                    {
                        case 1:
                            index = index1;
                            if (Input.GetKey(KeyCode.Alpha1))
                            {
                                value = true;
                            }
                            break;
                        case 2:
                            index = index2;
                            if (Input.GetKey(KeyCode.Alpha2))
                            {
                                value = true;
                            }
                            break;
                        case 3:
                            index = index3;
                            if (Input.GetKey(KeyCode.Alpha3))
                            {
                                value = true;
                            }
                            break;
                        case 4:
                            index = index4;
                            if (Input.GetKey(KeyCode.Alpha4))
                            {
                                value = true;
                            }
                            break;
                        case 5:
                            index = index5;
                            if (Input.GetKey(KeyCode.Alpha5))
                            {
                                value = true;
                            }
                            break;
                        case 6:
                            index = index6;
                            if (Input.GetKey(KeyCode.Alpha6))
                            {
                                value = true;
                            }
                            break;
                        case 7:
                            index = index7;
                            if (Input.GetKey(KeyCode.Alpha7))
                            {
                                value = true;
                            }
                            break;
                        case 8:
                            index = index8;
                            if (Input.GetKey(KeyCode.Alpha8))
                            {
                                value = true;
                            }
                            break;
                        case 9:
                            index = index9;
                            if (Input.GetKey(KeyCode.Alpha9))
                            {
                                value = true;
                            }
                            break;
                        case 0:
                            index = index0;
                            if (Input.GetKey(KeyCode.Alpha0))
                            {
                                value = true;
                            }
                            break;
                        default:
                            break;
                    }
                    dead_Interface.SetData(index, value ? 1f : 0f);
                }
            }
        }
    }

}
