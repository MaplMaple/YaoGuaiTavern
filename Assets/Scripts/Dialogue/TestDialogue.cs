using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;

public class TestDialogue : MonoBehaviour
{
    // Start is called before the first frame update
    public DialogManager dialogManager;
    void Start()
    {
        DialogData dialogData = new DialogData("Hellow world", "Protagonist");

        dialogManager.Show(dialogData);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
