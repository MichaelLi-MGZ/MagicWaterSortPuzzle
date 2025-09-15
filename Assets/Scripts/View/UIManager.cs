using System;
using System.Collections;
using System.Collections.Generic;
using Crystal;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameView gameView;

    public FinishView finishView;

    public ShopView shopView;

    public WarningView warningView;

    public WarningView bonusLevelView;

    public WarningView hintView;

    public ProfileView profileView;

    SafeArea.SimDevice[] Sims;

    int SimIdx;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitView()
    {
        /*
        Sims = (SafeArea.SimDevice[])Enum.GetValues(typeof(SafeArea.SimDevice));
        ToggleSafeArea();
        */

        gameView.InitView();
        shopView.InitView();

        profileView.InitView();
    }

    /// <summary>
    /// Toggle the safe area simulation device.
    /// </summary>
    void ToggleSafeArea()
    {
        SimIdx++;

        if (SimIdx >= Sims.Length)
            SimIdx = 0;

        SafeArea.Sim = Sims[SimIdx];
        //Debug.LogFormat("Switched to sim device {0} with debug key '{1}'", Sims[SimIdx], KeySafeArea);
    }
}
