using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;

public class PickupAbility : MonoBehaviour
{
    public DialogManager DialogManager;
    private bool isTriggered = false;
    public string dialogueContent;
    public string abilityName;
    private void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;
        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            InputManager.instance.SetInputEnabled(false);
            PlayerController.instance.inputDirection = Vector2.zero;
            var dialogTexts = new List<DialogData>();
            dialogTexts.Add(new DialogData(dialogueContent, callback: () => InputManager.instance.SetInputEnabled(true)));
            DialogManager.Show(dialogTexts);
            switch (abilityName)
            {
                case "DoubleJump":
                    other.GetComponent<PlayerController>().SetDoubleJumpAbility(true);
                    break;
                case "WallHold":
                    other.GetComponent<PlayerController>().SetWallHoldAbility(true);
                    break;
                case "Dash":
                    other.GetComponent<PlayerController>().SetDashAbility(true);
                    break;
            }
            // GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
}
