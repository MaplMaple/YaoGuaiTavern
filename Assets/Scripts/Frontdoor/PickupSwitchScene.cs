using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;
using UnityEngine.SceneManagement;

public class PickupSwitchScene : MonoBehaviour
{
    public DialogManager DialogManager;
    private bool isTriggered = false;
    public string dialogueContent;
    public string sceneName;
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
            dialogTexts.Add(new DialogData(dialogueContent, isSkipable: false, callback: () => { InputManager.instance.SetInputEnabled(true); SceneManager.LoadScene(sceneName); }));
            DialogManager.Show(dialogTexts);
        }
    }
}
