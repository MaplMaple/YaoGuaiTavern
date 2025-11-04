using UnityEngine;

/// <summary>
/// 致命尖刺 - 碰到会直接送回存档点
/// 用于跳跳乐等高难度路线
/// 可以被玩家攻击摧毁
/// </summary>
public class FatalSpike : MonoBehaviour, IHitableObject
{
    [Header("致命伤害设置")]
    [Tooltip("是否启用致命伤害（否则为普通伤害）")]
    public bool isFatal = true;
    
    [Header("可击打设置")]
    [Tooltip("被击打后是否销毁")]
    public bool destroyOnHit = true;
    
    [Tooltip("击打后播放的音效")]
    [SerializeField] private AudioClip _hitAudio;
    
    [Tooltip("击打后播放的特效")]
    [SerializeField] private GameObject _hitEffect;
    
    public AudioClip hitAudio { get; set; }
    public GameObject hitEffect { get; set; }
    
    private void Awake()
    {
        // 初始化接口属性
        hitAudio = _hitAudio;
        hitEffect = _hitEffect;
    }
    
    public void OnHit()
    {
        // 播放击打特效
        if (_hitEffect != null)
        {
            GameObject effect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // 播放击打音效
        if (_hitAudio != null)
        {
            AudioSource.PlayClipAtPoint(_hitAudio, transform.position);
        }
        
        // 根据设置决定是否销毁
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (isFatal)
                {
                    // 致命伤害 - 先造成普通伤害表现，再送回存档点
                    player.OnTakeDamage(transform.position);
                    player.OnTakeFatalDamage(transform.position);
                }
                else
                {
                    // 普通伤害 - 仅击退
                    player.OnTakeDamage(transform.position);
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                if (isFatal)
                {
                    // 致命伤害 - 先造成普通伤害表现，再送回存档点
                    player.OnTakeDamage(transform.position);
                    player.OnTakeFatalDamage(transform.position);
                }
                else
                {
                    // 普通伤害 - 仅击退
                    player.OnTakeDamage(transform.position);
                }
            }
        }
    }
}

