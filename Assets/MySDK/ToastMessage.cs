using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine autoHideCoroutine;

    private void Awake()
    {
        // 一开始一定是隐藏的
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(string msg, float duration)
    {
        // 防止连续调用叠协程
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        text.text = msg;
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;

        autoHideCoroutine = StartCoroutine(AutoHide(duration));
    }

    private IEnumerator AutoHide(float duration)
    {
        yield return new WaitForSeconds(duration);
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        autoHideCoroutine = null;
    }
}

