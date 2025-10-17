using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TermsDialogWindowController : MonoBehaviour
{
    public GameObject dialog;

    public TMPro.TMP_Text rightText;
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

}
