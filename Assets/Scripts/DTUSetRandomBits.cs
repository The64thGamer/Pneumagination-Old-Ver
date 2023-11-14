using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTUSetRandomBits : MonoBehaviour
{
    [SerializeField] DEAD_Interface deadInterface;
    [SerializeField] float timer;
    float internalTime;

    private void Update()
    {
        internalTime -= Time.deltaTime;
        if(internalTime < 0)
        {
            internalTime = timer;
            int index = Random.Range(0, deadInterface.GetDTUArrayLength());
            deadInterface.SetData(index, 1 - deadInterface.GetData(index));
        }
    }
}
