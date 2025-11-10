using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;

public class DashPickup : MonoBehaviour
{
    public DialogManager DialogManager;
    private bool isTriggered = false;
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
            dialogTexts.Add(new DialogData("已习得冲刺能力，按下 C 键进行冲刺。", callback: () => InputManager.instance.SetInputEnabled(true)));
            DialogManager.Show(dialogTexts);
            other.GetComponent<PlayerController>().SetDashAbility(true);
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
}
