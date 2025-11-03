using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private bool _isGround = false;
    public bool IsGround
    {
        get
        {
            return _isGround;
        }
        set
        {
            if (value && _isGround == false)
            {
                _isGround = value;
                TouchGround?.Invoke();
            }
            else if (!value && _isGround == true)
            {
                _isGround = value;
                LeaveGround?.Invoke();
            }
        }
    }

    private bool _isLeftWall = false;
    [SerializeField]
    public bool IsLeftWall
    {
        get
        {
            return _isLeftWall;
        }
        set
        {
            if (value && _isLeftWall == false)
            {
                _isLeftWall = value;
                TouchLeftWall?.Invoke();
            }
            else if (!value && _isLeftWall == true)
            {
                _isLeftWall = value;
                LeaveLeftWall?.Invoke();
            }
        }
    }

    private bool _isRightWall = false;
    [SerializeField]
    public bool IsRightWall
    {
        get
        {
            return _isRightWall;
        }
        set
        {
            if (value && _isRightWall == false)
            {
                _isRightWall = value;
                TouchRightWall?.Invoke();
            }
            else if (!value && _isRightWall == true)
            {
                _isRightWall = value;
                LeaveRightWall?.Invoke();
            }
        }
    }

    [Header("检测范围")]
    public float checkRaduis;
    public LayerMask groundLayer;
    public LayerMask holdableWallLayer;
    public List<Vector2> bottomOffsets;
    public List<Vector2> leftOffsets;
    public List<Vector2> rightOffsets;
    public Action TouchGround;
    public Action LeaveGround;
    public Action TouchLeftWall;
    public Action TouchRightWall;
    public Action LeaveLeftWall;
    public Action LeaveRightWall;

    private void Update()
    {
        bool bottomStatus = false;
        foreach (var offset in bottomOffsets)
        {
            if (Check(offset, groundLayer))
            {
                bottomStatus = true;
                break;
            }
        }
        IsGround = bottomStatus;

        if (!IsGround)
        {

            bool leftStatus = false;
            foreach (var offset in leftOffsets)
            {
                if (Check(offset, holdableWallLayer))
                {
                    leftStatus = true;
                    break;
                }
            }
            IsLeftWall = leftStatus;

            bool rightStatus = false;
            foreach (var offset in rightOffsets)
            {
                if (Check(offset, holdableWallLayer))
                {
                    rightStatus = true;
                    break;
                }
            }
            IsRightWall = rightStatus;
        }
        else
        {
            IsLeftWall = false;
            IsRightWall = false;
        }

        Debug.Log($"IsLeftWall: {IsLeftWall}, IsRightWall: {IsRightWall}");
    }

    public bool Check(Vector2 offset, LayerMask layerMask)
    {
        return Physics2D.OverlapCircle((Vector2)transform.position + offset, checkRaduis, layerMask);
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制底部检测点 - 绿色
        Gizmos.color = Color.green;
        foreach (var offset in bottomOffsets)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + offset, checkRaduis);
        }

        // 绘制左侧检测点 - 红色
        Gizmos.color = Color.red;
        foreach (var offset in leftOffsets)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + offset, checkRaduis);
        }

        // 绘制右侧检测点 - 蓝色
        Gizmos.color = Color.blue;
        foreach (var offset in rightOffsets)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + offset, checkRaduis);
        }
    }
}
