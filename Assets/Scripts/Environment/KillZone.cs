using UnityEngine;

/// <summary>
/// 死亡区域 - 用于岩浆、深渊等致命地形
/// 玩家进入后会被直接送回存档点
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    [Header("死亡区域设置")]
    [Tooltip("区域类型（用于 Debug 和特效）")]
    public EKillZoneType zoneType = EKillZoneType.Lava;
    
    [Tooltip("是否显示警告颜色")]
    public bool showWarningGizmos = true;
    
    [Tooltip("警告颜色")]
    public Color warningColor = new Color(1f, 0f, 0f, 0.3f);
    
    private void Awake()
    {
        // 确保是 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // 触发致命伤害
                player.OnTakeFatalDamage(transform.position);
                
                Debug.Log($"玩家进入死亡区域：{zoneType}");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showWarningGizmos) return;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        
        // 绘制危险区域
        Gizmos.color = warningColor;
        
        if (col is BoxCollider2D boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.offset, boxCol.size);
        }
        else if (col is CircleCollider2D circleCol)
        {
            Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
        }
        else if (col is PolygonCollider2D polyCol)
        {
            // 绘制多边形碰撞器的大致区域
            Bounds bounds = polyCol.bounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showWarningGizmos) return;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        
        // 选中时绘制更明显的边框
        Color edgeColor = warningColor;
        edgeColor.a = 1f;
        Gizmos.color = edgeColor;
        
        if (col is BoxCollider2D boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
        }
        else if (col is CircleCollider2D circleCol)
        {
            DrawWireCircle(transform.position + (Vector3)circleCol.offset, circleCol.radius);
        }
    }
    
    private void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
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

public enum EKillZoneType
{
    Lava,      // 岩浆
    Abyss,     // 深渊
    Poison,    // 毒液
    Spikes,    // 尖刺区域
    Other      // 其他
}

