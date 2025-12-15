using UnityEngine;
using MyGamez.Demo;

/// <summary>
/// Centralized service for handling privacy policy and terms of service dialogs
/// across different scenes without code duplication.
/// </summary>
public class DialogService : MonoBehaviour
{
    private static DialogService _instance;
    public static DialogService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogService>();
                if (_instance == null)
                {
                    GameObject dialogServiceObject = new GameObject("DialogService");
                    _instance = dialogServiceObject.AddComponent<DialogService>();
                    DontDestroyOnLoad(dialogServiceObject);
                }
            }
            return _instance;
        }
    }

    private DemoUIController _demoUIController;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the DemoUIController reference. Should be called from UIManager or GameManager.
    /// </summary>
    /// <param name="demoUIController">The DemoUIController instance</param>
    public void SetDemoUIController(DemoUIController demoUIController)
    {
        _demoUIController = demoUIController;
    }

    /// <summary>
    /// Shows the privacy policy and terms of service dialog.
    /// This method delegates to the DemoUIController's implementation.
    /// </summary>
    public void ShowPrivacyPolicyAndTosDialog()
    {
        if (_demoUIController != null)
        {
            _demoUIController.ShowPrivacyPolicyAndTosDialog();
        }
        else
        {
            Debug.LogError("DialogService: DemoUIController not set. Cannot show privacy policy dialog.");
        }
    }

    /// <summary>
    /// Gets the DemoUIController instance for direct access if needed.
    /// </summary>
    /// <returns>The DemoUIController instance or null if not set</returns>
    public DemoUIController GetDemoUIController()
    {
        return _demoUIController;
    }
}

