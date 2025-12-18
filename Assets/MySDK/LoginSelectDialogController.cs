using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginSelectDialogController : MonoBehaviour, IPointerClickHandler
{

    public delegate void Callback();
    public GameObject dialog;

    private Callback wechatLoginCallback;
    private Callback phoneLoginCallback;
    public UnityEngine.UI.Button wechatLoginButton;
    public UnityEngine.UI.Button phoneLoginButton;
    public UnityEngine.UI.Toggle agreeToggle;
    public TMPro.TMP_Text msgText;
    
    [Header("Private Dialog Integration")]
    public PrivateDialogWindowController privateDialogController;

    [Header("Tos Dialog Integration")]
    public TermsDialogWindowController termsDialogController;
    
    // Start is called before the first frame update
    void Start()
    {
        hide();
    }

    public void show()
    { 
        dialog.SetActive(true);
    }

    public void hide()
    {
        dialog.SetActive(false);
    }

    public void setWechatLoginButtonActive(bool active)
    {
        wechatLoginButton.gameObject.SetActive(active);
    }
    public void setPhoneLoginButtonActive(bool active)
    {
        phoneLoginButton.gameObject.SetActive(active);
    }

    public void setAgreeToggle(bool active)
    {
        agreeToggle.isOn = active;
    }

    public void setWechatLoginCallback(Callback cb)
    {
        wechatLoginCallback = cb;
    }

    public void setPhoneLoginCallback(Callback cb)
    {
        phoneLoginCallback = cb;
    }

    public void onWechatLoginClicked()
    {
        if (!agreeToggle.isOn)
        {
            Debug.Log("agree toggle is not on");
            ToastManager.Instance.Show("请阅读并同意《隐私政策》及《用户协议》");
            return;
        }
        Debug.Log("wechat login clicked");
        if (wechatLoginCallback != null)
        {
            Debug.Log("wechat login callback not null");
            wechatLoginCallback();
        }
        else{
            Debug.Log("wechat login callback is null");
        }
    }

    public void onPhoneLoginClicked()
    {
        if (!agreeToggle.isOn)
        {
            Debug.Log("agree toggle is not on");
            ToastManager.Instance.Show("请阅读并同意《隐私政策》及《用户协议》");
            return;
        }
        Debug.Log("phone login clicked");
        if (phoneLoginCallback != null)
        {
            Debug.Log("phone login callback not null");
            phoneLoginCallback();
        }
        else{
            Debug.Log("phone login callback is null");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(msgText, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = msgText.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            if (linkID == "pp")
            {
                Debug.Log("pp dialog show");
                ShowPrivateDialog();
            }
            else if (linkID == "tos")
            {
                Debug.Log("pp dialog show");
                ShowTermsDialog();
            }
        }
    }

    public void onAgreeToggleClicked()
    {
        Debug.Log("agree toggle clicked");
        agreeToggle.isOn = !agreeToggle.isOn;
        if (agreeToggle.isOn)
        {
            Debug.Log("agree toggle is on");
        }
        else
        {
            Debug.Log("agree toggle is off");
        }
    }
    
    /// <summary>
    /// Show the private dialog when "pp" link is clicked
    /// </summary>
    private void ShowPrivateDialog()
    {
        if (privateDialogController != null)
        {
            privateDialogController.show();
        }
        else
        {
            Debug.LogWarning("PrivateDialogController is not assigned!");
        }
    }

    /// <summary>
    /// Show the Tos dialog when "tos" link is clicked
    /// </summary>
    private void ShowTermsDialog()
    {
        if (termsDialogController != null)
        {
            termsDialogController.show();
        }
        else
        {
            Debug.LogWarning("TermsDialogController is not assigned!");
        }
    }


}
