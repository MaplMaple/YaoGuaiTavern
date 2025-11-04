using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("检查点设置")]
    [Tooltip("检查点的唯一ID")]
    public string checkpointId;
    
    [Tooltip("是否默认解锁")]
    public bool isUnlockedByDefault = false;
    
    [Tooltip("解锁范围半径（如果为0，则使用 PlayerController 的设置）")]
    public float unlockRadius = 0f;
    
    [Tooltip("是否已解锁")]
    [SerializeField] private bool isUnlocked = false;
    
    [Header("视觉反馈")]
    [Tooltip("解锁时的特效")]
    public GameObject unlockEffect;
    
    [Tooltip("未解锁时的颜色")]
    public Color lockedColor = Color.gray;
    
    [Tooltip("已解锁时的颜色")]
    public Color unlockedColor = Color.green;
    
    private SpriteRenderer spriteRenderer;
    
    public bool IsUnlocked => isUnlocked;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 如果没有设置ID，使用GameObject名称
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = gameObject.name;
        }
        
        if (isUnlockedByDefault)
        {
            Unlock();
        }
        else
        {
            UpdateVisual();
        }
    }
    
    private void Start()
    {
        // 注册到检查点管理器
        CheckpointManager.Instance?.RegisterCheckpoint(this);
    }
    
    private void OnDestroy()
    {
        // 从检查点管理器中注销
        CheckpointManager.Instance?.UnregisterCheckpoint(this);
    }
    
    /// <summary>
    /// 解锁检查点
    /// </summary>
    public void Unlock()
    {
        if (isUnlocked) return;
        
        isUnlocked = true;
        UpdateVisual();
        
        // 播放解锁特效
        if (unlockEffect != null)
        {
            GameObject effect = Instantiate(unlockEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        Debug.Log($"检查点 {checkpointId} 已解锁！");
    }
    
    /// <summary>
    /// 更新视觉效果
    /// </summary>
    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }
    
    /// <summary>
    /// 获取检查点位置
    /// </summary>
    public Vector3 GetRespawnPosition()
    {
        return transform.position;
    }
    
    /// <summary>
    /// 获取解锁范围半径
    /// </summary>
    private float GetUnlockRadius()
    {
        // 如果设置了自定义半径，使用自定义值
        if (unlockRadius > 0f)
        {
            return unlockRadius;
        }
        
        // 否则尝试从 PlayerController 获取
        if (PlayerController.instance != null)
        {
            return PlayerController.instance.unlockCheckpointDistance;
        }
        
        // 默认值
        return 2f;
    }
    
    private void OnDrawGizmos()
    {
        DrawCheckpointGizmos(0.3f);
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawCheckpointGizmos(1f);
    }
    
    /// <summary>
    /// 绘制检查点的 Gizmos
    /// </summary>
    private void DrawCheckpointGizmos(float alpha)
    {
        float radius = GetUnlockRadius();
        
        // 根据解锁状态选择颜色
        Color gizmosColor = isUnlocked ? unlockedColor : lockedColor;
        gizmosColor.a = alpha * 0.3f;
        
        // 绘制解锁范围（实心圆）
        Gizmos.color = gizmosColor;
        Gizmos.DrawSphere(transform.position, radius);
        
        // 绘制范围边框（更明显）
        gizmosColor.a = alpha;
        Gizmos.color = gizmosColor;
        DrawWireCircle(transform.position, radius, 32);
        
        // 绘制中心点标记
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
    
    /// <summary>
    /// 绘制线框圆形
    /// </summary>
    private void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}


