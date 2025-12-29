using UnityEngine;
using UnityEngine.UI;

public class ImageViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject imageViewerPanel; // 图片查看面板
    [SerializeField] private Image displayImage; // 显示图片的Image组件
    [SerializeField] private Button nextButton; // 下一张按钮
    [SerializeField] private Button previousButton; // 上一张按钮
    
    [Header("Toggle Button")]
    [SerializeField] private Button toggleButton; // 切换按钮（打开/关闭）
    
    [Header("Image Collection")]
    [SerializeField] private Sprite[] imageCollection; // 图片合集
    
    private int currentImageIndex = 0; // 当前显示的图片索引
    private bool isViewerOpen = false; // 图片查看器是否打开
    
    private void Start()
    {
        // 绑定切换按钮事件
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleImageViewer);
            
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextImage);
            
        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousImage);
            
        // 初始隐藏面板
        if (imageViewerPanel != null)
            imageViewerPanel.SetActive(false);
    }
    
    public void ToggleImageViewer()
    {
        if (isViewerOpen)
        {
            CloseImageViewer();
        }
        else
        {
            OpenImageViewer();
        }
    }
    
    public void OpenImageViewer()
    {
        if (imageCollection == null || imageCollection.Length == 0)
        {
            Debug.LogWarning("没有图片可以显示！");
            return;
        }
        
        isViewerOpen = true;
        
        // 使用暂停机制禁用游戏交互
        PauseController.SetPause(true);
        
        // 重置到第一张图片
        currentImageIndex = 0;
        
        // 显示当前图片
        UpdateDisplayImage();
        
        // 更新按钮状态
        UpdateButtonStates();
        
        // 显示面板
        if (imageViewerPanel != null)
            imageViewerPanel.SetActive(true);
    }
    
    public void ShowNextImage()
    {
        if (imageCollection == null || imageCollection.Length == 0) return;
        
        // 切换到下一张图片
        currentImageIndex = (currentImageIndex + 1) % imageCollection.Length;
        
        UpdateDisplayImage();
        UpdateButtonStates();
    }
    
    public void ShowPreviousImage()
    {
        if (imageCollection == null || imageCollection.Length == 0) return;
        
        // 切换到上一张图片
        currentImageIndex = (currentImageIndex - 1 + imageCollection.Length) % imageCollection.Length;
        
        UpdateDisplayImage();
        UpdateButtonStates();
    }
    
    public void CloseImageViewer()
    {
        isViewerOpen = false;
        
        // 恢复游戏（取消暂停）
        PauseController.SetPause(false);
        
        if (imageViewerPanel != null)
            imageViewerPanel.SetActive(false);
    }
    
    private void UpdateDisplayImage()
    {
        if (displayImage != null && imageCollection != null && currentImageIndex < imageCollection.Length)
        {
            displayImage.sprite = imageCollection[currentImageIndex];
        }
    }
    
    private void UpdateButtonStates()
    {
        if (imageCollection == null || imageCollection.Length <= 1)
        {
            // 如果只有一张图或没有图，禁用导航按钮
            if (nextButton != null)
                nextButton.interactable = false;
            if (previousButton != null)
                previousButton.interactable = false;
        }
        else
        {
            // 有多张图时启用导航按钮
            if (nextButton != null)
                nextButton.interactable = true;
            if (previousButton != null)
                previousButton.interactable = true;
        }
    }
    
    private void OnDestroy()
    {
        // 清理事件绑定
        if (toggleButton != null)
            toggleButton.onClick.RemoveAllListeners();
        if (nextButton != null)
            nextButton.onClick.RemoveAllListeners();
        if (previousButton != null)
            previousButton.onClick.RemoveAllListeners();
            
        // 确保销毁时恢复游戏状态
        PauseController.SetPause(false);
    }
}
