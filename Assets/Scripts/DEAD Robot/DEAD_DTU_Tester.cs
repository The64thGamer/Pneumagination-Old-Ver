using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEAD_DTU_Tester : MonoBehaviour
{
    [SerializeField] DEAD_Interface dead_Interface;
    [SerializeField] DEAD_Tester_Values[] testingValues;
    private void Update()
    {
        if(dead_Interface != null)
        {
            for (int i = 0; i < testingValues.Length; i++)
            {
                dead_Interface.SetData(testingValues[i].index, testingValues[i].value ? 1f : 0f);
            }
        }
    }

    [System.Serializable]
    public class DEAD_Tester_Values
    {
        public int index;
        public bool value;
    }
}
