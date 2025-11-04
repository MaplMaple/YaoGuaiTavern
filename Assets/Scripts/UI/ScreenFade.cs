using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

/// <summary>
/// 屏幕渐变效果管理器
/// 用于死亡、传送等场景转换
/// </summary>
public class ScreenFade : MonoBehaviour
{
    #region 单例
    private static ScreenFade instance;
    public static ScreenFade Instance => instance;
    #endregion

    [Header("渐变设置")]
    [Tooltip("渐变图像")]
    public Image fadeImage;
    
    [Tooltip("渐变颜色")]
    public Color fadeColor = Color.black;
    
    private Canvas canvas;
    private CancellationTokenSource fadeCancellationTokenSource;

    private void Awake()
    {
        InitializeSingleton();
        InitializeCanvas();
        InitializeFadeImage();
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCanvas()
    {
        // 获取或创建 Canvas
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // 确保在最上层
        
        // 添加 CanvasScaler
        if (GetComponent<CanvasScaler>() == null)
        {
            gameObject.AddComponent<CanvasScaler>();
        }
        
        // 添加 GraphicRaycaster
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void InitializeFadeImage()
    {
        if (fadeImage == null)
        {
            // 创建全屏黑色图像
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(transform);
            
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0); // 初始透明
            
            // 设置为全屏
            RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            // 确保初始透明
            Color color = fadeImage.color;
            color.a = 0;
            fadeImage.color = color;
        }
    }

    private void OnDestroy()
    {
        fadeCancellationTokenSource?.Cancel();
        fadeCancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// 渐变到黑屏再恢复
    /// </summary>
    /// <param name="duration">总时长（秒）</param>
    public async UniTask FadeOutAndIn(float duration)
    {
        // 取消之前的渐变
        fadeCancellationTokenSource?.Cancel();
        fadeCancellationTokenSource?.Dispose();
        fadeCancellationTokenSource = new CancellationTokenSource();

        float halfDuration = duration / 2f;

        try
        {
            // 渐变到黑色
            await FadeToBlack(halfDuration, fadeCancellationTokenSource.Token);
            
            // 渐变到透明
            await FadeToClear(halfDuration, fadeCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // 被取消时的处理
            Debug.Log("屏幕渐变被取消");
        }
    }

    /// <summary>
    /// 渐变到黑屏
    /// </summary>
    private async UniTask FadeToBlack(float duration, CancellationToken cancellationToken)
    {
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime 避免受 Time.timeScale 影响
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            
            await UniTask.Yield(cancellationToken);
        }

        fadeImage.color = targetColor;
    }

    /// <summary>
    /// 渐变到透明
    /// </summary>
    private async UniTask FadeToClear(float duration, CancellationToken cancellationToken)
    {
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            
            await UniTask.Yield(cancellationToken);
        }

        fadeImage.color = targetColor;
    }

    /// <summary>
    /// 立即设置为黑屏
    /// </summary>
    public void SetBlack()
    {
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    /// <summary>
    /// 立即设置为透明
    /// </summary>
    public void SetClear()
    {
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
    }
}

