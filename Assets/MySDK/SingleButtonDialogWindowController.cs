using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SingleButtonDialogWindowController : MonoBehaviour
{
    public delegate void Callback();
    public GameObject dialog;


    private Callback leftCallback;
    public TMPro.TMP_Text leftText;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text msgText;

    
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

    public void setLeftText(string text)
    {
        leftText.text = text;
    }

    public void setTitleText(string text)
    {
        titleText.text = text;
    }

    public void setMessageText(string text)
    {
        msgText.text = text;
    }

    public void setLeftCallback(Callback cb)
    {
        leftCallback = cb;
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
