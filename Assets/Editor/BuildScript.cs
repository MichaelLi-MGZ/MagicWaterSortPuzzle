using UnityEditor;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build iOS")]
    public static void BuildiOS()
    {
        string path = "/Users/zm/Downloads/MagicWater-output/IOS";
        string[] scenes = {
            "Assets/Scenes/PPAndRID.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Game.unity",
            "Assets/Scenes/LevelEditor.unity",
            "Assets/Scenes/Splash.unity"
        };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("✅ iOS build finished!");
    }
}