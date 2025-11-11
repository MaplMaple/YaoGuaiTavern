using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doublsb.Dialog;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class BarOpening : MonoBehaviour
{
    // public Transform playerSpawnPosition;
    public Transform playerNextPosition;
    public DialogManager DialogManager;
    public GameObject Player;
    public List<Light2D> lights;
    private SpriteRenderer playerSpriteRenderer;

    private void Awake()
    {
        playerSpriteRenderer = Player.GetComponent<SpriteRenderer>();
        playerSpriteRenderer.color = new Color(playerSpriteRenderer.color.r, playerSpriteRenderer.color.g, playerSpriteRenderer.color.b, 0);
    }

    private void Start()
    {
        PlayOpeningDialogue();
    }

    private void PlayOpeningDialogue()
    {
        var dialogTexts = new List<DialogData>();
        dialogTexts.Add(new DialogData(originalString: "几天前，我收到了我的朋友木樨发来的一封信件。", character: "黑屏"));
        dialogTexts.Add(new DialogData(originalString: "虽然没有寄出者的信息，但是我很确信这是她寄来的，因为随信而来的还有我在学生时代赠送给她的护身符。", character: "黑屏"));
        dialogTexts.Add(new DialogData(originalString: "信的内容几乎完全由我看不懂的文字进行书写，但落款处留下了一个地址，指向雨城的一家小酒馆。", character: "黑屏"));
        dialogTexts.Add(new DialogData(originalString: "为了调查这封信以及木樨发这封信给我的原因，我启程前往雨城。至于木樨那边到底发生了什么事，也许只有我到达酒馆之后才能知道了。", callback: () => { FadeInPlayer().Forget(); }, character: "黑屏"));
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
        PlayerMonologue();
    }

    private void PlayerMonologue()
    {
        var dialogTexts = new List<DialogData>();
        dialogTexts.Add(new DialogData(originalString: "门没锁……以木樨的性格会不锁门吗？说起来，她叫我来到底是为了什么呢……信上的地址没错，就是这里。不过这地方已经落灰了，木樨是遇到什么事了吗？", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "哟，看看谁来了，这不是我们的大作家嘛。", character: "???"));
        dialogTexts.Add(new DialogData(originalString: "谁在说话？", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "（等等，她是...）", character: "主角", callback: () => { PlayerWalkToFridge().Forget(); }));
        dialogTexts.Add(new DialogData(originalString: "我脑子里的问题现在越来越多了。说说吧，为什么你会在这里？木樨呢？", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "哼，你还是跟以前一样，老同学多年不见，不打声招呼就算了，态度还这么冷淡……", character: "???"));
        dialogTexts.Add(new DialogData(originalString: "（眼前这位名叫阿枫的人，和木樨一样，是我在学生时代的好友，毕业后我们的关系依然很好。不过，此时此刻，为什么她会在木樨的酒馆里呢？）", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "/emote:Blink2/说起来，为什么会在这，这句话该我问你才对。我可比你先到。怎么，木樨没告诉你，她会邀请别的客人吗？", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "很抱歉，她只给我寄了一封信。", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "（我拿出信件给阿枫观看）", character: "信件"));
        dialogTexts.Add(new DialogData(originalString: "看看这封信，除了这个地址外，就是大段大段的我看不懂的文字。你知道些什么吗？", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "/emote:Normal/我知道的……只有木樨失踪了。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "什么！？", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "有好几个月了……我给她发消息不回，打电话不接，酒馆也一直关门。就在我以为她真的人间蒸发的时候，也收到了一封和她寄给你类似的信。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "（我们都收到了信。这不是巧合，是某种邀请？或者说，是求救信号？）", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "所以你才会来这里。", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "/emote:Speak/不然呢？总不能干等着。这地方虽然又小又破，但好歹是木樨开的，我想着或许能找到点什么线索。可惜，如你所见，除了满屋子的灰，也没有别的东西了。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "这里的一切，都和她失踪前一样吗？", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "/emote:Normal/差不多吧。或许比以前更冷清了。以前好歹还有我这个酒鬼来捧场，现在连个鬼影都没有。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "/emote:Blink1/说起来，这还有张转让证明呢，你现在可是这个酒馆的新老板了。不请我喝一杯，作为重逢的贺礼？", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "现在不是喝酒的时候吧……", character: "主角", callback: () => { ScreenBlink().Forget(); }));
        // TODO: 播放灯管闪烁的音效，画面闪烁几下
        dialogTexts.Add(new DialogData(originalString: "/emote:Shock//speed:0.1/哇！搞什么？这地方电力系统都坏了吗？", character: "阿枫", isSkipable: false));
        // TODO: 播放"噼啪"的电流声音效，画面彻底变暗
        dialogTexts.Add(new DialogData(originalString: "……停电了。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "我去楼顶看看，可能是太阳能电力系统出了问题。你在这里别乱跑。", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "/emote:Shock/喂，你一个人去？现在都几点了，外面黑灯瞎火的。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "没事，我很快就回来。", character: "主角"));
        // TODO: 阿枫走到冰箱前面，切换为后背sprite，过了一会，向主角走来
        dialogTexts.Add(new DialogData(originalString: "/emote:Speak/拿上这个吧。最近雨城大大小小的有一系列怪事，注意安全哦。", character: "阿枫"));
        dialogTexts.Add(new DialogData(originalString: "获得武器【菜刀】", character: "菜刀"));
        dialogTexts.Add(new DialogData(originalString: "这把刀很锋利，也许可以用来防身。（按X进行攻击）", character: "主角"));
        dialogTexts.Add(new DialogData(originalString: "放心吧，我只是上去看看而已，大概不会有什么问题吧。", character: "主角", callback: () => { LoadScene(); }));
        DialogManager.Show(dialogTexts);
    }

    private async UniTask PlayerWalkToFridge()
    {
        var playerController = Player.GetComponent<PlayerController>();
        while (Player.transform.position.x < playerNextPosition.position.x)
        {
            playerController.inputDirection = new Vector2(1f, 0);
            await UniTask.DelayFrame(1);
        }
        playerController.inputDirection = new Vector2(-1f, 0);
        await UniTask.DelayFrame(10);
        playerController.inputDirection = new Vector2(0, 0);
    }

    private async UniTask ScreenBlink()
    {
        float intensity1 = 50f;
        float intensity2 = 20f;
        foreach (var light in lights)
        {
            light.intensity = intensity1;
        }
        await UniTask.Delay(200);
        foreach (var light in lights)
        {
            light.intensity = 1f;
        }
        await UniTask.Delay(300);
        foreach (var light in lights)
        {
            light.intensity = intensity2;
        }
        await UniTask.Delay(200);
        foreach (var light in lights)
        {
            light.intensity = intensity1;
        }
        await UniTask.Delay(300);
        foreach (var light in lights)
        {
            light.intensity = intensity2;
        }
        await UniTask.Delay(100);
        foreach (var light in lights)
        {
            light.intensity = 0f;
        }
    }

    private void LoadScene()
    {
        SceneManager.LoadScene("PaoPao Frontdoor");
    }
}
