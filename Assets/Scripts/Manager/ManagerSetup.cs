using UnityEngine;

public class ManagerSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SetupManagers()
    {
        // Create MySDK Manager
        if (MySDKManager.Instance == null)
        {
            GameObject mySDKManager = new GameObject("MySDKManager");
            mySDKManager.AddComponent<MySDKManager>();
            DontDestroyOnLoad(mySDKManager);
        }

        // Create Payment Manager
        if (PaymentManager.Instance == null)
        {
            GameObject paymentManager = new GameObject("PaymentManager");
            paymentManager.AddComponent<PaymentManager>();
            DontDestroyOnLoad(paymentManager);
        }

        // Create Advertising Manager
        if (AdvertisingManager.Instance == null)
        {
            GameObject advertisingManager = new GameObject("AdvertisingManager");
            advertisingManager.AddComponent<AdvertisingManager>();
            DontDestroyOnLoad(advertisingManager);
        }

        // Create Anti-Addiction Manager
        if (AntiAddictionManager.Instance == null)
        {
            GameObject antiAddictionManager = new GameObject("AntiAddictionManager");
            antiAddictionManager.AddComponent<AntiAddictionManager>();
            DontDestroyOnLoad(antiAddictionManager);
        }

        // Note: AudioManager is already present in the Game scene with proper AudioSource components
        // No need to create it programmatically
    }
}