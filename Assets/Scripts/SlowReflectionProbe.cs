using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowReflectionProbe : MonoBehaviour
{
    float timer;
    float reflectionTime;
    ReflectionProbe probe;
    void Start()
    {
        reflectionTime = PlayerPrefs.GetInt("Settings: Probe Timer");
        probe = this.GetComponent<ReflectionProbe>();
        if(reflectionTime == 0 || probe == null)
        {
            Destroy(this);
        }
        probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        timer = Random.Range(0, reflectionTime);
    }

    
    void Update()
    {
        if(timer <= 0)
        {
            timer = reflectionTime;
            probe.RenderProbe();
        }
        timer -= Time.deltaTime;
    }
}
