using System;
using System.Collections;
using System.Collections.Generic;
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
    public float attackInterval;
    public EAttackState attackState = EAttackState.Idle;
    public bool showAttackGizmos = true;
    public GameObject attackEffect;
    public Transform attackCenter;
    public LayerMask attackLayer;

    private Rigidbody2D rb;
    public Vector2 inputDirection;
    private float faceDirection = 1;
    private float attackTimer = 0;
    private PhysicsCheck physicsCheck;
    [SerializeField] private EJumpState jumpState = EJumpState.Ground;

    // Gizmos绘制相关
    private bool shouldDrawGizmos = false;
    private Vector2 gizmosHitboxCenter;
    private Vector2 gizmosHitboxSize;
    private float gizmosStartTime;
    private const float gizmosDrawDuration = 1.0f;

    private const float releaseJumpSpeedDeclineRate = 0.8f;

    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        physicsCheck = GetComponent<PhysicsCheck>();
        physicsCheck.TouchGround += OnTouchGround;
    }

    private void Update()
    {
        transform.localScale = new Vector3(faceDirection, 1, 1);
        UpdateAttack();
        UpdateGizmosTimer();
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
        float horizontalSpeed = horziontalMoveSpeed * inputDirection.x;
        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);
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
        else if (jumpState == EJumpState.HoldingLeftWall)
        {
            jumpState = EJumpState.AfterFirstJumpInAir;
            rb.velocity = new Vector2(wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
        }
        else if (jumpState == EJumpState.HoldingRightWall)
        {
            jumpState = EJumpState.AfterFirstJumpInAir;
            rb.velocity = new Vector2(-wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
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
}

public enum EJumpState
{
    Ground,
    HoldingLeftWall,
    HoldingRightWall,
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