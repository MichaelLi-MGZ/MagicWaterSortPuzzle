using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogWindowController : MonoBehaviour
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
}
