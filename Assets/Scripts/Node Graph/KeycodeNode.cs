using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeycodeNode : MonoBehaviour
{
    [Range(0,9)]
    [SerializeField] int keyCode;
    [SerializeField] Transform keycap;
    [SerializeField] List<Mesh> keycapMeshes;

    private void Start()
    {
        keycap.GetComponent<MeshFilter>().mesh = keycapMeshes[keyCode];
    }

    private void Update()
    {
        if(Input.GetKey(keyCode.ToString()))
        {
            keycap.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(keycap.transform.localPosition.z, 0.005f, Time.deltaTime * 35)); 
        }
        else
        {
            keycap.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(keycap.transform.localPosition.z, 0, Time.deltaTime * 25));
        }
    }
}
