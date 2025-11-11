using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;
using UnityEngine.SceneManagement;

public class EndScenePickup : MonoBehaviour
{
    public DialogManager DialogManager;
    private bool isTriggered = false;
    private void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;
        if (other.CompareTag("Player") && other.GetComponent<PlayerController>().HasDashAbility())
        {
            InputManager.instance.SetInputEnabled(false);
            PlayerController.instance.inputDirection = Vector2.zero;
            var dialogTexts = new List<DialogData>();
            dialogTexts.Add(new DialogData("游戏结束，感谢你的游玩。", callback: () => { SceneManager.LoadScene("MainMenu"); }));
            DialogManager.Show(dialogTexts);
            isTriggered = true;
        }
    }
}
