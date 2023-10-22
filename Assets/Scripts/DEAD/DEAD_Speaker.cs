using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DEAD_Speaker : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField] int defaultAudioSlot;

}
