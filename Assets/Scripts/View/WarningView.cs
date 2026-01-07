using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WarningView : BaseView
{
    public TextMeshProUGUI warningTxt;

    // When true, confirm button will execute a callback (e.g. purchase).
    // When false, confirm button just closes the view (same as close button).
    private Action confirmCallback;

    public override void InitView()
    {
        
    }

    public override void Start()
    {
       
    }

    public override void Update()
    {
       
    }

    public override void ShowView()
    {
        Debug.Log("WarningView: ShowView, no content");
        confirmCallback = null;
        AudioManager.instance.waterFull.Play();
        base.ShowView();
    }

    public override void ShowView(string content)
    {
        Debug.Log("WarningView: ShowView, content: " + content);
        confirmCallback = null;
        AudioManager.instance.waterFull.Play();
        base.ShowView(content);
        warningTxt.text = content;
    }

    public void ShowView(string content, Action onConfirm)
    {
        Debug.Log("WarningView: ShowView, onConfirm is not null? " + (onConfirm != null));
        confirmCallback = onConfirm;
        AudioManager.instance.waterFull.Play();
        base.ShowView();
        warningTxt.text = content;
    }

    public override void HideView()
    {
        base.HideView();
        AudioManager.instance.clickBtn.Play();
    }

    // Hook this to the bottom confirm button in the editor
    public void Confirm()
    {

        // In both modes, confirm will close the view after handling any callback
        HideView();
        
        Debug.Log("WarningView: Confirm, confirmCallback is not null? " + (confirmCallback != null));
        if (confirmCallback != null)
        {
            confirmCallback.Invoke();
            Debug.Log("WarningView: Confirm, confirmCallback invoked");
            confirmCallback = null;
        }


    }

}
