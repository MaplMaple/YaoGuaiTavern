using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;
using Cysharp.Threading.Tasks;

public class FrontdoorSceneControl : MonoBehaviour
{
    public DialogManager DialogManager;
    private SpriteRenderer playerSpriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        playerSpriteRenderer = PlayerController.instance.GetComponent<SpriteRenderer>();
        SceneStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private async void SceneStart()
    {
        InputManager.instance.SetInputEnabled(false);
        await FadeInPlayer();
        var dialogTexts = new List<DialogData>();
        dialogTexts.Add(new DialogData("奇怪…看样子右边去楼顶的路被人堵住了，得想办法绕过去。", "主角"));
        dialogTexts.Add(new DialogData("先往左边走走看吧。", "主角", callback: () => InputManager.instance.SetInputEnabled(true)));
        DialogManager.Show(dialogTexts);
    }

    private async UniTask FadeInPlayer()
    {
        float timer = 1.5f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            playerSpriteRenderer.color = new Color(playerSpriteRenderer.color.r, playerSpriteRenderer.color.g, playerSpriteRenderer.color.b, 1 - timer);
            await UniTask.DelayFrame(1);
        }
        playerSpriteRenderer.color = new Color(playerSpriteRenderer.color.r, playerSpriteRenderer.color.g, playerSpriteRenderer.color.b, 1);
        await UniTask.Delay(500);
    }
}
