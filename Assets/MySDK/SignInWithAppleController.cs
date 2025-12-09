using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controller for the Apple Sign-In button
/// </summary>
public class SignInWithAppleController : MonoBehaviour
{
    /// <summary>
    /// Called when Apple Sign-In button is clicked
    /// </summary>
    public void OnButtonClicked()
    {
        // Find DemoUIController and call its Apple Sign-In method
        MyGamez.Demo.DemoUIController demoController = FindObjectOfType<MyGamez.Demo.DemoUIController>();
        if (demoController != null)
        {
            demoController.OnAppleSignInButtonClicked();
        }
        else
        {
            Debug.LogError("[SignInWithAppleController] DemoUIController not found");
        }
    }
}