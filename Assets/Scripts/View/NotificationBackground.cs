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
            progressBarFill.DOFillAmount(1f, progressDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
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
        gameObject.SetActive(true);
        
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(true);
        }
        
        if (progressBarFill != null)
        {
            progressBarFill.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the notification background
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        
        // Also ensure background image is hidden
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
        
        // Hide progress bar as well
        if (progressBarFill != null)
        {
            progressBarFill.gameObject.SetActive(false);
        }
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