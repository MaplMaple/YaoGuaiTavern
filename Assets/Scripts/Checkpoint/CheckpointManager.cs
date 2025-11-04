using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    #region 单例
    private static CheckpointManager instance;
    public static CheckpointManager Instance => instance;
    #endregion

    private List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 注册检查点
    /// </summary>
    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (!allCheckpoints.Contains(checkpoint))
        {
            allCheckpoints.Add(checkpoint);
            Debug.Log($"检查点管理器：注册检查点 {checkpoint.checkpointId}");
        }
    }
    
    /// <summary>
    /// 注销检查点
    /// </summary>
    public void UnregisterCheckpoint(Checkpoint checkpoint)
    {
        allCheckpoints.Remove(checkpoint);
    }
    
    /// <summary>
    /// 获取最近的已解锁检查点
    /// </summary>
    public Checkpoint GetNearestUnlockedCheckpoint(Vector3 position)
    {
        Checkpoint nearestCheckpoint = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var checkpoint in allCheckpoints)
        {
            if (checkpoint.IsUnlocked)
            {
                float distance = Vector3.Distance(position, checkpoint.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCheckpoint = checkpoint;
                }
            }
        }
        
        return nearestCheckpoint;
    }
    
    /// <summary>
    /// 获取所有已解锁的检查点
    /// </summary>
    public List<Checkpoint> GetAllUnlockedCheckpoints()
    {
        return allCheckpoints.Where(cp => cp.IsUnlocked).ToList();
    }
    
    /// <summary>
    /// 通过ID获取检查点
    /// </summary>
    public Checkpoint GetCheckpointById(string id)
    {
        return allCheckpoints.FirstOrDefault(cp => cp.checkpointId == id);
    }
}


