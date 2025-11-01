using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    [Header("状态")]
    public bool isGround;

    [Header("检测范围")]
    public float checkRaduis;
    public LayerMask groundLayer;
    public Vector2 bottomOffset;
    public Action TouchGround;
    public Action LeaveGround;

    private void Update()
    {
        Check();
    }

    public void Check()
    {
        bool newStatus = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, checkRaduis, groundLayer);
        if (isGround && !newStatus)
        {
            LeaveGround?.Invoke();
        }
        else if (!isGround && newStatus)
        {
            TouchGround?.Invoke();
        }
        isGround = newStatus;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRaduis);
    }
}
