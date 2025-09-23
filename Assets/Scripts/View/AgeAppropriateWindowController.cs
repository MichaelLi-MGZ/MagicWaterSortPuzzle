using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controller for the Age Appropriate Window dialog
/// </summary>
public class AgeAppropriateWindowController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject ageAppropriateWindow;
    public Button closeButton;
    public Image ageImage; // The clickable age image on NotificationBG
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    
    
    private bool _isWindowVisible = false;
    
    /// <summary>
    /// Property to track window visibility
    /// </summary>
    public bool isWindowVisible 
    { 
        get { return _isWindowVisible; }
        set { _isWindowVisible = value; }
    }
    
    void Start()
    {
        // Ensure window starts hidden
        if (ageAppropriateWindow != null)
        {
            ageAppropriateWindow.SetActive(false);
            isWindowVisible = false;
        }
        else
        {
            Debug.LogError("AgeAppropriateWindowController: Age Appropriate Window GameObject not assigned!");
            return;
        }
        
        // Set up click events
        SetupClickEvents();
    }
    
    /// <summary>
    /// Set up all click events
    /// </summary>
    private void SetupClickEvents()
    {
        // Set up age image click event
        if (ageImage != null)
        {
            // Add Button component if it doesn't exist
            Button ageButton = ageImage.GetComponent<Button>();
            if (ageButton == null)
            {
                ageButton = ageImage.gameObject.AddComponent<Button>();
            }
            
            // Set up click event
            ageButton.onClick.RemoveAllListeners();
            ageButton.onClick.AddListener(OnAgeImageClicked);
        }
        else
        {
            Debug.LogError("AgeAppropriateWindowController: Age image not assigned!");
        }
        
        // Set up close button click event
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            Debug.Log("AgeAppropriateWindowController: Close button click event set up");
        }
        else
        {
            Debug.LogError("AgeAppropriateWindowController: Close button not assigned!");
        }
    }
    
    /// <summary>
    /// Called when age image is clicked
    /// </summary>
    public void OnAgeImageClicked()
    {
        Debug.Log("AgeAppropriateWindowController: Age image clicked");
        
        if (!isWindowVisible)
        {
            ShowWindow();
        }
        else
        {
            Debug.Log("AgeAppropriateWindowController: Window is already visible");
        }
    }
    
    /// <summary>
    /// Called when close button is clicked
    /// </summary>
    public void OnCloseButtonClicked()
    {
        Debug.Log("AgeAppropriateWindowController: Close button clicked");
        HideWindow();
    }
    
    /// <summary>
    /// Show the age appropriate window with animation
    /// </summary>
    public void ShowWindow()
    {
        if (ageAppropriateWindow == null)
        {
            Debug.LogError("AgeAppropriateWindowController: Age appropriate window not assigned!");
            return;
        }
        
        // Prevent multiple rapid calls
        if (isWindowVisible)
        {
            return;
        }
        
        // Activate the window
        ageAppropriateWindow.SetActive(true);
        isWindowVisible = true;
        
        // Get the main panel for animation
        CanvasGroup canvasGroup = ageAppropriateWindow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = ageAppropriateWindow.AddComponent<CanvasGroup>();
        }
        
        // Set initial state
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // Animate in
        canvasGroup.DOFade(1f, fadeInDuration)
            .SetEase(Ease.OutQuad);
    }
    
    /// <summary>
    /// Hide the age appropriate window with animation
    /// </summary>
    public void HideWindow()
    {
        if (ageAppropriateWindow == null)
        {
            Debug.LogError("AgeAppropriateWindowController: Age appropriate window not assigned!");
            return;
        }
        
        
        // Get the main panel for animation
        CanvasGroup canvasGroup = ageAppropriateWindow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = ageAppropriateWindow.AddComponent<CanvasGroup>();
        }
        
        // Disable interaction during animation
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Animate out
        canvasGroup.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                // Deactivate the window
                ageAppropriateWindow.SetActive(false);
                isWindowVisible = false;
            });
    }
    
    /// <summary>
    /// Toggle window visibility
    /// </summary>
    public void ToggleWindow()
    {
        if (isWindowVisible)
        {
            HideWindow();
        }
        else
        {
            ShowWindow();
        }
    }
    
    /// <summary>
    /// Check if window is currently visible
    /// </summary>
    /// <returns>True if visible, false if hidden</returns>
    public bool IsWindowVisible()
    {
        // Double-check the actual GameObject state
        bool actualGameObjectState = ageAppropriateWindow != null && ageAppropriateWindow.activeInHierarchy;
        
        if (isWindowVisible != actualGameObjectState)
        {
            // Sync the state
            isWindowVisible = actualGameObjectState;
        }
        
        return isWindowVisible;
    }
    
    /// <summary>
    /// Force refresh the window visibility state based on actual GameObject state
    /// </summary>
    public void RefreshVisibilityState()
    {
        if (ageAppropriateWindow != null)
        {
            bool actualState = ageAppropriateWindow.activeInHierarchy;
            if (isWindowVisible != actualState)
            {
                isWindowVisible = actualState;
            }
        }
    }
    
    
}