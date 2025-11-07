using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region 单例
    public static PlayerController instance;
    #endregion

    #region 公共参数配置
    
    [Header("移动参数")]
    public float horziontalMoveSpeed;
    public float normalGravityScale;
    
    [Header("跳跃参数")]
    public float groundJumpVelocity;
    public float doubleJumpVelocity;
    public float bounceVelocity;
    
    [Header("爬墙参数")]
    public float wallJumpHorizontalVelocity;
    public float wallJumpVerticalVelocity;
    public float wallHoldingGravityScale;
    
    [Header("冲刺参数")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashGravityScale = 0f;
    
    [Header("攻击参数")]
    public float attackInterval;
    public GameObject attackEffect;
    public Transform attackCenter;
    public LayerMask attackLayer;
    public bool showAttackGizmos = true;
    
    [Header("受击参数")]
    public Vector2 damagedVelocity = new Vector2(10f, 10f);
    public float hitPauseTime = 0.1f;
    public float invincibleTime = 0.5f;
    public float knockbackDuration = 0.3f;
    
    [Header("能力开关")]
    public bool canDoubleJump = true;
    public bool canDash = true;
    public bool canWallHold = true;
    
    [Header("存档点设置")]
    public float unlockCheckpointDistance = 2f;
    
    #endregion

    #region 公共状态
    public Vector2 inputDirection;
    public EAttackState attackState = EAttackState.Idle;
    public bool isAttacking = false;
    public bool isHit = false;
    public bool isBeingHit = false;
    public bool isInvincible = false;
    public bool isDashing = false;
    [SerializeField] public EJumpState jumpState = EJumpState.Ground;
    [SerializeField] public EWallHoldingState wallHoldingState = EWallHoldingState.None;
    [SerializeField] public EDashState dashState = EDashState.Charged;
    #endregion

    #region 私有组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PhysicsCheck physicsCheck;
    #endregion

    #region 私有状态变量
    
    // 移动相关
    private float faceDirection = 1;
    
    // 跳跃相关
    private const float releaseJumpSpeedDeclineRate = 0.8f;
    
    // 爬墙相关
    private const float wallJumpDuration = 0.15f;
    private bool isAfterWallJumping = false;
    
    // 冲刺相关
    private float dashDirection = 1f;
    
    // 攻击相关
    private float attackTimer = 0;
    private readonly Vector2 attackHitboxSizeHorizontal = new Vector2(5.0f, 2.0f);
    private readonly Vector2 attackHitboxSizeVertical = new Vector2(2.0f, 5.0f);
    
    // 受击相关
    private bool isInKnockback = false;
    
    // 视觉效果相关
    private float invincibleBlinkTimer = 0f;
    private const float invincibleBlinkInterval = 0.1f;
    private bool isVisibleDuringInvincible = true;
    private Color originalColor;
    private Color knockbackDebugColor = Color.red;
    
    // Gizmos绘制相关
    private bool shouldDrawGizmos = false;
    private Vector2 gizmosHitboxCenter;
    private Vector2 gizmosHitboxSize;
    private float gizmosStartTime;
    private const float gizmosDrawDuration = 1.0f;
    
    // 异步操作取消令牌
    private CancellationTokenSource wallJumpCancellationTokenSource = null;
    private CancellationTokenSource dashCancellationTokenSource = null;
    private CancellationTokenSource knockbackCancellationTokenSource = null;
    
    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        InitializeSingleton();
        InitializeComponents();
        InitializeRigidbody();
        InitializePhysicsCheck();
        InitializeVisuals();
    }

    private void Update()
    {
        UpdateFaceDirection();
        UpdateAttackTimer();
        UpdateWallHolding();
        UpdateDashRecovery();
        UpdateVisualEffects();
        UpdateGizmosTimer();
        UpdateCheckpointDetection();
        DebugLogStates();
    }

    private void FixedUpdate()
    {
        UpdateMove();
    }

    private void OnDestroy()
    {
        CleanupCancellationTokens();
    }

    private void OnDrawGizmos()
    {
        DrawAttackGizmos();
    }

    #endregion

    #region 初始化

    private void InitializeSingleton()
    {
        instance = this;
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        physicsCheck = GetComponent<PhysicsCheck>();
    }

    private void InitializeRigidbody()
    {
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void InitializePhysicsCheck()
    {
        physicsCheck.TouchGround += OnTouchGround;
        physicsCheck.LeaveLeftWall += OnHoldWallEnd;
        physicsCheck.LeaveRightWall += OnHoldWallEnd;
    }

    private void InitializeVisuals()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void CleanupCancellationTokens()
    {
        wallJumpCancellationTokenSource?.Cancel();
        wallJumpCancellationTokenSource?.Dispose();
        dashCancellationTokenSource?.Cancel();
        dashCancellationTokenSource?.Dispose();
        knockbackCancellationTokenSource?.Cancel();
        knockbackCancellationTokenSource?.Dispose();
    }

    #endregion

    #region 输入处理

    public void SetMoveInput(Vector2 inputDirection)
    {
        this.inputDirection = inputDirection;
    }

    public void OnPressJump()
    {
        if (isBeingHit) return;
        
        if (jumpState == EJumpState.Ground)
        {
            PerformGroundJump();
        }
        else if (jumpState == EJumpState.HoldingWall && canWallHold)
        {
            PerformWallJump();
        }
        else if ((jumpState == EJumpState.AfterFirstJumpInAir || jumpState == EJumpState.AfterBouncingInAir) && canDoubleJump)
        {
            PerformDoubleJump();
        }
    }

    public void OnReleaseJump()
    {
        if (rb.velocity.y > 0 && jumpState != EJumpState.AfterBouncingInAir)
        {
            rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y * releaseJumpSpeedDeclineRate);
        }
    }

    public void OnPressAttack()
    {
        if (isBeingHit) return;
        if (attackState != EAttackState.Idle) return;
        
        PerformAttack();
    }

    public void OnPressDash()
    {
        if (isBeingHit) return;
        if (wallHoldingState != EWallHoldingState.None) return;
        if (!canDash || dashState != EDashState.Charged || isDashing) return;
        
        PerformDash().Forget();
    }

    #endregion

    #region 移动系统

    private void UpdateFaceDirection()
    {
        transform.localScale = new Vector3(faceDirection, 1, 1);
    }

    private void UpdateMove()
    {
        if (isBeingHit) return;
        
        UpdateFaceDirectionFromInput();
        ApplyMovementVelocity();
    }

    private void UpdateFaceDirectionFromInput()
    {
        if (!Mathf.Approximately(inputDirection.x, 0))
        {
            faceDirection = inputDirection.x;
        }
    }

    private void ApplyMovementVelocity()
    {
        if (isDashing)
        {
            rb.velocity = new Vector2(dashSpeed * dashDirection, 0);
        }
        else if (isAfterWallJumping || isInKnockback)
        {
            // 墙跳后或击退期间不能控制移动方向
        }
        else
        {
            rb.velocity = new Vector2(horziontalMoveSpeed * inputDirection.x, rb.velocity.y);
        }
    }

    #endregion

    #region 跳跃系统

    private void PerformGroundJump()
    {
        jumpState = EJumpState.AfterFirstJumpInAir;
        rb.velocity = new Vector2(rb.velocity.x, groundJumpVelocity);
    }

    private void PerformDoubleJump()
    {
        jumpState = EJumpState.ExhaustedInAir;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpVelocity);
    }

    public void HitBounce()
    {
        if (canDash)
        {
            dashState = EDashState.Charged;
        }
        
        if (jumpState == EJumpState.AfterFirstJumpInAir || 
            jumpState == EJumpState.AfterBouncingInAir || 
            jumpState == EJumpState.ExhaustedInAir)
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceVelocity);
            jumpState = EJumpState.AfterBouncingInAir;
        }
    }

    private void OnTouchGround()
    {
        jumpState = EJumpState.Ground;
    }

    #endregion

    #region 爬墙系统

    private void UpdateWallHolding()
    {
        if (!canWallHold)
        {
            ResetWallHoldingIfActive();
            return;
        }
        
        if (wallHoldingState == EWallHoldingState.None && !isAfterWallJumping)
        {
            TryStartHoldingWall();
        }
        else if (wallHoldingState != EWallHoldingState.None && !isAfterWallJumping)
        {
            TryStopHoldingWall();
        }
    }

    private void ResetWallHoldingIfActive()
    {
        if (wallHoldingState != EWallHoldingState.None)
        {
            wallHoldingState = EWallHoldingState.None;
            rb.gravityScale = normalGravityScale;
        }
    }

    private void TryStartHoldingWall()
    {
        // 只有在垂直速度<=0时才能进入爬墙状态（即下落或静止时）
        if (rb.velocity.y > 0) return;
        
        if (physicsCheck.IsLeftWall && inputDirection.x < 0)
        {
            StartHoldingWall(EWallHoldingState.Left);
        }
        else if (physicsCheck.IsRightWall && inputDirection.x > 0)
        {
            StartHoldingWall(EWallHoldingState.Right);
        }
    }

    private void TryStopHoldingWall()
    {
        if (!((physicsCheck.IsLeftWall && inputDirection.x <= 0) || 
              (physicsCheck.IsRightWall && inputDirection.x >= 0)))
        {
            StopHoldingWall();
        }
    }

    private void StartHoldingWall(EWallHoldingState newWallHoldingState)
    {
        wallHoldingState = newWallHoldingState;
        rb.velocity = new Vector2(0, 0);
        jumpState = EJumpState.HoldingWall;
        rb.gravityScale = wallHoldingGravityScale;
        
        if (canDash)
        {
            dashState = EDashState.Charged;
        }
    }

    private void StopHoldingWall()
    {
        wallHoldingState = EWallHoldingState.None;
        jumpState = physicsCheck.IsGround ? EJumpState.Ground : EJumpState.AfterFirstJumpInAir;
        rb.gravityScale = normalGravityScale;
    }

    private void OnHoldWallEnd()
    {
        rb.gravityScale = normalGravityScale;
    }

    private void PerformWallJump()
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

    private async UniTask AfterWallJumping()
    {
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

    #endregion

    #region 冲刺系统

    private void UpdateDashRecovery()
    {
        if (canDash && dashState == EDashState.Exhausted && physicsCheck.IsGround)
        {
            dashState = EDashState.Charged;
        }
    }

    private async UniTask PerformDash()
    {
        if (isDashing) return;

        InitializeDash();
        
        float originalGravity = rb.gravityScale;
        rb.gravityScale = dashGravityScale;

        try
        {
            await UniTask.Delay((int)(dashDuration * 1000), cancellationToken: dashCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // 被取消时的处理
        }
        finally
        {
            FinalizeDash(originalGravity);
        }
    }

    private void InitializeDash()
    {
        dashCancellationTokenSource?.Cancel();
        dashCancellationTokenSource?.Dispose();
        dashCancellationTokenSource = new CancellationTokenSource();

        isDashing = true;
        dashState = EDashState.Exhausted;
        dashDirection = faceDirection;
    }

    private void FinalizeDash(float originalGravity)
    {
        isDashing = false;
        rb.gravityScale = originalGravity;
    }

    #endregion

    #region 攻击系统

    private void UpdateAttackTimer()
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

    private void PerformAttack()
    {
        DetermineAttackDirection();
        attackTimer = 0;
        
        Vector2 hitboxCenter = GetAttackHitboxCenter(attackState);
        ProcessAttackHits(hitboxCenter);
        CreateAttackEffect(attackState, hitboxCenter);
        RecordAttackGizmos(hitboxCenter);
        
        // 启动攻击动画标记
        SetAttackingState().Forget();
    }
    
    private async UniTask SetAttackingState()
    {
        isAttacking = true;
        await UniTask.Delay(150); // 0.2秒
        isAttacking = false;
    }

    private void DetermineAttackDirection()
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
    }

    private void ProcessAttackHits(Vector2 hitboxCenter)
    {
        bool hasHitSomething = false;
        
        foreach (Collider2D hitCollider in GetAttackHit(hitboxCenter, attackState))
        {
            if (hitCollider != null && hitCollider.TryGetComponent(out IHitableObject hitableObject))
            {
                hitableObject.OnHit();
                hasHitSomething = true;
            }
        }
        
        // 如果是向下攻击并且击中了东西，触发弹跳
        if (hasHitSomething && attackState == EAttackState.AttackDown)
        {
            HitBounce();
        }
    }

    private Vector2 GetAttackHitboxCenter(EAttackState attackState)
    {
        float horizontalOffset = 2.0f;
        float verticalOffset = 2.0f;
        
        Vector2 attackOffset = attackState switch
        {
            EAttackState.AttackHorizontal => new Vector2(faceDirection * horizontalOffset, 0),
            EAttackState.AttackUp => new Vector2(0, verticalOffset),
            EAttackState.AttackDown => new Vector2(0, -verticalOffset),
            _ => Vector2.zero
        };
        
        return attackCenter.position + (Vector3)attackOffset;
    }

    private Collider2D[] GetAttackHit(Vector2 hitboxCenter, EAttackState attackState)
    {
        Collider2D[] hitColliders = new Collider2D[10];
        Vector2 hitboxSize = (attackState == EAttackState.AttackHorizontal) 
            ? attackHitboxSizeHorizontal 
            : attackHitboxSizeVertical;
            
        Physics2D.OverlapBoxNonAlloc(hitboxCenter, hitboxSize, 0, hitColliders, attackLayer);
        return hitColliders;
    }

    private void CreateAttackEffect(EAttackState attackState, Vector2 hitboxCenter)
    {
        Quaternion attackRotation = attackState switch
        {
            EAttackState.AttackUp => Quaternion.Euler(0, 0, 90 * faceDirection),
            EAttackState.AttackDown => Quaternion.Euler(0, 0, -90 * faceDirection),
            _ => Quaternion.identity
        };
        
        GameObject attackEffectObj = Instantiate(attackEffect, hitboxCenter, attackRotation);
        attackEffectObj.transform.localScale = new Vector3(faceDirection, 1, 1);
        attackEffectObj.SetActive(true);
    }

    private void RecordAttackGizmos(Vector2 hitboxCenter)
    {
        if (showAttackGizmos)
        {
            shouldDrawGizmos = true;
            gizmosHitboxCenter = hitboxCenter;
            gizmosHitboxSize = (attackState == EAttackState.AttackUp || attackState == EAttackState.AttackDown)
                ? attackHitboxSizeVertical
                : attackHitboxSizeHorizontal;
            gizmosStartTime = Time.time;
        }
    }

    private void UpdateGizmosTimer()
    {
        if (shouldDrawGizmos && Time.time - gizmosStartTime >= gizmosDrawDuration)
        {
            shouldDrawGizmos = false;
        }
    }

    private void DrawAttackGizmos()
    {
        if (!shouldDrawGizmos || !showAttackGizmos) return;

        float remainingTime = gizmosDrawDuration - (Time.time - gizmosStartTime);
        float alpha = Mathf.Clamp01(remainingTime / gizmosDrawDuration);

        Gizmos.color = new Color(1f, 0f, 0f, alpha * 0.5f);
        Gizmos.DrawCube(gizmosHitboxCenter, gizmosHitboxSize);

        Gizmos.color = new Color(1f, 0f, 0f, alpha);
        Gizmos.DrawWireCube(gizmosHitboxCenter, gizmosHitboxSize);
    }

    #endregion

    #region 受击处理

    /// <summary>
    /// 玩家受到伤害
    /// </summary>
    /// <param name="attackerPosition">攻击者的位置，用于计算击退方向</param>
    public void OnTakeDamage(Vector2 attackerPosition)
    {
        if (isInvincible || isBeingHit) return;
        
        TakeDamage(attackerPosition).Forget();
    }
    
    /// <summary>
    /// 玩家受到致命伤害（直接送回存档点）
    /// </summary>
    /// <param name="attackerPosition">攻击者的位置（用于特效等）</param>
    public void OnTakeFatalDamage(Vector2 attackerPosition)
    {
        TakeFatalDamage(attackerPosition).Forget();
    }

    private async UniTask TakeDamage(Vector2 attackerPosition)
    {
        isHit = true;
        isBeingHit = true;
        isInvincible = true;
        
        CancelDashIfActive();
        ApplyKnockbackVelocity(attackerPosition);
        await ApplyHitPause();
        
        // 等待击退完成
        await ApplyKnockback();
        
        // 击退结束后，isHit 设置为 false
        isHit = false;
        
        await UniTask.Delay(200);
        isBeingHit = false;
        
        await UniTask.Delay((int)(invincibleTime * 1000));
        isInvincible = false;
    }

    private void CancelDashIfActive()
    {
        if (isDashing)
        {
            dashCancellationTokenSource?.Cancel();
            isDashing = false;
            rb.gravityScale = normalGravityScale;
        }
    }

    private void ApplyKnockbackVelocity(Vector2 attackerPosition)
    {
        float knockbackDirection = Mathf.Sign(attackCenter.position.x - attackerPosition.x);
        if (Mathf.Approximately(knockbackDirection, 0))
        {
            knockbackDirection = -faceDirection;
        }
        
        rb.velocity = new Vector2(damagedVelocity.x * knockbackDirection, damagedVelocity.y);
    }

    private async UniTask ApplyHitPause()
    {
        Time.timeScale = 0f;
        await UniTask.Delay((int)(hitPauseTime * 1000), ignoreTimeScale: true);
        Time.timeScale = 1f;
    }

    private async UniTask ApplyKnockback()
    {
        if (isInKnockback)
        {
            knockbackCancellationTokenSource?.Cancel();
            knockbackCancellationTokenSource?.Dispose();
        }
        
        knockbackCancellationTokenSource = new CancellationTokenSource();
        isInKnockback = true;
        
        try
        {
            await UniTask.Delay((int)(knockbackDuration * 1000), cancellationToken: knockbackCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // 被取消时的处理
        }
        finally
        {
            isInKnockback = false;
        }
    }
    
    private async UniTask TakeFatalDamage(Vector2 attackerPosition)
    {
        // 短暂暂停游戏，给玩家反馈
        Time.timeScale = 0f;
        await UniTask.Delay((int)(hitPauseTime * 1000), ignoreTimeScale: true);
        Time.timeScale = 1f;
        
        // 等待 0.2 秒后开始处理
        await UniTask.Delay(200);
        
        // 开始屏幕渐变效果（0.4秒）
        if (ScreenFade.Instance != null)
        {
            // 启动渐变效果（不等待完成）
            ScreenFade.Instance.FadeOutAndIn(0.4f).Forget();
            
            // 等待渐变到黑色（0.2秒，即总时长的一半）
            await UniTask.Delay(200);
        }
        
        // 在黑屏时回到存档点
        RespawnAtNearestCheckpoint();
        
        Debug.Log("玩家受到致命伤害，回到存档点");
    }

    #endregion

    #region 视觉效果

    private void UpdateVisualEffects()
    {
        if (spriteRenderer == null) return;
        
        UpdateInvincibleBlink();
        UpdateKnockbackDebugColor();
    }

    private void UpdateInvincibleBlink()
    {
        if (isInvincible && !isBeingHit)
        {
            invincibleBlinkTimer += Time.deltaTime;
            if (invincibleBlinkTimer >= invincibleBlinkInterval)
            {
                invincibleBlinkTimer = 0f;
                isVisibleDuringInvincible = !isVisibleDuringInvincible;
                spriteRenderer.enabled = isVisibleDuringInvincible;
            }
        }
        else
        {
            if (!spriteRenderer.enabled)
            {
                spriteRenderer.enabled = true;
            }
            invincibleBlinkTimer = 0f;
            isVisibleDuringInvincible = true;
        }
    }

    private void UpdateKnockbackDebugColor()
    {
        if (isInKnockback)
        {
            spriteRenderer.color = knockbackDebugColor;
        }
        else if (spriteRenderer.color == knockbackDebugColor)
        {
            spriteRenderer.color = originalColor;
        }
    }

    #endregion

    #region 能力开关控制接口

    /// <summary>
    /// 启用或禁用二段跳能力
    /// </summary>
    public void SetDoubleJumpAbility(bool enabled)
    {
        canDoubleJump = enabled;
    }

    /// <summary>
    /// 启用或禁用冲刺能力
    /// </summary>
    public void SetDashAbility(bool enabled)
    {
        canDash = enabled;
        if (!enabled)
        {
            dashState = EDashState.Exhausted;
        }
    }

    /// <summary>
    /// 启用或禁用爬墙能力
    /// </summary>
    public void SetWallHoldAbility(bool enabled)
    {
        canWallHold = enabled;
        if (!enabled && wallHoldingState != EWallHoldingState.None)
        {
            wallHoldingState = EWallHoldingState.None;
            rb.gravityScale = normalGravityScale;
            if (!physicsCheck.IsGround)
            {
                jumpState = EJumpState.AfterFirstJumpInAir;
            }
        }
    }

    /// <summary>
    /// 获取当前是否拥有二段跳能力
    /// </summary>
    public bool HasDoubleJumpAbility()
    {
        return canDoubleJump;
    }

    /// <summary>
    /// 获取当前是否拥有冲刺能力
    /// </summary>
    public bool HasDashAbility()
    {
        return canDash;
    }

    /// <summary>
    /// 获取当前是否拥有爬墙能力
    /// </summary>
    public bool HasWallHoldAbility()
    {
        return canWallHold;
    }

    #endregion

    #region 存档点系统

    /// <summary>
    /// 检测附近的检查点并解锁
    /// </summary>
    private void UpdateCheckpointDetection()
    {
        if (CheckpointManager.Instance == null) return;
        
        // 获取所有检查点
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, unlockCheckpointDistance);
        
        foreach (var collider in nearbyColliders)
        {
            Checkpoint checkpoint = collider.GetComponent<Checkpoint>();
            if (checkpoint != null && !checkpoint.IsUnlocked)
            {
                checkpoint.Unlock();
            }
        }
    }

    /// <summary>
    /// 回到最近的存档点
    /// </summary>
    public void RespawnAtNearestCheckpoint()
    {
        if (CheckpointManager.Instance == null)
        {
            Debug.LogWarning("CheckpointManager 未找到！");
            return;
        }
        
        Checkpoint nearestCheckpoint = CheckpointManager.Instance.GetNearestUnlockedCheckpoint(transform.position);
        
        if (nearestCheckpoint != null)
        {
            RespawnAtCheckpoint(nearestCheckpoint);
        }
        else
        {
            Debug.LogWarning("没有找到已解锁的检查点！");
        }
    }

    /// <summary>
    /// 在指定检查点重生
    /// </summary>
    public void RespawnAtCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null || !checkpoint.IsUnlocked)
        {
            Debug.LogWarning("无法在未解锁的检查点重生！");
            return;
        }
        
        // 重置玩家状态
        ResetPlayerState();
        
        // 移动到检查点位置
        transform.position = checkpoint.GetRespawnPosition();
        
        Debug.Log($"玩家在检查点 {checkpoint.checkpointId} 重生");
    }

    /// <summary>
    /// 重置玩家状态
    /// </summary>
    private void ResetPlayerState()
    {
        // 重置速度
        rb.velocity = Vector2.zero;
        
        // 重置状态
        isAttacking = false;
        isHit = false;
        isBeingHit = false;
        isInvincible = false;
        isInKnockback = false;
        isDashing = false;
        isAfterWallJumping = false;
        
        // 重置跳跃和爬墙状态
        jumpState = EJumpState.Ground;
        wallHoldingState = EWallHoldingState.None;
        
        // 重置攻击状态
        attackState = EAttackState.Idle;
        attackTimer = 0;
        
        // 重置冲刺充能
        if (canDash)
        {
            dashState = EDashState.Charged;
        }
        
        // 重置重力
        rb.gravityScale = normalGravityScale;
        
        // 取消所有异步操作
        wallJumpCancellationTokenSource?.Cancel();
        dashCancellationTokenSource?.Cancel();
        knockbackCancellationTokenSource?.Cancel();
        
        // 恢复可见性和颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }
        
        // 恢复时间流速
        Time.timeScale = 1f;
    }

    #endregion

    #region Debug

    private void DebugLogStates()
    {
        Debug.Log(jumpState);
        Debug.Log($"isAfterWallJumping {isAfterWallJumping}");
        Debug.Log($"wallHoldingStatus {wallHoldingState}");
        Debug.Log($"dashState {dashState}, isDashing {isDashing}");
    }

    #endregion
}

#region 枚举定义

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

#endregion
