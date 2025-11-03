using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public float groundJumpVelocity;
    public float bounceVelocity;
    public float wallJumpHorizontalVelocity;
    public float wallJumpVerticalVelocity;
    public float doubleJumpVelocity;
    public float horziontalMoveSpeed;
    public float dashSpeed = 25f; // 5米 / 0.2秒 = 25 m/s
    public float dashDuration = 0.2f;
    public float attackInterval;
    public float normalGravityScale;
    public float wallHoldingGravityScale;
    public float dashGravityScale = 0f; // 冲刺时的重力缩放
    public EAttackState attackState = EAttackState.Idle;
    public bool showAttackGizmos = true;
    public GameObject attackEffect;
    public Transform attackCenter;
    public LayerMask attackLayer;

    private Rigidbody2D rb;
    public Vector2 inputDirection;
    private float faceDirection = 1;
    private float attackTimer = 0;
    private const float wallJumpDuration = 0.15f;
    private PhysicsCheck physicsCheck;
    [SerializeField] public EJumpState jumpState = EJumpState.Ground;
    [SerializeField] public EWallHoldingState wallHoldingState = EWallHoldingState.None;
    [SerializeField] public EDashState dashState = EDashState.Charged;

    // Gizmos绘制相关
    private bool shouldDrawGizmos = false;
    private Vector2 gizmosHitboxCenter;
    private Vector2 gizmosHitboxSize;
    private float gizmosStartTime;
    private const float gizmosDrawDuration = 1.0f;
    private const float releaseJumpSpeedDeclineRate = 0.8f;
    private bool isAfterWallJumping = false;
    public bool isDashing = false;
    private float dashDirection = 1f;
    private CancellationTokenSource wallJumpCancellationTokenSource = null;
    private CancellationTokenSource dashCancellationTokenSource = null;

    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        physicsCheck = GetComponent<PhysicsCheck>();
        physicsCheck.TouchGround += OnTouchGround;
        physicsCheck.LeaveLeftWall += OnHoldWallEnd;
        physicsCheck.LeaveRightWall += OnHoldWallEnd;
    }

    private void OnDestroy()
    {
        // 清理CancellationTokenSource，防止内存泄漏
        wallJumpCancellationTokenSource?.Cancel();
        wallJumpCancellationTokenSource?.Dispose();
        dashCancellationTokenSource?.Cancel();
        dashCancellationTokenSource?.Dispose();
    }

    private void Update()
    {
        transform.localScale = new Vector3(faceDirection, 1, 1);
        UpdateAttack();
        UpdateWallHolding();
        UpdateGizmosTimer();
        UpdateDashRecovery();
        Debug.Log(jumpState);
        Debug.Log($"isAfterWallJumping {isAfterWallJumping}");
        Debug.Log($"wallHoldingStatus {wallHoldingState}");
        Debug.Log($"dashState {dashState}, isDashing {isDashing}");
    }

    private void UpdateGizmosTimer()
    {
        if (shouldDrawGizmos && Time.time - gizmosStartTime >= gizmosDrawDuration)
        {
            shouldDrawGizmos = false;
        }
    }

    private void FixedUpdate()
    {
        UpdateMove();
    }

    private void UpdateMove()
    {
        if (!Mathf.Approximately(inputDirection.x, 0))
        {
            faceDirection = inputDirection.x;
        }

        if (isDashing)
        {
            // 冲刺期间保持冲刺速度和方向
            rb.velocity = new Vector2(dashSpeed * dashDirection, 0);
        }
        else if (isAfterWallJumping)
        {
            // rb.velocity = new Vector2(horziontalMoveSpeed * 1, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(horziontalMoveSpeed * inputDirection.x, rb.velocity.y);
        }
    }

    private void UpdateAttack()
    {
        if (attackState != EAttackState.Idle)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackState = EAttackState.Idle;
                attackTimer = 0;
            }
        }
    }

    private void UpdateWallHolding()
    {
        if (wallHoldingState == EWallHoldingState.None && !isAfterWallJumping)
        {
            if (physicsCheck.IsLeftWall && inputDirection.x < 0)
            {
                StartHoldingWall(EWallHoldingState.Left);
            }
            else if (physicsCheck.IsRightWall && inputDirection.x > 0)
            {
                StartHoldingWall(EWallHoldingState.Right);
            }
        }
        else if (wallHoldingState != EWallHoldingState.None && !isAfterWallJumping)
        {
            if (!((physicsCheck.IsLeftWall && inputDirection.x <= 0) || (physicsCheck.IsRightWall && inputDirection.x >= 0)))
            {
                wallHoldingState = EWallHoldingState.None;
                jumpState = physicsCheck.IsGround ? EJumpState.Ground : EJumpState.AfterFirstJumpInAir;
                rb.gravityScale = normalGravityScale;
            }
        }
    }

    private void UpdateDashRecovery()
    {
        if (dashState == EDashState.Exhausted)
        {
            if (physicsCheck.IsGround)
            {
                dashState = EDashState.Charged;
            }
        }
    }

    private void StartHoldingWall(EWallHoldingState newWallHoldingState)
    {
        wallHoldingState = newWallHoldingState;
        rb.velocity = new Vector2(0, 0);
        jumpState = EJumpState.HoldingWall;
        rb.gravityScale = wallHoldingGravityScale;
        dashState = EDashState.Charged; // 贴墙时充能
    }

    private void OnHoldWallEnd()
    {
        rb.gravityScale = normalGravityScale;
    }

    public void SetMoveInput(Vector2 inputDirection)
    {
        this.inputDirection = inputDirection;
    }

    private void OnTouchGround()
    {
        jumpState = EJumpState.Ground;
    }

    public void OnPressJump()
    {
        if (jumpState == EJumpState.Ground)
        {
            jumpState = EJumpState.AfterFirstJumpInAir;
            rb.velocity = new Vector2(rb.velocity.x, groundJumpVelocity);
        }
        else if (jumpState == EJumpState.HoldingWall)
        {
            if (wallHoldingState == EWallHoldingState.Left)
            {
                jumpState = EJumpState.AfterFirstJumpInAir;
                rb.velocity = new Vector2(wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
            }
            else if (wallHoldingState == EWallHoldingState.Right)
            {
                jumpState = EJumpState.AfterFirstJumpInAir;
                rb.velocity = new Vector2(-wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
            }
            AfterWallJumping().Forget();
        }
        else if (jumpState == EJumpState.AfterFirstJumpInAir || jumpState == EJumpState.AfterBouncingInAir)
        {
            jumpState = EJumpState.ExhaustedInAir;
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpVelocity);
        }
    }

    public void OnReleaseJump()
    {
        if (rb.velocity.y > 0 && jumpState != EJumpState.AfterBouncingInAir)
        {
            rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y * releaseJumpSpeedDeclineRate);
        }
    }

    public void HitBounce()
    {
        dashState = EDashState.Charged;
        if (jumpState == EJumpState.AfterFirstJumpInAir || jumpState == EJumpState.AfterBouncingInAir || jumpState == EJumpState.ExhaustedInAir)
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceVelocity);
            jumpState = EJumpState.AfterBouncingInAir;
        }
    }

    public void OnPressAttack()
    {
        if (attackState == EAttackState.Idle)
        {
            if (inputDirection.y >= 1)
            {
                attackState = EAttackState.AttackUp;
            }
            else if (inputDirection.y <= -1)
            {
                attackState = EAttackState.AttackDown;
            }
            else
            {
                attackState = EAttackState.AttackHorizontal;
            }
            attackTimer = 0;
            Vector2 hitboxCenter = GetAttackHitboxCenter(attackState);
            foreach (Collider2D hitCollider in GetAttackHit(hitboxCenter, attackState))
            {
                if (hitCollider != null && hitCollider.TryGetComponent(out IHitableObject hitableObject))
                {
                    hitableObject.OnHit();
                }
            }
            CreateAttackEffect(attackState, hitboxCenter);

            // 记录Gizmos绘制信息
            if (showAttackGizmos)
            {
                shouldDrawGizmos = true;
                gizmosHitboxCenter = hitboxCenter;
                gizmosHitboxSize = attackHitboxSizeHorizontal;
                if (attackState == EAttackState.AttackUp || attackState == EAttackState.AttackDown)
                {
                    gizmosHitboxSize = attackHitboxSizeVertical;
                }
                gizmosStartTime = Time.time;
            }
        }
    }

    private readonly Vector2 attackHitboxSizeHorizontal = new Vector2(5.0f, 2.0f);
    private readonly Vector2 attackHitboxSizeVertical = new Vector2(2.0f, 5.0f);

    private Vector2 GetAttackHitboxCenter(EAttackState attackState)
    {
        float horizontalOffset = 2.0f;
        float verticalOffset = 2.0f;
        Vector2 horizontalAttackOffset = new Vector2(faceDirection * horizontalOffset, 0);
        Vector2 upAttackOffset = new Vector2(0, verticalOffset);
        Vector2 downAttackOffset = new Vector2(0, -verticalOffset);
        Vector2 attackOffset = Vector2.zero;
        switch (attackState)
        {
            case EAttackState.AttackHorizontal:
                attackOffset = horizontalAttackOffset;
                break;
            case EAttackState.AttackUp:
                attackOffset = upAttackOffset;
                break;
            case EAttackState.AttackDown:
                attackOffset = downAttackOffset;
                break;
        }
        return attackCenter.position + (Vector3)attackOffset;
    }

    private void CreateAttackEffect(EAttackState attackState, Vector2 hitboxCenter)
    {

        Quaternion attackRotation = Quaternion.identity;
        switch (attackState)
        {
            case EAttackState.AttackHorizontal:
                attackRotation = Quaternion.Euler(0, 0, 0);
                break;
            case EAttackState.AttackUp:
                attackRotation = Quaternion.Euler(0, 0, 90 * faceDirection);
                break;
            case EAttackState.AttackDown:
                attackRotation = Quaternion.Euler(0, 0, -90 * faceDirection);
                break;
        }
        GameObject attackEffectObj = Instantiate(attackEffect, hitboxCenter, attackRotation);
        attackEffectObj.transform.localScale = new Vector3(faceDirection, 1, 1);
        attackEffectObj.SetActive(true);
    }

    private Collider2D[] GetAttackHit(Vector2 hitboxCenter, EAttackState attackState)
    {
        Collider2D[] hitColliders = new Collider2D[10];
        if (attackState == EAttackState.AttackHorizontal)
        {
            Physics2D.OverlapBoxNonAlloc(hitboxCenter, attackHitboxSizeHorizontal, 0, hitColliders, attackLayer);
        }
        else if (attackState == EAttackState.AttackUp || attackState == EAttackState.AttackDown)
        {
            Physics2D.OverlapBoxNonAlloc(hitboxCenter, attackHitboxSizeVertical, 0, hitColliders, attackLayer);
        }
        return hitColliders;
    }

    private void OnDrawGizmos()
    {
        if (shouldDrawGizmos && showAttackGizmos)
        {
            // 根据剩余时间调整透明度
            float remainingTime = gizmosDrawDuration - (Time.time - gizmosStartTime);
            float alpha = Mathf.Clamp01(remainingTime / gizmosDrawDuration);

            // 设置Gizmos颜色（红色，带透明度）
            Gizmos.color = new Color(1f, 0f, 0f, alpha * 0.5f);

            // 绘制攻击hitbox
            Gizmos.DrawCube(gizmosHitboxCenter, gizmosHitboxSize);

            // 绘制边框（更明显）
            Gizmos.color = new Color(1f, 0f, 0f, alpha);
            Gizmos.DrawWireCube(gizmosHitboxCenter, gizmosHitboxSize);
        }
    }

    public void OnPressDash()
    {
        if (dashState == EDashState.Charged && !isDashing)
        {
            PerformDash().Forget();
        }
    }

    private async UniTask PerformDash()
    {
        if (isDashing) return;

        // 取消之前的冲刺（如果有）
        dashCancellationTokenSource?.Cancel();
        dashCancellationTokenSource?.Dispose();
        dashCancellationTokenSource = new CancellationTokenSource();

        // 开始冲刺
        isDashing = true;
        dashState = EDashState.Exhausted;
        dashDirection = faceDirection;

        // 保存原有重力并设置冲刺重力
        float originalGravity = rb.gravityScale;
        rb.gravityScale = dashGravityScale;

        try
        {
            // 等待冲刺时间
            await UniTask.Delay((int)(dashDuration * 1000), cancellationToken: dashCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // 被取消时的处理
        }
        finally
        {
            // 结束冲刺
            isDashing = false;
            rb.gravityScale = originalGravity;
        }
    }

    private async UniTask AfterWallJumping()
    {
        // 如果已经在墙跳中，取消之前的
        if (isAfterWallJumping)
        {
            wallJumpCancellationTokenSource?.Cancel();
            wallJumpCancellationTokenSource?.Dispose();
        }

        wallJumpCancellationTokenSource = new CancellationTokenSource();
        isAfterWallJumping = true;

        await UniTask.Delay((int)(wallJumpDuration * 1000), cancellationToken: wallJumpCancellationTokenSource.Token);

        isAfterWallJumping = false;
    }
}

public enum EDashState
{
    Charged,
    Exhausted,
}
public enum EWallHoldingState
{
    None,
    Left,
    Right,
}

public enum EJumpState
{
    Ground,
    HoldingWall,
    AfterFirstJumpInAir,
    AfterBouncingInAir,
    ExhaustedInAir,
}

public enum EAttackState
{
    Idle,
    AttackHorizontal,
    AttackUp,
    AttackDown,
    Hit,
}