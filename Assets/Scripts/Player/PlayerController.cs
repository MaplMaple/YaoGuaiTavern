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
    public float wallJumpHorizontalVelocity;
    public float wallJumpVerticalVelocity;
    public float doubleJumpVelocity;
    public float horziontalMoveSpeed;

    private Rigidbody2D rb;
    public Vector2 inputDirection;
    private float faceDirection = 1;
    private PhysicsCheck physicsCheck;
    [SerializeField] private EJumpState jumpState = EJumpState.Ground;


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
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void SetMoveInput(Vector2 inputDirection)
    {
        this.inputDirection = inputDirection;
    }

    private void Move()
    {
        if (!Mathf.Approximately(inputDirection.x, 0))
        {
            faceDirection = inputDirection.x;
        }
        float horizontalSpeed = horziontalMoveSpeed * inputDirection.x;
        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);
        Debug.Log(rb.velocity);
    }

    private void OnTouchGround()
    {
        jumpState = EJumpState.Ground;
    }

    public void OnPressJump()
    {
        if (jumpState == EJumpState.Ground)
        {
            jumpState = EJumpState.AfterFirstJump;
            rb.velocity = new Vector2(rb.velocity.x, groundJumpVelocity);
        }
        else if (jumpState == EJumpState.WallLeft)
        {
            jumpState = EJumpState.AfterFirstJump;
            rb.velocity = new Vector2(wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
        }
        else if (jumpState == EJumpState.WallRight)
        {
            jumpState = EJumpState.AfterFirstJump;
            rb.velocity = new Vector2(-wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
        }
        else if (jumpState == EJumpState.AfterFirstJump)
        {
            jumpState = EJumpState.Exhausted;
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpVelocity);
        }
    }

    public void OnReleaseJump()
    {
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y * releaseJumpSpeedDeclineRate);
        }
    }

    public void Attack()
    {
        
    }
}

public enum EJumpState
{
    Ground,
    WallLeft,
    WallRight,
    AfterFirstJump,
    Exhausted,
}