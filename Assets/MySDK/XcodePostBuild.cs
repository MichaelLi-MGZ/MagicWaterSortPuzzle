#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class XcodePostBuild : MonoBehaviour
{
    [PostProcessBuildAttribute]
    public static void OnPostprocessBuild(UnityEditor.BuildTarget target, string path)
    {
#if UNITY_IOS
        var projectPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        string targetName = project.GetUnityFrameworkTargetGuid();
        //Debug.Log("XcodePostBuild::OnPostprocessBuild() targetName=" + targetName); // DEBUG
        project.AddFrameworkToProject(targetName, "libsqlite3.tbd", false);
        project.WriteToFile(projectPath);
#endif

    }
}
#endif // UNITY_EDITOR
