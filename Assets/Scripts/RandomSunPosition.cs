using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSunPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.transform.eulerAngles = new Vector3(Random.Range(135.0f,45.0f),Random.Range(0.0f,360.0f), 0);
    }

}
