using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildTabletApk
{
    public static void PerformBuild()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0) scenes = new string[] { "Assets/Scenes/SimpleTablet.unity" };
        if (scenes.Length == 0)
            throw new InvalidOperationException("Nenhuma cena habilitada em EditorBuildSettings.");

        string outputDir = Path.GetFullPath("Builds/Android");
        Directory.CreateDirectory(outputDir);

        string outputPath = Path.Combine(outputDir, "BrasilVRController-tablet.apk");

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
            throw new Exception($"Build falhou: {summary.result}. Veja o Editor.log para detalhes.");

        UnityEngine.Debug.Log($"APK gerado em: {outputPath}");
    }
}
