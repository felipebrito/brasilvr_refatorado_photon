using UnityEditor;
using UnityEngine;
using System.IO;

public class AutoBuildVR
{
    [MenuItem("BrasilVR/Build All VR APKs (1 to 4)")]
    public static void BuildAllAPKs()
    {
        string buildFolder = Path.Combine(Application.dataPath, "../Builds");
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string originalId = PlayerSettings.applicationIdentifier;

        string[] scenes = { "Assets/Scenes/2.unity" }; // Cenas ativas no build settings

        for (int i = 1; i <= 4; i++)
        {
            string packageName = "com.brasilvr" + i;
            string apkName = "BrasilVR_Oculus_Player_" + i + ".apk";
            string buildPath = Path.Combine(buildFolder, apkName);

            Debug.Log($"Building APK for Player {i} with package: {packageName}");

            PlayerSettings.applicationIdentifier = packageName;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Successfully built {apkName}");
            }
            else
            {
                Debug.LogError($"Failed to build {apkName}");
            }
        }

        // Restore original
        PlayerSettings.applicationIdentifier = originalId;
        Debug.Log("Finished building all APKs!");
        
        // Revelar pasta de builds
        EditorUtility.RevealInFinder(buildFolder);
    }
}
