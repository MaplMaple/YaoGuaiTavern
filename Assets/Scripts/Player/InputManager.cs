using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class InputManager : MonoBehaviour
{
    private void Update()
    {
        if (PlayerController.instance == null) return;
        int horizontalMoveInput = 0;
        int verticalMoveInput = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalMoveInput -= 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalMoveInput += 1;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            verticalMoveInput += 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalMoveInput -= 1;
        }
        PlayerController.instance.SetMoveInput(new Vector2(horizontalMoveInput, verticalMoveInput));
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayerController.instance.OnPressJump();
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            PlayerController.instance.OnReleaseJump();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerController.instance.OnPressAttack();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayerController.instance.OnPressDash();
        }
    }
}