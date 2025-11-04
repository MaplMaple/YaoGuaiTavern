using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingSpike : MonoBehaviour, IHitableObject
{
    [Header("攻击设置")]
    [Tooltip("是否为致命伤害（送回存档点）")]
    public bool isFatal = false;
    
    [SerializeField] private AudioClip _hitAudio;
    [SerializeField] private GameObject _hitEffect;
    public AudioClip hitAudio { get; set; }
    public GameObject hitEffect { get; set; }
    
    public void OnHit()
    {
        // 弹跳逻辑现在由 PlayerController 在向下攻击时自动处理
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测是否碰到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (isFatal)
                {
                    // 致命伤害 - 直接送回存档点
                    player.OnTakeFatalDamage(transform.position);
                }
                else
                {
                    // 普通伤害 - 击退
                    player.OnTakeDamage(transform.position);
                }
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
                if (isFatal)
                {
                    // 致命伤害 - 直接送回存档点
                    player.OnTakeFatalDamage(transform.position);
                }
                else
                {
                    // 普通伤害 - 击退
                    player.OnTakeDamage(transform.position);
                }
            }
        }
    }
}
