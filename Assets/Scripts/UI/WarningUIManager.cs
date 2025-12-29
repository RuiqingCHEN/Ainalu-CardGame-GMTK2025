using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class WarningUIManager : Singleton<WarningUIManager>
{
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text warningMessageText; // 新增：显示具体警告信息的文本
    
    private CanvasGroup canvasGroup;
    
    private void Start()
    {
        // 确保有CanvasGroup组件
        canvasGroup = warningPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = warningPanel.AddComponent<CanvasGroup>();
        }
        
        // 绑定关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseWarning);
        }
        
        // 初始隐藏
        warningPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseWarning);
        }
    }
    
    public void ShowWarning()
    {
        // 检查当前场景是否为Level3
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Level3")
        {
            return; // Level3不显示警告
        }
        
        // 更新警告信息文本
        UpdateWarningMessage();
        
        // 暂停其他交互
        Interactions.Instance.SetWarningUIActive(true);
        
        // 显示面板
        warningPanel.SetActive(true);
        
        // 淡入动画
        canvasGroup.alpha = 0;
        warningPanel.transform.localScale = Vector3.one * 0.8f;
        
        canvasGroup.DOFade(1, 0.3f);
        warningPanel.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }
    
    // 新增：更新警告信息
    private void UpdateWarningMessage()
    {
        if (warningMessageText != null && TurnCardTracker.Instance != null)
        {
            string message = TurnCardTracker.Instance.GetWarningMessage();
            if (string.IsNullOrEmpty(message))
            {
                message = "每回合必须出且仅出2张右边卡牌！";
            }
            warningMessageText.text = message;
        }
    }
    
    public void CloseWarning()
    {
        // 淡出动画
        canvasGroup.DOFade(0, 0.2f);
        warningPanel.transform.DOScale(0.8f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                warningPanel.SetActive(false);
                // 恢复交互
                Interactions.Instance.SetWarningUIActive(false);
            });
    }
    
    // 修改：供外部调用以自动关闭警告（当满足条件时）
    public void TryAutoCloseWarning()
    {
        if (warningPanel.activeInHierarchy && TurnCardTracker.Instance != null && TurnCardTracker.Instance.CanEndTurn())
        {
            CloseWarning();
        }
    }
    
    // 检查警告是否正在显示
    public bool IsWarningShowing()
    {
        return warningPanel.activeInHierarchy;
    }
}