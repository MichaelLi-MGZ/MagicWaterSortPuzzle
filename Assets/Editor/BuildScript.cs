using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

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

        Debug.Log("🚀 Starting iOS build...");
        
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✅ iOS build succeeded!");
            Debug.Log($"Output path: {summary.outputPath}");
            Debug.Log($"Total build time: {summary.totalTime.TotalSeconds:F1}s");
        }
        else
        {
            Debug.LogError($"❌ iOS build failed with result: {summary.result}");
        }
    }
}