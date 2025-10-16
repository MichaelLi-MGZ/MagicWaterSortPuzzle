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
            options = BuildOptions.AcceptExternalModificationsToPlayer // ✅ 保留现有 Xcode 工程
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✅ iOS build succeeded!");
            Debug.Log($"Output path: {summary.outputPath}");
            Debug.Log($"Total build time: {summary.totalTime.TotalSeconds:F1}s");
            EditorApplication.Exit(0); // ✅ 自动退出 Unity（成功）
        }
        else
        {
            Debug.LogError($"❌ iOS build failed with result: {summary.result}");
            EditorApplication.Exit(1); // ❌ 自动退出 Unity（失败）
        }
    }
}