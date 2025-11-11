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
            dialogTexts.Add(new DialogData("呼…我记得我摸了一下那张符咒，再次醒来的时候就到这个地方了……", "主角"));
            dialogTexts.Add(new DialogData("这个地方到处都是熊熊燃烧的蓝色火焰，我得尽快找到出去的路。", "主角"));
            dialogTexts.Add(new DialogData("我最好不要碰到这些钢筋尖刺和上面飞着的怪物。", "主角"));
            dialogTexts.Add(new DialogData("等等，护身符……", "主角"));
            dialogTexts.Add(new DialogData("木樨的护身符仿佛和幻境共鸣，正在猛烈地震动！", "信件"));
            dialogTexts.Add(new DialogData("感觉有一股力量正在注入身体，是这个护身符在保护我吗？也许我可以借助这股力量离开这里。", "主角"));
            dialogTexts.Add(new DialogData("已习得灵火冲刺，按下 C 键进行冲刺。", callback: () => InputManager.instance.SetInputEnabled(true)));
            DialogManager.Show(dialogTexts);
            other.GetComponent<PlayerController>().SetDashAbility(true);
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
}
