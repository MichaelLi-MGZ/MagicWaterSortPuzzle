using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DialogWindowController : MonoBehaviour, IPointerClickHandler
{
    public delegate void Callback();
    public GameObject dialog;

    private Callback rightCallback;
    private Callback leftCallback;
    public TMPro.TMP_Text rightText;
    public TMPro.TMP_Text leftText;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text msgText;
    public UnityEngine.UI.Button btnRight;
    
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

    public void setRightButtonActive(bool active)
    {
        btnRight.gameObject.SetActive(active);
    }
    public void setLeftText(string text)
    {
        leftText.text = text;
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

    public void setRightCallback(Callback cb)
    {
        rightCallback = cb;
    }

    public void setLeftCallback(Callback cb)
    {
        leftCallback = cb;
    }

    public void onRightClicked()
    {
        Debug.Log("right clicked");
        if (rightCallback != null)
        {
            Debug.Log("right click callback not null");
            rightCallback();
        }
        else{
            Debug.Log("right click callback is null");
        }
    }

    public void onLeftClicked()
    {
        Debug.Log("left clicked");
        if (leftCallback != null)
        {
            Debug.Log("left click callback not null");
            leftCallback();
        }
        else{
            Debug.Log("left click callback is null");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        Camera cam = null;
        Canvas canvas = msgText.canvas;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(msgText, eventData.position, cam);
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
        else{
            Debug.Log("DialogWindowController: OnPointerClick, linkIndex is -1");
        }
    }
    
    /// <summary>
    /// Show the private dialog when "pp" link is clicked
    /// </summary>
    private void ShowPrivateDialog()
    {
        if (privateDialogController != null)
        {
            Debug.Log("ShowPrivateDialog, privateDialogController is not null");
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
            Debug.Log("ShowTermsDialog, termsDialogController is not null");
            termsDialogController.show();
        }
        else
        {
            Debug.LogWarning("TermsDialogController is not assigned!");
        }
    }
}
