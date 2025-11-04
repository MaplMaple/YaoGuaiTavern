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
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测是否碰到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // 传入尖刺的位置作为攻击者位置
                player.OnTakeDamage(transform.position);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果使用的是 Trigger 碰撞器
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // 传入尖刺的位置作为攻击者位置
                player.OnTakeDamage(transform.position);
            }
        }
    }
}
