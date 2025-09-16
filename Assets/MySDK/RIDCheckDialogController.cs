using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RIDCheckDialogController : MonoBehaviour
{
    public delegate void Callback();
    public GameObject dialog;

    private Callback validateCallback;

    public TMP_InputField inputFieldName;
    public TMP_InputField inputFieldRIN;

    

    

    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    public void Show()
    {
        dialog.SetActive(true);
    }

    public void Hide()
    {
        dialog.SetActive(false);
    }

    public void SetValidateClickedCallback(Callback cb)
    {
        validateCallback = cb;
    }

    public void OnValidateClicked()
    {
        if (validateCallback != null)
        {
            validateCallback();
        }
    }

    public string GetName()
    {
        return inputFieldName.text.Trim();
    }
    public string GetRIN()
    {
        return inputFieldRIN.text.Trim();
    }
}
