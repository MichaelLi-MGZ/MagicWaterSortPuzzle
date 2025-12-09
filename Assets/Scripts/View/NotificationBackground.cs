 using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles the notification background with progress bar for MySDK initialization
/// </summary>
public class NotificationBackground : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    public Image progressBarFill;
    public float progressDuration = 8.0f; // 8 seconds
    
    [Header("Background Settings")]
    public Image backgroundImage;
    
    private System.Action onProgressComplete;
    
    void Start()
    {
        // Ensure background is visible
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(true);
        }
        
        // Initialize progress bar
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
            progressBarFill.gameObject.SetActive(true);
        }
        
    }
    
    /// <summary>
    /// Start the progress bar animation and call callback when complete
    /// </summary>
    /// <param name="onComplete">Callback to call when progress reaches 100%</param>
    public void StartProgress(System.Action onComplete)
    {
        onProgressComplete = onComplete;
        
        Debug.Log("=== NotificationBackground.StartProgress() called ===");
        
        if (progressBarFill != null)
        {
            Debug.Log($"✅ Progress bar fill image found: {progressBarFill.name}");
            Debug.Log($"   - Image Type: {progressBarFill.type}");
            Debug.Log($"   - Fill Method: {progressBarFill.fillMethod}");
            Debug.Log($"   - Current Fill Amount: {progressBarFill.fillAmount}");
            Debug.Log($"   - Duration: {progressDuration} seconds");
            
            // Check if Image Type is Filled
            if (progressBarFill.type != Image.Type.Filled)
            {
                Debug.LogError($"❌ CRITICAL: Image Type is NOT 'Filled'! Current: {progressBarFill.type}");
                Debug.LogError("🔧 FIX: Set the Image Type to 'Filled' in the Image component!");
                onProgressComplete?.Invoke();
                return;
            }
            
            // Reset to 0
            progressBarFill.fillAmount = 0f;
            
            // Animate progress bar from 0 to 100% over the specified duration
            Debug.Log($"[NotificationBackground] Starting DOFillAmount animation: 0 -> 1 over {progressDuration} seconds");
            progressBarFill.DOFillAmount(1f, progressDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    Debug.Log("[NotificationBackground] Progress bar animation completed (100%), invoking onProgressComplete callback");
                    Debug.Log($"[NotificationBackground] Final fill amount: {progressBarFill.fillAmount}");
                    onProgressComplete?.Invoke();
                });
        }
        else
        {
            Debug.LogError("❌ NotificationBackground: Progress bar fill image not assigned!");
            Debug.LogError("🔧 FIX: Assign the progress bar Image to the NotificationBackground component");
            // Call immediately if no progress bar
            onProgressComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Show the notification background
    /// </summary>
    public void Show()
    {
        Debug.Log("[NotificationBackground] Show() called");
        Debug.Log($"[NotificationBackground] Current active state before Show(): {gameObject.activeSelf}");
        
        gameObject.SetActive(true);
        
        Debug.Log($"[NotificationBackground] GameObject activated: {gameObject.activeSelf}");
        
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(true);
            Debug.Log($"[NotificationBackground] Background image activated: {backgroundImage.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[NotificationBackground] Background image is null");
        }
        
        if (progressBarFill != null)
        {
            progressBarFill.gameObject.SetActive(true);
            Debug.Log($"[NotificationBackground] Progress bar fill activated: {progressBarFill.gameObject.activeSelf}, current fill: {progressBarFill.fillAmount}");
        }
        else
        {
            Debug.LogWarning("[NotificationBackground] Progress bar fill is null");
        }
        
        Debug.Log($"[NotificationBackground] IsVisible() after Show(): {IsVisible()}");
    }
    
    /// <summary>
    /// Hide the notification background
    /// </summary>
    public void Hide()
    {
        Debug.Log("[NotificationBackground] Hide() called");
        Debug.Log($"[NotificationBackground] Current active state before Hide(): {gameObject.activeSelf}");
        
        gameObject.SetActive(false);
        
        Debug.Log($"[NotificationBackground] GameObject deactivated: {gameObject.activeSelf}");
        
        // Also ensure background image is hidden
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
            Debug.Log("[NotificationBackground] Background image deactivated");
        }
        
        // Hide progress bar as well
        if (progressBarFill != null)
        {
            progressBarFill.gameObject.SetActive(false);
            Debug.Log($"[NotificationBackground] Progress bar fill deactivated, final fill amount: {progressBarFill.fillAmount}");
        }
        
        Debug.Log($"[NotificationBackground] IsVisible() after Hide(): {IsVisible()}");
    }
    
    /// <summary>
    /// Set the progress bar fill amount manually (0-1)
    /// </summary>
    /// <param name="amount">Fill amount from 0 to 1</param>
    public void SetProgress(float amount)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = Mathf.Clamp01(amount);
        }
    }
    
    /// <summary>
    /// Get current progress (0-1)
    /// </summary>
    /// <returns>Current progress amount</returns>
    public float GetProgress()
    {
        if (progressBarFill != null)
        {
            return progressBarFill.fillAmount;
        }
        return 0f;
    }
    
    /// <summary>
    /// Check if notification background is currently visible
    /// </summary>
    /// <returns>True if visible, false if hidden</returns>
    public bool IsVisible()
    {
        return gameObject.activeInHierarchy;
    }
    
    
}