using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class CelAnimator : MonoBehaviour
{
    OutlineEffect effect;

    private void OnEnable()
    {
        effect = Camera.main.transform.Find("Cel Camera").GetComponent<OutlineEffect>();

        effect.OutlineLayers[0].Add(this.gameObject);
        effect.OutlineLayers[1].Add(this.gameObject);
    }

    private void OnDisable()
    {
        effect.OutlineLayers[0].Remove(this.gameObject);
        effect.OutlineLayers[1].Remove(this.gameObject);
    }

}
