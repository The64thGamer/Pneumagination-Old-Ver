using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LODAdjuster : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Renderer[] rends = this.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i].name.Contains("LOD0"))
            {
                rends[i].gameObject.SetActive(true);
            }
            else
            {
                rends[i].gameObject.SetActive(false);
            }
        }
    }
}
