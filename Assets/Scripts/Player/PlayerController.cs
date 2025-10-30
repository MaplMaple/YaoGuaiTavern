using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    public Vector2 inputDirection;
    public bool isRun = false; 
    [Header("基本参数")]
    public float speed;
    public float jumpForce;
    private PhysicsCheck physicsCheck;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputControl = new PlayerInputControl();

        physicsCheck = GetComponent<PhysicsCheck>();

        inputControl.Gameplay.Jump.started += Jump;

        inputControl.Gameplay.Run.started += RunStarted;
        inputControl.Gameplay.Run.canceled += RunCanceled;
    }

    private void OnEnable()
    {
        inputControl.Enable();
    }

    private void Ondisable()
    {
        inputControl.Disable();
    }

    private void Update()
    {
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void Move()
    {
        rb.velocity = new Vector2(Time.deltaTime * speed * inputDirection.x, rb.velocity.y);

        int faceDir = (int)transform.localScale.x;

        if (inputDirection.x > 0)
            faceDir = 1;
        if (inputDirection.x < 0)
            faceDir = -1;

        if (isRun == true)
        {
            rb.velocity += new Vector2(1.2f * Time.deltaTime * speed * inputDirection.x,0);
        }
        //人物翻转
        transform.localScale = new Vector3(faceDir, 1, 1);
    }

    private void RunStarted(InputAction.CallbackContext context)
    {
        isRun = true;
    }
    private void RunCanceled(InputAction.CallbackContext context)
    {
        isRun = false;
    }
    private void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log("Jump");
        if(physicsCheck.isGround)
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        
    }

}
