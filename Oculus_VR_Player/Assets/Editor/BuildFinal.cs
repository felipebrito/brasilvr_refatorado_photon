using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public static class BuildFinal
{
    public static void BuildAll()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string codePath = "Assets/Scripts/UserStatusSend.cs";
        string originalCode = File.ReadAllText(codePath);

        // Ensure both XR Loaders are assigned exactly like the original working build
        EnsureXRLoaders();

        // Ensure IL2CPP and ARM64
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        // Exact settings from BuildOculosAndroid.cs
        PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan });
        
        for (int i = 1; i <= 4; i++)
        {
            string pkgName = $"com.Vortex.BrasilVR{i}";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, pkgName);

            string outputFullPath = Path.Combine(projectRoot, $"Builds/BrasilVR{i}.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFullPath));

            // Set Slot
            string newCode = System.Text.RegularExpressions.Regex.Replace(originalCode, @"int slotIndex = \d+; // FORCANDO PARA PLAYER \d+ \(Slot \d+\)", $"int slotIndex = {i-1}; // FORCANDO PARA PLAYER {i} (Slot {i-1})");
            File.WriteAllText(codePath, newCode);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/2.unity" },
                locationPathName = outputFullPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging // Keep exactly as BuildOculosAndroid.cs
            };

            Debug.Log($"Building {pkgName}...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Android build failed for {pkgName}: {report.summary.result}");
                File.WriteAllText(codePath, originalCode); // restore
                EditorApplication.Exit(1);
                return;
            }
        }
        
        File.WriteAllText(codePath, originalCode); // restore
        Debug.Log("All builds succeeded!");
        EditorApplication.Exit(0);
    }

    private static void EnsureXRLoaders()
    {
        XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
        EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
        if (buildTargetSettings != null)
        {
            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (settings != null && settings.AssignedSettings != null)
            {
                string oculusLoader = "Unity.XR.Oculus.OculusLoader";
                string openXRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader";
                try { XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, oculusLoader, BuildTargetGroup.Android); } catch {}
                try { XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, openXRLoader, BuildTargetGroup.Android); } catch {}
                EditorUtility.SetDirty(settings);
                EditorUtility.SetDirty(settings.AssignedSettings);
            }
        }
    }
}
