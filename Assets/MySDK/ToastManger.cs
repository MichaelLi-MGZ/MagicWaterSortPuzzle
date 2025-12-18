using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [SerializeField] private ToastMessage toastUI;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Show(string msg, float duration = 1f)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowAndroidToast(msg, duration);
#else
        toastUI.Show(msg, duration);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void ShowAndroidToast(string msg, float duration)
    {
        try
        {
            using (AndroidJavaClass unityPlayer =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity =
                    unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                if (activity == null)
                    return;

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    using (AndroidJavaClass toastClass =
                        new AndroidJavaClass("android.widget.Toast"))
                    {
                        int toastLength = duration > 2f
                            ? toastClass.GetStatic<int>("LENGTH_LONG")
                            : toastClass.GetStatic<int>("LENGTH_SHORT");

                        AndroidJavaObject toast =
                            toastClass.CallStatic<AndroidJavaObject>(
                                "makeText",
                                activity,
                                msg,
                                toastLength
                            );

                        toast.Call("show");
                    }
                }));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Android Toast failed: {e.Message}");
        }
    }
#endif
}