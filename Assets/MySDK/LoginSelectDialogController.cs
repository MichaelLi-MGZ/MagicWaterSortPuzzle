using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginSelectDialogController : MonoBehaviour
{

    public GameObject dialog;    

    

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


}
