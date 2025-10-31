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
        
        // Check if path exists and has a valid Xcode project structure
        bool canAppend = false;
        if (System.IO.Directory.Exists(path))
        {
            // Check if it contains an Xcode project (has .xcodeproj or .xcworkspace)
            string[] xcodeProjects = System.IO.Directory.GetDirectories(path, "*.xcodeproj");
            string[] xcodeWorkspaces = System.IO.Directory.GetDirectories(path, "*.xcworkspace");
            canAppend = (xcodeProjects.Length > 0 || xcodeWorkspaces.Length > 0);
            Debug.Log($"📁 Build path exists: {path}, Can append: {canAppend}");
            
            if (!canAppend)
            {
                Debug.LogWarning($"⚠️ Path exists but no Xcode project found. Will do clean build.");
            }
        }
        else
        {
            Debug.Log($"📁 Build path does not exist. Will create new: {path}");
        }
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.iOS,
            options = canAppend ? BuildOptions.AcceptExternalModificationsToPlayer : BuildOptions.None
        };

        int exitCode = 1; // default to failure unless proven otherwise
        try
        {
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ iOS build succeeded!");
                Debug.Log($"Output path: {summary.outputPath}");
                Debug.Log($"Total build time: {summary.totalTime.TotalSeconds:F1}s");
                exitCode = 0;
            }
            else
            {
                Debug.LogError($"❌ iOS build failed with result: {summary.result}");
                if (report != null && report.steps != null)
                {
                    foreach (var step in report.steps)
                    {
                        if (step.messages != null)
                        {
                            foreach (var msg in step.messages)
                            {
                                if (msg.type == LogType.Error || msg.type == LogType.Exception)
                                {
                                    Debug.LogError($"Build error: {msg.content}");
                                }
                            }
                        }
                    }
                }
                exitCode = 1;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Build exception: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            exitCode = 1;
        }
        finally
        {
            // Flush all logs before exiting
            System.Console.Out.Flush();
            System.Console.Error.Flush();
            
            Debug.Log($"🛑 Exiting Unity batchmode with exit code: {exitCode}");
            
            // Force immediate exit in batch mode
            // EditorApplication.Exit() might not work reliably, so use System.Environment.Exit() as fallback
            try
            {
                EditorApplication.Exit(exitCode);
            }
            catch (System.Exception)
            {
                // If EditorApplication.Exit fails, force exit with System.Environment.Exit
                System.Environment.Exit(exitCode);
            }
            
            // If we're still here, force exit
            System.Threading.Thread.Sleep(100); // Give EditorApplication.Exit time to work
            System.Environment.Exit(exitCode);
        }
    }
}