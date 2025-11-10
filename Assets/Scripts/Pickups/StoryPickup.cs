using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;

public class StoryPickup : MonoBehaviour
{
    public DialogManager DialogManager;
    private bool isTriggered = false;
    public List<string> texts;
    public List<string> characters;
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
            for (int i = 0; i < texts.Count; i++)
            {
                if (i != texts.Count - 1)
                {
                    dialogTexts.Add(new DialogData(texts[i], characters[i]));
                }
                else
                {
                    dialogTexts.Add(new DialogData(texts[i], characters[i], callback: () => InputManager.instance.SetInputEnabled(true)));
                }
            }
            DialogManager.Show(dialogTexts);
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
}
