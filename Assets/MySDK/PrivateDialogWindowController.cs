using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PrivateDialogWindowController : MonoBehaviour
{
    public GameObject dialog;

    public TMPro.TMP_Text rightText;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text msgText;
    public UnityEngine.UI.Button btnRight;
    
    [Header("WebView Integration")]
    public WebViewComponent webViewComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        hide();
        
        // Initialize WebView if not already done
        if (webViewComponent == null)
        {
            InitializeWebView();
        }
        
        // Hide WebView initially - it should only show when dialog is shown
        if (webViewComponent != null)
        {
            webViewComponent.Hide();
        }
    }

    public void show()
    {
        dialog.SetActive(true);
        
        // Show WebView
        if (webViewComponent != null)
        {
            webViewComponent.Show();
        }
    }
    

    public void hide()
    {
        dialog.SetActive(false);
        
        // Hide WebView
        if (webViewComponent != null)
        {
            webViewComponent.Hide();
        }
    }

    public void setRightButtonActive(bool active)
    {
        btnRight.gameObject.SetActive(active);
    }

    public void setRightText(string text)
    {
        rightText.text = text;
    }

    public void setTitleText(string text)
    {
        titleText.text = text;
    }

    public void setMessageText(string text)
    {
        msgText.text = text;
    }
    
    /// <summary>
    /// Initialize the WebView component for displaying privacy policy
    /// </summary>
    private void InitializeWebView()
    {
        // Remove any existing Scroll View components
        RemoveScrollViewComponents();
        
        // Create WebView GameObject if it doesn't exist
        GameObject webViewGO = new GameObject("PrivacyPolicyWebView");
        webViewGO.transform.SetParent(this.transform, false);
        
        // Add WebViewComponent
        webViewComponent = webViewGO.AddComponent<WebViewComponent>();
        
        // Configure the WebView to load privacy policy
        webViewComponent.streamingAssetsSubFolder = "webview/";
        webViewComponent.filesToLoad = new string[] {
            "privacy_policy.html",
            "UnityGateway.js"
        };
        webViewComponent.transparent = true;
        
        Debug.Log("WebView initialized for privacy policy display");
    }
    
    /// <summary>
    /// Remove Scroll View components since we're replacing them with WebView
    /// </summary>
    private void RemoveScrollViewComponents()
    {
        // Find and remove Scroll View GameObject
        Transform scrollViewTransform = this.transform.Find("Scroll View");
        if (scrollViewTransform != null)
        {
            Debug.Log("Removing Scroll View component - replacing with WebView");
            DestroyImmediate(scrollViewTransform.gameObject);
        }
        
        // Also check for any ScrollRect components in children
        ScrollRect[] scrollRects = GetComponentsInChildren<ScrollRect>();
        foreach (ScrollRect scrollRect in scrollRects)
        {
            if (scrollRect.gameObject.name.Contains("Scroll"))
            {
                Debug.Log("Removing ScrollRect component - replacing with WebView");
                DestroyImmediate(scrollRect.gameObject);
            }
        }
    }

}
