using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.MPE;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("动画配置")]
    [Tooltip("攻击动画状态名称（需要与Animator中的状态名一致）")]
    public string attackHorizontalStateName = "AttackHorizontal";
    public string attackUpStateName = "AttackUp";
    public string attackDownStateName = "AttackDown";

    [Header("动画原始时长（自动检测）")]
    [Tooltip("这些值会在运行时自动从Animator中获取，也可以手动设置作为备用")]
    public float attackHorizontalAnimLength = 0.5f;
    public float attackUpAnimLength = 0.5f;
    public float attackDownAnimLength = 0.5f;

    private Animator anim;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private PlayerController playerController;
    private EAttackState currentAttackState = EAttackState.Idle;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playerController = GetComponent<PlayerController>();

        // 自动检测动画长度
        AutoDetectAnimationLengths();
    }

    /// <summary>
    /// 自动从Animator中检测攻击动画的时长
    /// </summary>
    private void AutoDetectAnimationLengths()
    {
        if (anim == null || anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning("PlayerAnimation: Animator或RuntimeAnimatorController未找到，使用默认动画时长");
            return;
        }

        // 尝试通过状态名获取动画长度
        float horizontalLength = GetAnimationLengthByStateName(attackHorizontalStateName);
        if (horizontalLength > 0)
        {
            attackHorizontalAnimLength = horizontalLength;
            Debug.Log($"自动检测到水平攻击动画时长: {attackHorizontalAnimLength}秒");
        }
        else
        {
            Debug.LogWarning($"未找到状态 '{attackHorizontalStateName}'，使用默认值: {attackHorizontalAnimLength}秒");
        }

        float upLength = GetAnimationLengthByStateName(attackUpStateName);
        if (upLength > 0)
        {
            attackUpAnimLength = upLength;
            Debug.Log($"自动检测到上攻击动画时长: {attackUpAnimLength}秒");
        }
        else
        {
            Debug.LogWarning($"未找到状态 '{attackUpStateName}'，使用默认值: {attackUpAnimLength}秒");
        }

        float downLength = GetAnimationLengthByStateName(attackDownStateName);
        if (downLength > 0)
        {
            attackDownAnimLength = downLength;
            Debug.Log($"自动检测到下攻击动画时长: {attackDownAnimLength}秒");
        }
        else
        {
            Debug.LogWarning($"未找到状态 '{attackDownStateName}'，使用默认值: {attackDownAnimLength}秒");
        }
    }

    /// <summary>
    /// 通过状态名获取动画长度
    /// </summary>
    private float GetAnimationLengthByStateName(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return -1;

        // 方法1: 通过AnimationClip名称查找
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == stateName)
            {
                return clip.length;
            }
        }

        // 方法2: 如果方法1失败，尝试遍历所有层和状态
        for (int layer = 0; layer < anim.layerCount; layer++)
        {
            AnimatorControllerParameter[] parameters = anim.parameters;
            // 获取当前层的所有状态（这需要通过RuntimeAnimatorController）
            foreach (AnimationClip clip in clips)
            {
                // 模糊匹配：状态名可能包含动画名
                if (clip.name.Contains(stateName) || stateName.Contains(clip.name))
                {
                    return clip.length;
                }
            }
        }

        return -1; // 未找到
    }

    private void Update()
    {
        SetAnimation();
        HandleAttackAnimation();
    }

    public void SetAnimation()
    {
        anim.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("velocityY", rb.velocity.y);
        anim.SetBool("isGround", physicsCheck.IsGround);
        anim.SetInteger("attackState", (int)playerController.attackState);
        anim.SetBool("isDashing", playerController.isDashing);
        anim.SetBool("isHoldingWall", playerController.wallHoldingState != EWallHoldingState.None);
    }

    private void HandleAttackAnimation()
    {
        // 检测攻击状态变化
        if (playerController.attackState != currentAttackState && playerController.attackState != EAttackState.Idle)
        {
            // 计算并设置动画播放速度
            float originalAnimLength = GetOriginalAnimLength(playerController.attackState);
            float speedMultiplier = originalAnimLength / playerController.attackInterval;
            anim.speed = speedMultiplier;
        }
        else if (playerController.attackState == EAttackState.Idle && currentAttackState != EAttackState.Idle)
        {
            // 攻击结束，恢复正常速度
            anim.speed = 1.0f;
        }
        currentAttackState = playerController.attackState;
    }

    private float GetOriginalAnimLength(EAttackState attackState)
    {
        switch (attackState)
        {
            case EAttackState.AttackHorizontal:
                return attackHorizontalAnimLength;
            case EAttackState.AttackUp:
                return attackUpAnimLength;
            case EAttackState.AttackDown:
                return attackDownAnimLength;
            default:
                return 0.5f;
        }
    }
}
