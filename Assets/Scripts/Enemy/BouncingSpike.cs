using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingSpike : MonoBehaviour, IHitableObject
{
    [SerializeField] private AudioClip _hitAudio;
    [SerializeField] private GameObject _hitEffect;
    public AudioClip hitAudio { get; set; }
    public GameObject hitEffect { get; set; }
    public void OnHit()
    {
        PlayerController.instance.HitBounce();
    }
}
