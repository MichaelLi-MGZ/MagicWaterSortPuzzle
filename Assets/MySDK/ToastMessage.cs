using System;
using System.Collections;
using System.Collections.Generic;
using MyGamez.Demo;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    public const int LENGTH_LONG = 1;
    public const int LENGTH_SHORT = 0;

    [Tooltip("Duration of long toast message. Only applicable to non-android version")]
    public const float LONG_DURATION = 5f;
    [Tooltip("Duration of short toast message. Only applicable to non-android version")]
    public const float SHORT_DURATION = 1f;

    private static DemoUIController controller;
    private static GameObject toastMessage;

    public static void SetToastObject(GameObject obj, DemoUIController cntr)
    {
        toastMessage = obj;
        controller = cntr;
    }

    public static void Show(string text, int length = LENGTH_SHORT)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, text, length);
                toastObject.Call("show");
            }));
        }
    }
#else
        // Demo toastmessage for non-android
        if (toastMessage == null)
        {
            return;
        }
        Text txt = toastMessage.GetComponentInChildren<Text>();
        txt.text = text;
        // TODO: fade in / fade out?
        toastMessage.SetActive(true);
        ToastMessage msg = (ToastMessage)controller.gameObject.AddComponent(typeof(ToastMessage));
        msg.Invoke(nameof(Hide), length == LENGTH_SHORT ? SHORT_DURATION : LONG_DURATION);
        msg.StartCoroutine(msg.ExecuteAfter(length == LENGTH_SHORT ? SHORT_DURATION : LONG_DURATION, msg.Hide));
    }

    private IEnumerator ExecuteAfter(float time, Action task)
    {
        yield return new WaitForSeconds(time);
        task();
    }

    private void Hide()
    {
        toastMessage.SetActive(false);
        Destroy(this);
    }
#endif
}
