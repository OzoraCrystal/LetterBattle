using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Image foregroundHealthBar; // 主血条（填充图片）
    public Image backgroundHealthBar; // 缓冲血条（填充图片）
    public TextMeshProUGUI playerHealthText; // 显示血量数值的文本

    public float smoothSpeed = 1f; // 缓降速度

    private void Awake()
    {
        if (foregroundHealthBar == null || backgroundHealthBar == null)
        {
            Debug.LogError("缺少血条图片!");
        }
    }

    // 绑定玩家血量管理器
    public void BindPlayerHealth(HealthManager playerHealthManager)
    {
        if (playerHealthManager == null)
        {
            Debug.LogError("玩家的 HealthManager 为空!");
            return;
        }

        playerHealthManager.onHealthChanged += UpdatePlayerHealthBar; // 监听血量变化事件
    }

    // 更新玩家血量条
    private void UpdatePlayerHealthBar(float currentHealth, float maxHealth)
    {
        float healthRatio = currentHealth / maxHealth;

        if (foregroundHealthBar != null)
        {
            foregroundHealthBar.fillAmount = healthRatio; // 主血条立即更新
        }

        if (backgroundHealthBar != null)
        {
            StopAllCoroutines(); // 停止上一个缓降协程，防止冲突
            StartCoroutine(SmoothHealthBar(backgroundHealthBar, healthRatio));
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"{currentHealth}/{maxHealth}"; // 更新血量文字
        }
    }

    // 缓降血条协程
    private IEnumerator SmoothHealthBar(Image bar, float targetFillAmount)
    {
        while (Mathf.Abs(bar.fillAmount - targetFillAmount) > 0.01f)
        {
            bar.fillAmount = Mathf.Lerp(bar.fillAmount, targetFillAmount, smoothSpeed * Time.deltaTime);
            yield return null; // 等待下一帧
        }

        bar.fillAmount = targetFillAmount; // 确保最后精确对齐
    }
}
