using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitableObject
{
    public AudioClip hitAudio { get; set; }
    public GameObject hitEffect { get; set; }
    public void OnHit();
}